using MonoMac.Foundation;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using System.Collections.Specialized;

namespace DylanBriedis.iMessageBridge
{
    internal static class HttpServer
    {
        static HttpListener server;

        public static void Start()
        {
            Logging.Log("[HttpServer] Starting server...");
            server = new HttpListener();
            if (NSUserDefaults.StandardUserDefaults.BoolForKey("ServerAuthentication"))
                server.AuthenticationSchemes = AuthenticationSchemes.Basic;
            server.Prefixes.Add("http://*:9080/");
            running = true;
            server.Start();
            new Thread(() =>
            {
                while (running)
                    try
                    {
                        ThreadPool.QueueUserWorkItem(ContextProcess, server.GetContext());
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("[HttpServer] GetContext error: " + ex.Message);
                    }
            }).Start();
        }

        static bool running = false;
        static void ContextProcess(object o)
        {
            try
            {
                HttpListenerContext context = (HttpListenerContext)o;
                Logging.Log("[HttpServer] Request from " + context.Request.RemoteEndPoint + " : " + context.Request.Url.PathAndQuery);
                context.Response.ContentType = "application/json";
                using (StreamReader sr = new StreamReader(context.Request.InputStream))
                using (StreamWriter sw = new StreamWriter(context.Response.OutputStream))
                    try
                    {
                        if (NSUserDefaults.StandardUserDefaults.BoolForKey("ServerAuthentication"))
                        {
                            HttpListenerBasicIdentity id = (HttpListenerBasicIdentity)context.User.Identity;
                            if (!(id.Name == "user" && id.Password.Length > 0 && id.Password == NSUserDefaults.StandardUserDefaults.StringForKey("ServerPassword")))
                            {
                                context.Response.StatusCode = 401;
                                sw.Write("{\"status\":\"access denied\"}");
                                return;
                            }
                        }
                        NameValueCollection body = null;
                        if (context.Request.HttpMethod == "POST")
                            body = HttpUtility.ParseQueryString(sr.ReadToEnd());
                        switch (context.Request.Url.AbsolutePath)
                        {
                            case "/test":
                                sw.Write("{\"status\":\"ok\"}");
                                break;
                            case "/serverinfo":
                                sw.Write(string.Format("{{\"status\":\"ok\",\"bridgeVersion\":\"{0}\"}}",
                                    ServerInfo.BridgeVersion));
                                break;

                            case "/recipients":
                                sw.Write(JSON.FormatDictionaryResponse(DatabaseStore.Recipients));
                                break;
                            case "/recipientpicture":
                                Recipient recipient = DatabaseStore.Recipients[Convert.ToInt32(context.Request.QueryString["id"])];
                                if (recipient != null)
                                    if (recipient.HasPicture)
                                    {
                                        context.Response.ContentType = "image/jpeg";
                                        context.Response.OutputStream.Write(recipient._picture, 0, recipient._picture.Length);
                                    }
                                    else
                                        context.Response.StatusCode = 404;
                                else
                                    context.Response.StatusCode = 404;
                                break;
                            case "/conversations":
                                sw.Write(JSON.FormatDictionaryResponse(DatabaseStore.Conversations));
                                break;
                            case "/messages":
                                sw.Write(JSON.FormatDictionaryResponse(DatabaseStore.Messages));
                                break;
                            case "/attachments":
                                sw.Write(JSON.FormatDictionaryResponse(DatabaseStore.Attachments));
                                break;
                            case "/attachment":
                                Attachment attachment = DatabaseStore.Attachments[Convert.ToInt32(context.Request.QueryString["id"])];
                                if (attachment != null)
                                    if (File.Exists(attachment.FullFileName))
                                    {
                                        context.Response.ContentType = attachment.MimeType;
                                        context.Response.AddHeader("Content-Disposition", "inline; filename=\"" + attachment.FileName + "\"");
                                        using (Stream fileStream = File.OpenRead(attachment.FullFileName))
                                            fileStream.CopyTo(context.Response.OutputStream);
                                    }
                                    else
                                        context.Response.StatusCode = 404;
                                else
                                    context.Response.StatusCode = 404;
                                break;

                            case "/send":
                                NSAppleScript appleScript;
                                if (string.IsNullOrEmpty(body["sms"]))
                                    // By default messages are sent using iMessage.
                                    appleScript = new NSAppleScript(string.Format(@"
                                        tell application ""Messages""
                                            set serviceID to id of 1st service whose service type = iMessage
                                            send ""{0}"" to buddy ""{1}"" of service id serviceID
                                        end tell", JSON.FormatString(body["text"]), body["recipient"]));
                                else
                                    // If the body has the "sms" parameter, send as SMS.
                                    appleScript = new NSAppleScript(string.Format(@"
                                        tell application ""Messages""
                                            send ""{0}"" to buddy ""{1}"" of service ""SMS""
                                        end tell", JSON.FormatString(body["text"]), body["recipient"]));
                                NSDictionary errorInfo;
                                appleScript.ExecuteAndReturnError(out errorInfo);
                                if (errorInfo == null)
                                    sw.Write("{\"status\":\"ok\"}");
                                else
                                {
                                    context.Response.StatusCode = 400;
                                    sw.Write(string.Format("{{\"status\":\"error\",\"error\":{{\"briefMessage\":\"{0}\",\"message\":\"{1}\",\"number\":{2}}}}}",
                                        JSON.FormatString(errorInfo.ValueForKey(new NSString("NSAppleScriptErrorBriefMessage")).ToString()), JSON.FormatString(errorInfo.ValueForKey(new NSString("NSAppleScriptErrorMessage")).ToString()), JSON.FormatString(errorInfo.ValueForKey(new NSString("NSAppleScriptErrorNumber")).ToString())));
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        context.Response.StatusCode = 500;
                        sw.Write(JSON.Error(ex));
                    }
            }
            catch (Exception ex)
            {
                Logging.Log("[HttpServer] ContextProcess error: " + ex.Message);
            }
        }
        

        public static void Stop()
        {
            Logging.Log("[HttpServer] Stopping server...");
            running = false;
            server.Stop();
            Logging.Log("[HttpServer] Stopped server...");
        }
    }
}
