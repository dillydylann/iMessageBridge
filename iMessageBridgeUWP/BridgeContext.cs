#pragma warning disable CS4014 

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace DylanBriedis.iMessageBridge
{
    /// <summary>
    /// Provides the API to an iMessage Bridge server.
    /// </summary>
    public sealed class BridgeContext : IDisposable
    {
        /// <summary>
        /// Constucts a bridge context with a hostname or an IP address to connect to.
        /// </summary>
        /// <param name="host">A hostname or an IP address.</param>
        public BridgeContext(string host)
        {
            Hostname = host;
        }

        /// <summary>
        /// The hostname that this client will connect to.
        /// </summary>
        public string Hostname { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            client.Dispose();
        }

        Dictionary<int, Recipient> recipients = new Dictionary<int, Recipient>();
        /// <summary>
        /// The list of recipients on the bridge.
        /// </summary>
        public IReadOnlyDictionary<int, Recipient> Recipients { get { return recipients; } }
        Dictionary<int, Conversation> conversations = new Dictionary<int, Conversation>();
        /// <summary>
        /// The list of conversations on the bridge.
        /// </summary>
        public IReadOnlyDictionary<int, Conversation> Conversations { get { return conversations; } }
        Dictionary<int, Message> messages = new Dictionary<int, Message>();
        /// <summary>
        /// The list of messages on the bridge.
        /// </summary>
        public IReadOnlyDictionary<int, Message> Messages { get { return messages; } }
        Dictionary<int, Attachment> attachments = new Dictionary<int, Attachment>();
        /// <summary>
        /// The list of attachments on the bridge.
        /// </summary>
        public IReadOnlyDictionary<int, Attachment> Attachments { get { return attachments; } }

        /// <summary>
        /// The credentials for logging in to a authentication enabled bridge.
        /// </summary>
        public Credentials AuthenticationCredentials { get; set; }

        /// <summary>
        /// Gets a value indicating whether if the bridge needs authentication or not.
        /// </summary>
        /// <returns>Returns true if the user needs to be authenticated.</returns>
        public IAsyncOperation<bool> AuthenticationIsRequiredAsync()
        {
            return AuthenticationIsRequiredAsyncInternal().AsAsyncOperation();
        }

        async Task<bool> AuthenticationIsRequiredAsyncInternal()
        {
            HttpWebRequest req = WebRequest.CreateHttp("http://" + Hostname + ":9080/test");
            try
            {
                (await req.GetResponseAsync()).Dispose();
                return false;
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                    using (ex.Response)
                        return (ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.Unauthorized;
                else
                    throw;
            }
        }

        /// <summary>
        /// Validate the currently set credentials.
        /// </summary>
        /// <returns>Returns true if the user is authenticated.</returns>
        public IAsyncOperation<bool> ValidateCredentialsAsync()
        {
            return ValidateCredentialsAsyncInternal().AsAsyncOperation();
        }

        async Task<bool> ValidateCredentialsAsyncInternal()
        {
            HttpWebRequest req = WebRequest.CreateHttp("http://" + Hostname + ":9080/test");
            req.Credentials = AuthenticationCredentials.ToNetworkCredentials();
            try
            {
                (await req.GetResponseAsync()).Dispose();
                return true;
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                    using (ex.Response)
                        return (ex.Response as HttpWebResponse).StatusCode != HttpStatusCode.Unauthorized;
                else
                    throw;
            }
        }

        async Task<JObject> GetJSONRequest(string path)
        {
            HttpWebRequest req = WebRequest.CreateHttp("http://" + Hostname + ":9080/" + path);
            if (AuthenticationCredentials != null)
                req.Credentials = AuthenticationCredentials.ToNetworkCredentials();
            HttpWebResponse res;
            try
            {
                res = await req.GetResponseAsync() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                    res = ex.Response as HttpWebResponse;
                else
                    throw;
            }
            using (StreamReader sr = new StreamReader(res.GetResponseStream()))
                return JObject.Parse(sr.ReadToEnd());
        }

        bool inited = false;
        /// <summary>
        /// Initializes the context by getting all the recipients, conversations, and messages from the bridge.
        /// </summary>
        public IAsyncAction InitAsync()
        {
            return InitAsyncInternal().AsAsyncAction();
        }

        async Task InitAsyncInternal()
        {
            if (inited)
                throw new InvalidOperationException("Please don't call Init() more than once!");
            JObject recipientsJson = await GetJSONRequest("recipients");
            foreach (JObject d in recipientsJson["data"])
                recipients.Add((int)d["id"], JsonToObject(ObjectType.Recipient, d) as Recipient);
            JObject attachmentsJson = await GetJSONRequest("attachments");
            foreach (JObject d in attachmentsJson["data"])
                attachments.Add((int)d["id"], JsonToObject(ObjectType.Attachment, d) as Attachment);
            JObject messagesJson = await GetJSONRequest("messages");
            foreach (JObject d in messagesJson["data"])
                messages.Add((int)d["id"], JsonToObject(ObjectType.Message, d) as Message);
            JObject conversationsJson = await GetJSONRequest("conversations");
            foreach (JObject d in conversationsJson["data"])
                conversations.Add((int)d["id"], JsonToObject(ObjectType.Conversation, d) as Conversation);
            inited = true;
        }

        IIdObject JsonToObject(ObjectType objectType, JObject json)
        {
            switch (objectType)
            {
                case ObjectType.Recipient:
                    return new Recipient()
                    {
                        Id = (int)json["id"],
                        Address = json["address"].ToString(),
                        Country = json["country"].ToString(),
                        ServiceType = (ServiceType)Enum.Parse(typeof(ServiceType), json["serviceType"].ToString()),
                        Name = json["name"].ToString(),
                        HasPicture = (bool)json["hasPicture"]
                    };
                case ObjectType.Conversation:
                    {
                        Conversation c = new Conversation()
                        {
                            Id = (int)json["id"],
                            Name = json["name"].ToString(),
                            Recipients = new List<Recipient>(),
                            Messages = new List<Message>(),
                            ServiceType = (ServiceType)Enum.Parse(typeof(ServiceType), json["serviceType"].ToString()),
                            DisplayName = json["displayName"].ToString()
                        };
                        foreach (int r in json["recipients"])
                            c.Recipients.Add(recipients[r]);
                        foreach (int m in json["messages"])
                            c.Messages.Add(messages[m]);
                        return c;
                    }
                case ObjectType.Message:
                    {
                        Message m = new Message()
                        {
                            Id = (int)json["id"],
                            From = json["from"].Type != JTokenType.Null ? recipients[(int)json["from"]] : null,
                            FromMe = (bool)json["fromMe"],
                            ServiceType = (ServiceType)Enum.Parse(typeof(ServiceType), json["serviceType"].ToString()),
                            Date = DateTime.Parse(json["date"].ToString()),
                            DateRead = json["dateRead"].Type != JTokenType.Null ? DateTime.Parse(json["dateRead"].ToString()) : (DateTime?)null,
                            DateDelivered = json["dateDelivered"].Type != JTokenType.Null ? DateTime.Parse(json["dateDelivered"].ToString()) : (DateTime?)null,
                            HasRead = (bool)json["hasRead"],
                            HasDelivered = (bool)json["hasDelivered"],
                            HasSent = (bool)json["hasSent"],
                            Subject = json["subject"].ToString(),
                            Text = json["text"].ToString(),
                            Attachments = new List<Attachment>()
                        };
                        foreach (int a in json["attachments"])
                            m.Attachments.Add(attachments[a]);
                        return m;
                    }
                case ObjectType.Attachment:
                    return new Attachment()
                    {
                        Id = (int)json["id"],
                        FullFileName = json["fullFileName"].ToString(),
                        FileName = json["fileName"].ToString(),
                        CreatedDate = DateTime.Parse(json["createdDate"].ToString()),
                        MimeType = json["mimeType"].ToString(),
                        TotalBytes = (long)json["totalBytes"]
                    };
            }
            return null;
        }

        /// <summary>
        /// Downloads a recipient's contact picture from the bridge.
        /// </summary>
        /// <param name="id">The recipient's id.</param>
        /// <returns>The picture's stream from the bridge.</returns>
        public IAsyncOperation<IInputStream> DownloadRecipientPictureAsync(int id)
        {
            return DownloadRecipientPictureAsyncInternal(id).AsAsyncOperation();
        }

        async Task<IInputStream> DownloadRecipientPictureAsyncInternal(int id)
        {
            HttpWebRequest req = WebRequest.CreateHttp("http://" + Hostname + ":9080/recipientpicture?id=" + id);
            if (AuthenticationCredentials != null)
                req.Credentials = AuthenticationCredentials.ToNetworkCredentials();
            return (await req.GetResponseAsync()).GetResponseStream().AsInputStream();
        }

        /// <summary>
        /// Downloads an attachment from the bridge.
        /// </summary>
        /// <param name="id">The attachment's id.</param>
        /// <returns>The attachment's stream from the bridge.</returns>
        public IAsyncOperation<IInputStream> DownloadAttachmentAsync(int id)
        {
            return DownloadAttachmentAsyncInternal(id).AsAsyncOperation();
        }

        async Task<IInputStream> DownloadAttachmentAsyncInternal(int id)
        {
            HttpWebRequest req = WebRequest.CreateHttp("http://" + Hostname + ":9080/attachment?id=" + id);
            if (AuthenticationCredentials != null)
                req.Credentials = AuthenticationCredentials.ToNetworkCredentials();
            return (await req.GetResponseAsync()).GetResponseStream().AsInputStream();
        }

        /// <summary>
        /// Sends a text message to a recipient.
        /// </summary>
        /// <param name="recipient">The recipient you want it to send to.</param>
        /// <param name="text">The message to send.</param>
        public IAsyncAction SendMessageAsync(Recipient recipient, string text)
        {
            return SendMessageAsyncInternal(recipient, text).AsAsyncAction();
        }

        async Task SendMessageAsyncInternal(Recipient recipient, string text)
        {
            await SendMessageAsyncInternal(recipient.Address, text, recipient.ServiceType == ServiceType.SMS);
        }

        /// <summary>
        /// Sends a text message to a recipient.
        /// </summary>
        /// <param name="recipientAddress">The recipient's phone number or email.</param>
        /// <param name="text">The message to send.</param>
        /// <param name="sendAsSMS">Sets whether to send as a SMS or an iMessage.</param>
        public IAsyncAction SendMessageAsync(string recipientAddress, string text, bool sendAsSMS)
        {
            return SendMessageAsyncInternal(recipientAddress, text, sendAsSMS).AsAsyncAction();
        }

        async Task SendMessageAsyncInternal(string recipientAddress, string text, bool sendAsSMS)
        {
            HttpWebRequest req = WebRequest.CreateHttp("http://" + Hostname + ":9080/send");
            req.Method = "POST";
            if (AuthenticationCredentials != null)
                req.Credentials = AuthenticationCredentials.ToNetworkCredentials();
            using (StreamWriter sr = new StreamWriter(await req.GetRequestStreamAsync()))
                sr.Write("recipient=" + Uri.EscapeDataString(recipientAddress) + "&text=" + Uri.EscapeDataString(text) + (sendAsSMS ? "&sms=1" : ""));
            (await req.GetResponseAsync()).Dispose();
        }

        MessageWebSocket client;
        DataWriter clientWriter;

        /// <summary>
        /// Starts listening for bridge events. All events will get called to StreamUpdate.
        /// </summary>
        public IAsyncAction StartStreamAsync()
        {
            return StartStreamAsyncInternal().AsAsyncAction();
        }

        async Task StartStreamAsyncInternal()
        {
            client = new MessageWebSocket();
            client.MessageReceived += Client_MessageReceived;
            await client.ConnectAsync(new Uri("ws://" + Hostname + ":9081/"));
            clientWriter = new DataWriter(client.OutputStream);
        }
        
        private async void Client_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            using (DataReader reader = args.GetDataReader())
                //try
                {
                    string str = reader.ReadString(reader.UnconsumedBufferLength);
                    if (!(str.StartsWith("\0") && str.EndsWith("\0")))
                    {
                        JObject json = JObject.Parse(str);
                        switch (json["event"].ToString())
                        {
                            case "auth":
                                if ((bool)json["needsAuth"])
                                {
                                    string login = string.Format("{{\"username\":\"{0}\",\"password\":\"{1}\"}}", AuthenticationCredentials.UserName.Replace("\"", "\\\""), AuthenticationCredentials.Password.Replace("\"", "\\\""));
                                    clientWriter.WriteString(login);
                                    await clientWriter.FlushAsync();
                                }
                                break;
                            case "update":
                                EventType eventType = (EventType)Enum.Parse(typeof(EventType), json["eventType"].ToString());
                                ObjectType objectType = (ObjectType)Enum.Parse(typeof(ObjectType), json["objectType"].ToString());
                                IIdObject obj = JsonToObject(objectType, (JObject)json["obj"]);
                                switch (eventType)
                                {
                                    case EventType.Add:
                                        switch (objectType)
                                        {
                                            case ObjectType.Recipient:
                                                recipients.Add(obj.Id, obj as Recipient);
                                                break;
                                            case ObjectType.Conversation:
                                                conversations.Add(obj.Id, obj as Conversation);
                                                break;
                                            case ObjectType.Message:
                                                messages.Add(obj.Id, obj as Message);
                                                break;
                                            case ObjectType.Attachment:
                                                attachments.Add(obj.Id, obj as Attachment);
                                                break;
                                        }
                                        break;
                                    case EventType.Update:
                                        switch (objectType)
                                        {
                                            case ObjectType.Recipient:
                                                UpdateObject(recipients[obj.Id], obj);
                                                break;
                                            case ObjectType.Conversation:
                                                UpdateObject(conversations[obj.Id], obj);
                                                break;
                                            case ObjectType.Message:
                                                UpdateObject(messages[obj.Id], obj);
                                                break;
                                            case ObjectType.Attachment:
                                                UpdateObject(attachments[obj.Id], obj);
                                                break;
                                        }
                                        break;
                                    case EventType.Remove:
                                        switch (objectType)
                                        {
                                            case ObjectType.Recipient:
                                                recipients.Remove(obj.Id);
                                                break;
                                            case ObjectType.Conversation:
                                                conversations.Remove(obj.Id);
                                                break;
                                            case ObjectType.Message:
                                                messages.Remove(obj.Id);
                                                break;
                                            case ObjectType.Attachment:
                                                attachments.Remove(obj.Id);
                                                break;
                                        }
                                        break;
                                }
                                StreamUpdate?.Invoke(this, new StreamUpdateEventArgs()
                                {
                                    ObjectType = objectType,
                                    EventType = eventType,
                                    Object = obj
                                });
                                break;
                        }
                    }
                }
                //catch (Exception ex)
                //{
                //    StopStream();
                //    StreamUpdate?.Invoke(this, new StreamUpdateEventArgs() { Error = ex });
                //}
        }

        // Keep an object referenced with the original.
        void UpdateObject(object origObj, object newObj)
        {
            var props = origObj.GetType().GetProperties();
            for (int i = 0; i < props.Length; i++)
                props[i].SetValue(origObj, props[i].GetValue(newObj));
        }

        /// <summary>
        /// Occurs when a stream update occurs.
        /// </summary>
        public event StreamUpdateEventHandler StreamUpdate;

        /// <summary>
        /// Stops listening for bridge events.
        /// </summary>
        public void StopStream()
        {
            clientWriter.Dispose();
            client.Close(1000, "Closed");
            client.Dispose();
        }
    }

    /// <summary>
    /// Represents the method that will handle an event for stream updates.
    /// </summary>
    /// <param name="sender">The source of the event. This would reference the current bridge context.</param>
    /// <param name="e">The event's arguments containing update infomation.</param>
    public delegate void StreamUpdateEventHandler(object sender, StreamUpdateEventArgs e);

    /// <summary>
    /// Represents event data for stream updates.
    /// </summary>
    public sealed class StreamUpdateEventArgs
    {
        /// <summary>
        /// Gets what type of object was updated.
        /// </summary>
        public ObjectType ObjectType { get; internal set; }
        /// <summary>
        /// Gets what type of event was invoked.
        /// </summary>
        public EventType EventType { get; internal set; }
        /// <summary>
        /// The updated object.
        /// </summary>
        public IIdObject Object { get; internal set; }
        /// <summary>
        /// Gets an exception object when the update errors.
        /// </summary>
        public Exception Error { get; internal set; }
    }
}
