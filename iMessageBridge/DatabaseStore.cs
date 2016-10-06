using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Timers;
using System.Diagnostics;

namespace DylanBriedis.iMessageBridge
{
    internal static class DatabaseStore
    {
        public static Dictionary<int, Recipient> Recipients = new Dictionary<int, Recipient>();
        public static Dictionary<int, Conversation> Conversations = new Dictionary<int, Conversation>();
        public static Dictionary<int, Message> Messages = new Dictionary<int, Message>();
        public static Dictionary<int, Attachment> Attachments = new Dictionary<int, Attachment>();

        static SqliteConnection dbConnection;
        static Timer updateTimer;
        public static void Init()
        {
            Logging.Log("[DatabaseStore] Initializing database store...");
            foreach (Process proc in Process.GetProcessesByName("IMDPersistenceAgent"))
                // IMDPersistenceAgent locks the database so we need to kill it.
                try
                {
                    Logging.Log("[DatabaseStore] Killing IMDPersistenceAgent...");
                    proc.Kill();
                }
                catch { }
            // Load iMessage's database.
            dbConnection = new SqliteConnection("Data Source=" + Environment.GetEnvironmentVariable("HOME") + "/Library/Messages/chat.db;Version=3;");
            dbConnection.Open();
            Logging.Log("[DatabaseStore] Connected to chat database");
            updateTimer = new Timer(1000);
            updateTimer.Elapsed += (s, e) => UpdateStore(false);
            UpdateStore(true);
            updateTimer.Start();
            Logging.Log("[DatabaseStore] Init done");
        }

