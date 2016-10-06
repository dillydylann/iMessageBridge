using MonoMac.Foundation;
using System;
using System.Diagnostics;
using MonoMac.AppKit;
using DylanBriedis.iMessageBridge.UI;

namespace DylanBriedis.iMessageBridge
{
    class Program
    {
        static void Main(string[] args)
        {
            NSApplication.Init();
            Logging.Log("iMessage Bridge 1.0");
            NSUserDefaults.StandardUserDefaults.RegisterDefaults(NSDictionary.FromFile(NSBundle.MainBundle.PathForResource("DefaultPreferences", "plist")));

            // Find out if user is logged into iMessage.
            NSAppleScript asConnectionStatus = new NSAppleScript("tell application \"Messages\" to get 1st service's connection status whose service type = iMessage");
            NSDictionary ei;
            NSAppleEventDescriptor aed = asConnectionStatus.ExecuteAndReturnError(out ei);
            if (ei == null)
                if (aed.StringValue == "dcon") // iMessage service is disconnected.
                {
                    NSAlert alert = new NSAlert()
                    {
                        MessageText = "It appears you're not signed in to iMessage.",
                        InformativeText = "The bridge will not work if you are not signed in. Would you like to sign in to iMessage?"
                    };
                    alert.AddButton("Yes");
                    alert.AddButton("Not now");
                    NSRunningApplication.CurrentApplication.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);
                    if (alert.RunModal() == 1000) // User clicks yes.
                        Process.Start("/Applications/Messages.app");
                    Environment.Exit(1);
                }
            DatabaseStore.Init();
            HttpServer.Start();
            StreamServer.Start();
            if (NSUserDefaults.StandardUserDefaults.BoolForKey("DiscoveryMode"))
                Discovery.Register();
            
            NSApplication app = NSApplication.SharedApplication;
            app.Delegate = new AppDelegate();
            app.Run();
        }

        public static void Exit()
        {
            Logging.Log("Shutting down...");
            Discovery.Unregister();
            HttpServer.Stop();
            StreamServer.Stop();
            DatabaseStore.Close();
            Logging.Log("Exiting...");
            Process.GetCurrentProcess().Kill(); // Environment.Exit(0); hangs the app for some reason.
        }
    }
}
