using DylanBriedis.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace DylanBriedis.iMessageBridge.TestClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        BridgeContext context = new BridgeContext("192.168.3.36"); // Replace 10.10.10.70 with your bridge's IP.
        bool loginSuccess = false;
        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            if (context.AuthenticationIsRequired)
                while (!loginSuccess)
                    if (Properties.Settings.Default.LoginSaved)
                    {
                        context.AuthenticationCredentials = new NetworkCredential(Properties.Settings.Default.Username, Properties.Settings.Default.Password);
                        loginSuccess = context.ValidateCredentials();
                        if (!loginSuccess)
                            Properties.Settings.Default.LoginSaved = false;
                    }
                    else
                    {
                        LoginDialogInfo ldi = LoginDialog.Show(IntPtr.Zero, "iMessage Bridge Login", "Authentication is required.", true);
                        if (ldi != null)
                        {
                            if (ldi.SaveChecked)
                            {
                                Properties.Settings.Default.LoginSaved = true;
                                Properties.Settings.Default.Username = ldi.Username;
                                Properties.Settings.Default.Password = ldi.Password;
                            }
                            context.AuthenticationCredentials = new NetworkCredential(ldi.Username, ldi.Password);
                            loginSuccess = context.ValidateCredentials();
                            if (!loginSuccess)
                                Properties.Settings.Default.LoginSaved = false;
                        }
                        else
                            Environment.Exit(0);
                    }
            Properties.Settings.Default.Save();
            context.Init();
            context.StartStream();

            foreach (KeyValuePair<int, Recipient> r in context.Recipients)
                listBox1.Items.Add(r.Value);
            foreach (KeyValuePair<int, Conversation> c in context.Conversations)
                listBox4.Items.Add(c.Value);

            context.StreamUpdate += (s, ev) =>
            {
                if (ev.Error == null)
                {
                    listBox2.Items.Add(ev.EventType + " | " + ev.ObjectType + " | " + ev.Object);
                    switch (ev.ObjectType)
                    {
                        case ObjectType.Recipient:
                            listBox1.Items.Clear();
                            foreach (KeyValuePair<int, Recipient> r in context.Recipients)
                                listBox1.Items.Add(r.Value);
                            break;
                        case ObjectType.Conversation:
                            listBox4.Items.Clear();
                            foreach (KeyValuePair<int, Conversation> c in context.Conversations)
                            {
                                listBox4.Items.Add(c.Value);
                                if (currentConversation != null)
                                    if (currentConversation.Id == c.Value.Id)
                                    {
                                        currentConversation = c.Value;
                                        listBox3.Items.Clear();
                                        foreach (Message m in currentConversation.Messages)
                                            listBox3.Items.Add(m);
                                    }
                            }
                            break;
                    }
                    if (ev.EventType == EventType.Add && ev.ObjectType == ObjectType.Message)
                    {
                        var message = ev.Object as Message;
                        if (!message.FromMe)
                            notifyIcon1.ShowBalloonTip(5000, message.From.Name, message.Text, ToolTipIcon.Info);
                    }
                }
                else if (ev.Error.Message.ToLower().Contains("close")) // Reconnect when the stream closes unexpectedly.
                    context.StartStream();
                else
                    throw ev.Error;
            };

            propertyGrid1.SelectedObject = context;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            context.SendMessage(textBox1.Text, textBox2.Text, false);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            context.SendMessage(textBox1.Text, textBox2.Text, true);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = (listBox1.SelectedItem as Recipient).Address;
            label1.Text = (listBox1.SelectedItem as Recipient).ServiceType.ToString();
            propertyGrid1.SelectedObject = listBox1.SelectedItem;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            context.SendMessage(listBox1.SelectedItem as Recipient, textBox2.Text);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                context.StartStream();
            else
                context.StopStream();
        }

        Conversation currentConversation;
        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox3.Clear();
            listBox3.Items.Clear();
            currentConversation = (listBox4.SelectedItem as Conversation);
            foreach (Recipient r in currentConversation.Recipients)
                textBox3.Text += r.Name + " (" + r.Address + "), ";
            foreach (Message m in currentConversation.Messages)
                listBox3.Items.Add(m);
            propertyGrid1.SelectedObject = listBox4.SelectedItem;
        }

        private void showPictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = new Form();
            Image i = Image.FromStream(context.DownloadRecipientPicture((listBox1.SelectedItem as Recipient).Id));
            f.BackgroundImage = i;
            f.ClientSize = new Size(i.Width, i.Height);
            f.Text = (listBox1.SelectedItem as Recipient).Name;
            f.Show();
        }

        private void listBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                listBox1.SelectedIndex = listBox1.IndexFromPoint(e.X, e.Y);
                showPictureToolStripMenuItem.Enabled = (listBox1.SelectedItem as Recipient).HasPicture;
                contextMenuStrip1.Show(listBox1, e.X, e.Y);
            }
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = listBox3.SelectedItem;
        }
    }
}