        static bool updating = false;
        static void UpdateStore(bool init)
        {
            if (!updating)
                try
                {
                    updating = true;
                    List<int> currentRecipients = new List<int>();
                    SqliteDataReader recipientsReader = new SqliteCommand("SELECT * FROM handle") { Connection = dbConnection }.ExecuteReader();
                    while (recipientsReader.Read())
                    {
                        Recipient recipient;
                        if (!Recipients.ContainsKey(recipientsReader.GetInt32(0)))
                        {
                            Person person = ContactUtils.GetPerson(recipientsReader.GetValue(1).ToString());
                            recipient = new Recipient()
                            {
                                Id = recipientsReader.GetInt32(0),
                                Address = recipientsReader.GetValue(1).ToString(),
                                Country = recipientsReader.GetValue(2).ToString(),
                                ServiceType = (ServiceType)Enum.Parse(typeof(ServiceType), recipientsReader.GetValue(3).ToString()),
                                Name = person.name,
                                HasPicture = person.picture != null,
                                _picture = person.picture
                            };
                            Logging.Log("[DatabaseStore] Added recipient: " + recipient.Id);
                            Recipients.Add(recipient.Id, recipient);
                            if (!init)
                                InvokeStoreUpdate(ObjectType.Recipient, EventType.Add, recipient);
                        }
                        else
                            recipient = Recipients[recipientsReader.GetInt32(0)];
                        currentRecipients.Add(recipient.Id);
                    }
                    if (init)
                        Logging.Log("[DatabaseStore] Found " + Recipients.Count + " recipients");

                    List<int> currentAttachments = new List<int>();
                    SqliteDataReader attachmentsReader = new SqliteCommand("SELECT * FROM attachment") { Connection = dbConnection }.ExecuteReader();
                    while (attachmentsReader.Read())
                    {
                        Attachment attachment;
                        if (!Attachments.ContainsKey(attachmentsReader.GetInt32(0)))
                        {
                            attachment = new Attachment()
                            {
                                Id = attachmentsReader.GetInt32(0),
                                FullFileName = FormatPath(attachmentsReader.GetValue(4).ToString()),
                                FileName = attachmentsReader.GetValue(10).ToString(),
                                CreatedDate = ConvertChatDateFormat(attachmentsReader.GetInt32(2)).Value,
                                MimeType = attachmentsReader.GetValue(6).ToString(),
                                TotalBytes = attachmentsReader.GetInt32(11),
                            };
                            Logging.Log("[DatabaseStore] Added attachment: " + attachment.Id);
                            Attachments.Add(attachmentsReader.GetInt32(0), attachment);
                            if (!init)
                                InvokeStoreUpdate(ObjectType.Attachment, EventType.Add, attachment);
                        }
                        else
                            attachment = Attachments[attachmentsReader.GetInt32(0)];
                        currentAttachments.Add(attachment.Id);
                    }
                    if (init)
                        Logging.Log("[DatabaseStore] Found " + Attachments.Count + " attachments");

                    List<KeyValuePair<int, int>> messagesAttachmentsJoin = new List<KeyValuePair<int, int>>();
                    SqliteDataReader messagesAttachmentsJoinReader = new SqliteCommand("SELECT * FROM message_attachment_join") { Connection = dbConnection }.ExecuteReader();
                    while (messagesAttachmentsJoinReader.Read())
                        messagesAttachmentsJoin.Add(new KeyValuePair<int, int>(messagesAttachmentsJoinReader.GetInt32(0), messagesAttachmentsJoinReader.GetInt32(1)));

                    List<int> currentMessages = new List<int>();
                    SqliteDataReader messagesReader = new SqliteCommand("SELECT * FROM message") { Connection = dbConnection }.ExecuteReader();
                    while (messagesReader.Read())
                    {
                        Message message;
                        bool added = false;
                        bool update = false;
                        if (!Messages.ContainsKey(messagesReader.GetInt32(0)))
                        {
                            message = new Message()
                            {
                                Id = messagesReader.GetInt32(0),
                                Text = messagesReader.GetValue(2).ToString(),
                                Subject = messagesReader.GetValue(6).ToString(),
                                Attachments = new List<Attachment>(),
                                ServiceType = (ServiceType)Enum.Parse(typeof(ServiceType), messagesReader.GetValue(11).ToString()),
                                Date = ConvertChatDateFormat(messagesReader.GetInt32(15)).Value,
                                DateRead = ConvertChatDateFormat(messagesReader.GetInt32(16)),
                                DateDelivered = ConvertChatDateFormat(messagesReader.GetInt32(17)),
                                HasDelivered = messagesReader.GetInt32(18) == 1,
                                FromMe = messagesReader.GetInt32(21) == 1,
                                HasRead = messagesReader.GetInt32(26) == 1,
                                HasSent = messagesReader.GetInt32(28) == 1,
                            };
                            if (messagesReader.GetInt32(5) > 0)
                                message.From = Recipients[messagesReader.GetInt32(5)];
                            added = true;
                        }
                        else
                        {
                            message = Messages[messagesReader.GetInt32(0)];
                            if (message.ServiceType.ToString() != messagesReader.GetValue(11).ToString())
                            {
                                message.ServiceType = (ServiceType)Enum.Parse(typeof(ServiceType), messagesReader.GetValue(11).ToString());
                                update = true;
                            }
                            if (message.HasDelivered != (messagesReader.GetInt32(18) == 1))
                            {
                                message.DateDelivered = ConvertChatDateFormat(messagesReader.GetInt32(17));
                                message.HasDelivered = messagesReader.GetInt32(18) == 1;
                                update = true;
                            }
                            if (message.HasRead != (messagesReader.GetInt32(26) == 1))
                            {
                                message.DateRead = ConvertChatDateFormat(messagesReader.GetInt32(16));
                                message.HasRead = messagesReader.GetInt32(26) == 1;
                                update = true;
                            }
                            if (message.HasSent != (messagesReader.GetInt32(28) == 1))
                            {
                                message.HasSent = messagesReader.GetInt32(28) == 1;
                                update = true;
                            }
                        }
                        foreach (var join in messagesAttachmentsJoin)
                            if (join.Key == message.Id)
                                if (!message.Attachments.Contains(Attachments[join.Value]))
                                {
                                    message.Attachments.Add(Attachments[join.Value]);
                                    update = true;
                                }
                        if (added)
                        {
                            Logging.Log("[DatabaseStore] Added message: " + message.Id);
                            Messages.Add(message.Id, message);
                            InvokeStoreUpdate(ObjectType.Message, EventType.Add, message);
                        }
                        else if (update)
                        {
                            Logging.Log("[DatabaseStore] Updated message: " + message.Id);
                            InvokeStoreUpdate(ObjectType.Message, EventType.Update, message);
                        }
                        currentMessages.Add(message.Id);
                    }
                    if (init)
                        Logging.Log("[DatabaseStore] Found " + Messages.Count + " messages");

                    List<KeyValuePair<int, int>> conversationsRecipientsJoin = new List<KeyValuePair<int, int>>();
                    SqliteDataReader conversationsRecipientsJoinReader = new SqliteCommand("SELECT * FROM chat_handle_join") { Connection = dbConnection }.ExecuteReader();
                    while (conversationsRecipientsJoinReader.Read())
                        conversationsRecipientsJoin.Add(new KeyValuePair<int, int>(conversationsRecipientsJoinReader.GetInt32(0), conversationsRecipientsJoinReader.GetInt32(1)));

                    List<KeyValuePair<int, int>> conversationsMessagesJoin = new List<KeyValuePair<int, int>>();
                    SqliteDataReader conversationsMessagesJoinReader = new SqliteCommand("SELECT * FROM chat_message_join") { Connection = dbConnection }.ExecuteReader();
                    while (conversationsMessagesJoinReader.Read())
                        conversationsMessagesJoin.Add(new KeyValuePair<int, int>(conversationsMessagesJoinReader.GetInt32(0), conversationsMessagesJoinReader.GetInt32(1)));

                    List<int> currentConversations = new List<int>();
                    SqliteDataReader conversationsReader = new SqliteCommand("SELECT * FROM chat") { Connection = dbConnection }.ExecuteReader();
                    while (conversationsReader.Read())
                    {
                        Conversation conversation;
                        bool added = false;
                        bool update = false;
                        if (!Conversations.ContainsKey(conversationsReader.GetInt32(0)))
                        {
                            conversation = new Conversation()
                            {
                                Id = conversationsReader.GetInt32(0),
                                Name = conversationsReader.GetValue(6).ToString(),
                                ServiceType = (ServiceType)Enum.Parse(typeof(ServiceType), conversationsReader.GetValue(7).ToString()),
                                HasGroupName = conversationsReader.GetValue(12).ToString() != "",
                                DisplayName = conversationsReader.GetValue(12).ToString(),
                                Messages = new List<Message>(),
                                Recipients = new List<Recipient>()
                            };
                            added = true;
                        }
                        else
                        {
                            conversation = Conversations[conversationsReader.GetInt32(0)];
                            if (conversation.HasGroupName != (conversationsReader.GetValue(12).ToString() != ""))
                            {
                                conversation.HasGroupName = conversationsReader.GetValue(12).ToString() != "";
                                if (!conversation.HasGroupName)
                                    conversation.DisplayName = FormatGroupName(conversation.Recipients);
                                else
                                    conversation.DisplayName = conversationsReader.GetValue(12).ToString();
                                update = true;
                            }
                        }
                        foreach (var join in conversationsRecipientsJoin)
                            if (join.Key == conversation.Id)
                            {
                                if (!conversation.Recipients.Contains(Recipients[join.Value]))
                                {
                                    conversation.Recipients.Add(Recipients[join.Value]);
                                    update = true;
                                }
                                if (!conversation.HasGroupName)
                                    conversation.DisplayName = FormatGroupName(conversation.Recipients);
                            }
                        foreach (var join in conversationsMessagesJoin)
                            if (join.Key == conversation.Id)
                                if (!conversation.Messages.Contains(Messages[join.Value]))
                                {
                                    conversation.Messages.Add(Messages[join.Value]);
                                    update = true;
                                }
                        if (added)
                        {
                            Logging.Log("[DatabaseStore] Added conversation: " + conversation.Id);
                            Conversations.Add(conversation.Id, conversation);
                            InvokeStoreUpdate(ObjectType.Conversation, EventType.Add, conversation);
                        }
                        else if (update)
                        {
                            Logging.Log("[DatabaseStore] Updated conversation: " + conversation.Id);
                            InvokeStoreUpdate(ObjectType.Conversation, EventType.Update, conversation);
                        }
                        currentConversations.Add(conversation.Id);
                    }
                    if (init)
                        Logging.Log("[DatabaseStore] Found " + Conversations.Count + " conversations");

                    Dictionary<int, Conversation> conversationsToRemove = new Dictionary<int, Conversation>();
                    foreach (var o in Conversations)
                        if (!currentConversations.Contains(o.Key))
                            conversationsToRemove.Add(o.Key, o.Value);
                    foreach (var otr in conversationsToRemove)
                    {
                        Logging.Log("[DatabaseStore] Removed conversation: " + otr.Key);
                        InvokeStoreUpdate(ObjectType.Conversation, EventType.Remove, otr.Value);
                        Conversations.Remove(otr.Key);
                    }

                    Dictionary<int, Message> messagesToRemove = new Dictionary<int, Message>();
                    foreach (var o in Messages)
                        if (!currentMessages.Contains(o.Key))
                            messagesToRemove.Add(o.Key, o.Value);
                    foreach (var otr in messagesToRemove)
                    {
                        Logging.Log("[DatabaseStore] Removed message: " + otr.Key);
                        InvokeStoreUpdate(ObjectType.Message, EventType.Remove, otr.Value);
                        Messages.Remove(otr.Key);
                    }

                    foreach (var c in Conversations)
                    {
                        bool update = false;
                        for (int i = 0; i < c.Value.Messages.Count; i++)
                            if (messagesToRemove.ContainsKey(c.Value.Messages[i].Id))
                            {
                                c.Value.Messages.RemoveAt(i);
                                update = true;
                            }
                        if (update)
                        {
                            Logging.Log("[DatabaseStore] Updated conversation: " + c.Key);
                            InvokeStoreUpdate(ObjectType.Conversation, EventType.Update, c.Value);
                        }
                    }

                    Dictionary<int, Attachment> attachmentsToRemove = new Dictionary<int, Attachment>();
                    foreach (var o in Attachments)
                        if (!currentAttachments.Contains(o.Key))
                            attachmentsToRemove.Add(o.Key, o.Value);
                    foreach (var otr in attachmentsToRemove)
                    {
                        Logging.Log("[DatabaseStore] Removed attachment: " + otr.Key);
                        InvokeStoreUpdate(ObjectType.Attachment, EventType.Remove, otr.Value);
                        Attachments.Remove(otr.Key);
                    }

                    Dictionary<int, Recipient> recipientsToRemove = new Dictionary<int, Recipient>();
                    foreach (var o in Recipients)
                        if (!currentRecipients.Contains(o.Key))
                            recipientsToRemove.Add(o.Key, o.Value);
                    foreach (var otr in recipientsToRemove)
                    {
                        Logging.Log("[DatabaseStore] Removed recipient: " + otr.Key);
                        InvokeStoreUpdate(ObjectType.Recipient, EventType.Remove, otr.Value);
                        Recipients.Remove(otr.Key);
                    }


                    updating = false;
                }
                catch (Exception ex)
                {
                    Logging.Log("[DatabaseStore] Failed to update store: " + ex.ToString());
                    updating = false;
                }
            else
                Logging.Log("[DatabaseStore] Still busy updating");
        }

