using System;
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;

namespace DylanBriedis.iMessageBridge.UI
{
    public class AppDelegate : NSApplicationDelegate
    {
        public AppDelegate() { }

        NSStatusItem statusMenuItem = NSStatusBar.SystemStatusBar.CreateStatusItem(-1);
        public override void FinishedLaunching(NSObject notification)
        {
            NSBundle.MainBundle.LoadNibNamed("MainMenu", this, null);
            statusMenuItem = NSStatusBar.SystemStatusBar.CreateStatusItem(-1);
            statusMenuItem.Image = NSImage.ImageNamed("TrayIcon");
            statusMenuItem.AlternateImage = NSImage.ImageNamed("TrayIconAlt.png");
            statusMenuItem.ToolTip = "iMessage Bridge";
            var statusMenu = new NSMenu();
            statusMenu.AddItem("About iMessage Bridge", new Selector("statusMenuAbout:"), "");
            statusMenu.AddItem(NSMenuItem.SeparatorItem);
            statusMenu.AddItem("Settings", new Selector("statusMenuSettings:"), "");
            statusMenu.AddItem(NSMenuItem.SeparatorItem);
            statusMenu.AddItem("Exit", new Selector("statusMenuExit:"), "");
            statusMenuItem.Menu = statusMenu;

            NSUserDefaults.StandardUserDefaults.AddObserver(this, new NSString("ServerAuthentication"), NSKeyValueObservingOptions.New, IntPtr.Zero);
            NSUserDefaults.StandardUserDefaults.AddObserver(this, new NSString("DiscoveryMode"), NSKeyValueObservingOptions.New, IntPtr.Zero);
            NSUserDefaults.StandardUserDefaults.AddObserver(this, new NSString("DiscoveryDisplayName"), NSKeyValueObservingOptions.New, IntPtr.Zero);

            if (!NSUserDefaults.StandardUserDefaults.BoolForKey("FirstTimeShown"))
            {
                NSAlert alert = new NSAlert()
                {
                    MessageText = "Welcome to iMessage Bridge",
                    InformativeText = "To get the most out of your experience, would you like to setup the bridge first?"
                };
                alert.AddButton("Yes");
                alert.AddButton("Not now");
                NSRunningApplication.CurrentApplication.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);
                if (alert.RunModal() == 1000)
                    StatusMenuSettings();
                else
                    new NSAlert() { MessageText = "Tip", InformativeText = "If you want to setup the bridge later, just click the icon on the menu bar and then click settings." }.RunModal();
                NSUserDefaults.StandardUserDefaults.SetBool(true, "FirstTimeShown");
            }
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            switch (keyPath.ToString())
            {
                case "ServerAuthentication":
                    HttpServer.Stop();
                    HttpServer.Start();
                    break;
                case "DiscoveryMode":
                    if (NSUserDefaults.StandardUserDefaults.BoolForKey("DiscoveryMode"))
                        Discovery.Register();
                    else
                        Discovery.Unregister();
                    break;
                case "DiscoveryDisplayName":
                    Discovery.Unregister();
                    Discovery.Register();
                    break;
            }
        }

        AboutWindowController aboutWindow = new AboutWindowController();
        [Action("statusMenuAbout:")]
        void StatusMenuAbout()
        {
            NSRunningApplication.CurrentApplication.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);
            if (!aboutWindow.Window.IsVisible)
                aboutWindow.Window.Center();
            aboutWindow.Window.MakeKeyAndOrderFront(this);
        }

        SettingsWindowController settingsWindow = new SettingsWindowController();
        [Action("statusMenuSettings:")]
        void StatusMenuSettings()
        {
            NSRunningApplication.CurrentApplication.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);
            if (!settingsWindow.Window.IsVisible)
                settingsWindow.Window.Center();
            settingsWindow.Window.MakeKeyAndOrderFront(this);
        }

        [Action("statusMenuExit:")]
        void StatusMenuExit()
        {
            ExitApplication();
        }

        public void ExitApplication()
        {
            if (!NSUserDefaults.StandardUserDefaults.BoolForKey("DontShowExitDialog"))
            {
                NSAlert alert = new NSAlert()
                {
                    MessageText = "Do you want to exit iMessage Bridge?",
                    InformativeText = "Clients that are connected to this computer will no longer receive messages if the bridge is not running.",
                    ShowsSuppressionButton = true
                };
                alert.AddButton("Yes");
                alert.AddButton("No");
                NSRunningApplication.CurrentApplication.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);
                if (alert.RunModal() == 1000)
                {
                    if (alert.SuppressionButton.State == NSCellStateValue.On)
                        NSUserDefaults.StandardUserDefaults.SetBool(true, "DontShowExitDialog");
                    Program.Exit();
                }
            }
            else
                Program.Exit();
        }

        public override NSApplicationTerminateReply ApplicationShouldTerminate(NSApplication sender)
        {
            ExitApplication();
            return NSApplicationTerminateReply.Cancel;
        }
    }
}
