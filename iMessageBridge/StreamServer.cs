using MonoMac.Foundation;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using vtortola.WebSockets;
using vtortola.WebSockets.Rfc6455;

namespace DylanBriedis.iMessageBridge
{
    internal static class StreamServer
    {
        static WebSocketListener server;
        static List<WebSocket> webSockets = new List<WebSocket>();

        static bool running = false;
        public static void Start()
        {
            Logging.Log("[StreamServer] Starting server...");
            server = new WebSocketListener(new IPEndPoint(IPAddress.Any, 9081));
            server.Standards.RegisterStandard(new WebSocketFactoryRfc6455(server));
            running = true;
            server.Start();
            new Thread(async () =>
            {
                while (running)
                {
                    WebSocket ws = await server.AcceptWebSocketAsync(CancellationToken.None);
                    Logging.Log("[StreamServer] Connection from " + ws.RemoteEndpoint.Address.ToString());

                    if (NSUserDefaults.StandardUserDefaults.BoolForKey("NotificationsUserConnects"))
                        NSUserNotificationCenter.DefaultUserNotificationCenter.DeliverNotification(new NSUserNotification() { Title = "Client connected", Subtitle = ws.RemoteEndpoint.Address.ToString() });

                    ThreadPool.QueueUserWorkItem((o) =>
                    {
                        if (NSUserDefaults.StandardUserDefaults.BoolForKey("ServerAuthentication"))
                        {
                            ws.WriteString("{\"event\":\"auth\",\"needsAuth\":true}");
                            JObject json = JObject.Parse(ws.ReadString());
                            if (json["username"].ToString() == "user" && json["password"].ToString() == NSUserDefaults.StandardUserDefaults.StringForKey("ServerPassword"))
                            {
                                Logging.Log("[StreamServer] Successful login from " + ws.RemoteEndpoint.Address.ToString());
                                webSockets.Add(ws);
                            }
                            else
                                using (ws)
                                {
                                    Logging.Log("[StreamServer] Incorrect login from " + ws.RemoteEndpoint.Address.ToString());
                                    ws.WriteString("{ \"event\": \"close\" }");
                                }
                        }
                        else
                        {
                            ws.WriteString("{\"event\":\"auth\",\"needsAuth\":false}");
                            webSockets.Add(ws);
                        }

                        while (ws.IsConnected)
                            ws.ReadString(); // Keep alive.
                    });
                }
            }).Start();
            DatabaseStore.StoreUpdate += DatabaseStore_StoreUpdate;
            Logging.Log("[StreamServer] Started server");
        }

        private static void DatabaseStore_StoreUpdate(ObjectType updateObjectType, EventType updateEventType, IIdObject obj)
        {
            foreach (WebSocket ws in webSockets)
                if (ws.IsConnected)
                    ws.WriteString(string.Format("{{\"event\":\"update\",\"objectType\":\"{0}\",\"eventType\":\"{1}\",\"obj\":{2}}}", updateObjectType, updateEventType, JSON.FormatJSONObject(obj, false)));
            webSockets.RemoveAll((ws) => !ws.IsConnected);
        }

        public static void Stop()
        {
            Logging.Log("[StreamServer] Stopping server...");
            running = false;
            foreach (WebSocket ws in webSockets)
                using (ws)
                    ws.WriteString("{ \"event\": \"close\" }");
            webSockets.Clear();
            server.Stop();
            server.Dispose();
            Logging.Log("[StreamServer] Stopped server");
        }
    }
}