        static string FormatPath(string path)
        {
            if (path.StartsWith("~/"))
                return Environment.GetEnvironmentVariable("HOME") + path.Remove(0, 1);
            else
                return path;
        }

        static string FormatGroupName(List<Recipient> recipients)
        {
            if (recipients.Count == 1)
                return recipients[0].Name;
            else if (recipients.Count == 2)
                return recipients[0].Name + " & " + recipients[1].Name;
            else if (recipients.Count == 3)
                return recipients[0].Name + ", " + recipients[1].Name + " and 1 other";
            else
                return recipients[0].Name + ", " + recipients[1].Name + " and " + (recipients.Count - 2) + " others";
        }

        public static event DatabaseStoreUpdateEventHandler StoreUpdate;
        static void InvokeStoreUpdate(ObjectType updateObjectType, EventType updateEventType, IIdObject obj)
        {
            StoreUpdate?.Invoke(updateObjectType, updateEventType, obj);
        }

        public static void Close()
        {
            Logging.Log("[DatabaseStore] Closing database store...");
            updateTimer.Stop();
            dbConnection.Close();
        }

        // Convert iMessage date to DateTime format.
        static DateTime? ConvertChatDateFormat(int date)
        {
            if (date > 0)
                return new DateTime(2001, 1, 1).AddSeconds(date).ToLocalTime();
            else
                return null;
        }
    }

    internal delegate void DatabaseStoreUpdateEventHandler(ObjectType updateObjectType, EventType updateEventType, IIdObject obj);
}
