using DylanBriedis.iMessageBridge;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace iMessageBridgeUWPTestClient
{
    sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        internal BridgeContext context;
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;
            }
            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                Window.Current.Activate();
            }
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.ToastNotification)
            {
                ToastNotificationActivatedEventArgs eventArgs = args as ToastNotificationActivatedEventArgs;
                WwwFormUrlDecoder query = new WwwFormUrlDecoder(eventArgs.Argument);
                context.SendMessageAsync(query[0].Value, eventArgs.UserInput["replyTextBox"].ToString(), query[1].Value == "1");
            }
        }
    }
}
