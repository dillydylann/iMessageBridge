#pragma warning disable CS4014

using DylanBriedis.iMessageBridge;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.Data.Xml.Dom;
using Windows.Security.Credentials.UI;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace iMessageBridgeUWPTestClient
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        BridgeContext context = new BridgeContext("192.168.3.36");
        bool loginSuccess = false;
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            (Application.Current as App).context = context;
            ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;
            if (settings.Values["LoginSaved"] == null)
            {
                settings.Values["Username"] = "";
                settings.Values["Password"] = "";
                settings.Values["LoginSaved"] = false;
            }
            if (await context.AuthenticationIsRequiredAsync())
                while (!loginSuccess)
                {
                    if ((bool)ApplicationData.Current.LocalSettings.Values["LoginSaved"])
                    {
                        context.AuthenticationCredentials = new Credentials(settings.Values["Username"].ToString(), settings.Values["Password"].ToString());
                        loginSuccess = await context.ValidateCredentialsAsync();
                        if (!loginSuccess)
                            settings.Values["LoginSaved"] = false;
                    }
                    else
                    {
                        CredentialPickerResults cpr = await CredentialPicker.PickAsync(new CredentialPickerOptions()
                        {
                            AuthenticationProtocol = AuthenticationProtocol.Basic,
                            Caption = "iMessage Bridge Login",
                            Message = "Authentication is required.",
                            CredentialSaveOption = CredentialSaveOption.Unselected, 
                            TargetName = "."
                        });
                        if (cpr.Credential != null)
                        {
                            if (cpr.CredentialSaved)
                            {
                                settings.Values["LoginSaved"] = true;
                                settings.Values["Username"] = cpr.CredentialUserName;
                                settings.Values["Password"] = cpr.CredentialPassword;
                            }
                            context.AuthenticationCredentials = new Credentials(cpr.CredentialUserName, cpr.CredentialPassword);
                            loginSuccess = await context.ValidateCredentialsAsync();
                            if (!loginSuccess)
                                settings.Values["LoginSaved"] = false;
                        }
                        else
                            Application.Current.Exit();
                    }
                }
            await context.InitAsync();

            foreach (KeyValuePair<int, Recipient> r in context.Recipients)
                recipientsListBox.Items.Add(r.Value);
            foreach (KeyValuePair<int, Conversation> c in context.Conversations)
                conversationsListBox.Items.Add(c.Value);

            context.StreamUpdate += async (s, ev) =>
            {
                if (ev.Error == null)
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                     {
                         streamEventsListBox.Items.Add(ev.EventType + " | " + ev.ObjectType + " | " + ev.Object);
                         switch (ev.ObjectType)
                         {
                             case ObjectType.Recipient:
                                 recipientsListBox.Items.Clear();
                                 foreach (KeyValuePair<int, Recipient> r in context.Recipients)
                                     recipientsListBox.Items.Add(r.Value);
                                 break;
                             case ObjectType.Conversation:
                                 conversationsListBox.Items.Clear();
                                 foreach (KeyValuePair<int, Conversation> c in context.Conversations)
                                 {
                                     conversationsListBox.Items.Add(c.Value);
                                     if (currentConversation != null)
                                         if (currentConversation.Id == c.Value.Id)
                                         {
                                             currentConversation = c.Value;
                                             conversationMessagesListBox.Items.Clear();
                                             foreach (Message m in currentConversation.Messages)
                                                 conversationMessagesListBox.Items.Add(m);
                                         }
                                 }
                                 break;
                         }
                         if (ev.EventType == EventType.Add && ev.ObjectType == ObjectType.Message)
                         {
                             var message = ev.Object as Message;
                             if (!message.FromMe)
                             {
                                 string pictureFileName = Path.GetTempFileName();
                                 if (message.From.HasPicture)
                                     using (IInputStream iis = await context.DownloadRecipientPictureAsync(message.From.Id))
                                     using (FileStream fs = File.Create(pictureFileName))
                                         iis.AsStreamForRead().CopyTo(fs);

                                 ToastNotifier tn = ToastNotificationManager.CreateToastNotifier();
                                 XmlDocument nx = new XmlDocument();
                                 nx.LoadXml(string.Format(@"<toast>
    <visual>
        <binding template=""ToastGeneric"">
            <text hint-maxLines=""1"">{0}</text>
            <text>{1}</text>
            <image placement=""appLogoOverride"" hint-crop=""circle"" src=""{2}""/>
        </binding>
    </visual>
    <actions>
        <input id=""replyTextBox"" type=""text"" placeHolderContent=""Reply...""/>
        <action
          content=""Send""
          arguments=""address={3}&sms={4}""/>
    </actions>
</toast>", message.From.Name, message.Text, message.From.HasPicture ? pictureFileName : "", message.From.Address, message.ServiceType == ServiceType.SMS ? "1" : "0"));
                                 ToastNotification n = new ToastNotification(nx);
                                 tn.Show(n);
                             }
                         }
                     });
                else if (ev.Error.Message.ToLower().Contains("closed")) // Reconnect when the stream closes unexpectedly.
                    await context.StartStreamAsync();
                else
                    throw ev.Error;
            };
        }

        private void sendiMsgBtn_Click(object sender, RoutedEventArgs e)
        {
            context.SendMessageAsync(addressTextBox.Text, textTextBox.Text, false);
        }

        private void sendSMSBtn_Click(object sender, RoutedEventArgs e)
        {
            context.SendMessageAsync(addressTextBox.Text, textTextBox.Text, true);
        }

        private void recipientsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (recipientsListBox.SelectedItem != null)
            {
                addressTextBox.Text = (recipientsListBox.SelectedItem as Recipient).Address;
                serviceTypeLabel.Text = (recipientsListBox.SelectedItem as Recipient).ServiceType.ToString();
            }
        }

        Conversation currentConversation;
        private void conversationsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (conversationsListBox.SelectedItem != null)
            {
                conversationRecipientsTextBox.Text = "";
                conversationMessagesListBox.Items.Clear();
                currentConversation = (conversationsListBox.SelectedItem as Conversation);
                foreach (Recipient r in currentConversation.Recipients)
                    conversationRecipientsTextBox.Text += r.Name + " (" + r.Address + "), ";
                foreach (Message m in currentConversation.Messages)
                    conversationMessagesListBox.Items.Add(m);
            }
        }

        private async void streamingCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (streamingCheckBox.IsChecked.Value)
                await context.StartStreamAsync();
            else
                context.StopStream();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            context.SendMessageAsync(recipientsListBox.SelectedItem as Recipient, textTextBox.Text);
        }
    }
}
