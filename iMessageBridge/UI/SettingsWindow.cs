using System;
using MonoMac.AppKit;
using MonoMac.Foundation;
using System.Drawing;

namespace DylanBriedis.iMessageBridge.UI
{
    [Register("SettingsWindow")]
    public class SettingsWindow : NSWindow
    {
        public SettingsWindow(IntPtr handle) : base(handle) { }
        [Export("initWithCoder:")]
        public SettingsWindow(NSCoder coder) : base(coder) { }

        SettingsServerViewController serverViewController = new SettingsServerViewController();
        SettingsDiscoveryViewController discoveryViewController = new SettingsDiscoveryViewController();
        SettingsNotificationsViewController notificationsViewController = new SettingsNotificationsViewController();
        public override void AwakeFromNib()
        {
            Level = NSWindowLevel.Floating;

            Toolbar.SelectedItemIdentifier = "Server";
            ContentView = serverViewController.View;
            ChangeHeight(221);
        }

        [Action("tabBtnClick:")]
        void TabBtnClick(NSToolbarItem item)
        {
            Toolbar.SelectedItemIdentifier = item.Identifier;
            switch (item.Identifier)
            {
                case "Server":
                    ContentView = serverViewController.View;
                    ChangeHeight(221);
                    break;
                case "Discovery":
                    ContentView = discoveryViewController.View;
                    ChangeHeight(246);
                    break;
                case "Notifications":
                    ContentView = notificationsViewController.View;
                    ChangeHeight(74);
                    break;
            }
        }

        public void ChangeHeight(float height)
        {
            RectangleF frame = Frame;
            float contentHeight = ContentView.Frame.Height;
            float toolbarHeight = frame.Height - contentHeight;
            frame.Y -= toolbarHeight + height;
            frame.Y += frame.Height;
            frame.Size = new SizeF(400, toolbarHeight + height);
            SetFrame(frame, true, true);
        }
    }

    [Register("SettingsWindowController")]
    public class SettingsWindowController : NSWindowController
    {
        public SettingsWindowController() : base("SettingsWindow")
        {
            LoadWindow();
        }
    }

    #region Setting views
    [Register("SettingsServerView")]
    public class SettingsServerView : NSView
    {
        public SettingsServerView(IntPtr handle) : base(handle) { }
        [Export("initWithCoder:")]
        public SettingsServerView(NSCoder coder) : base(coder) { }
    }

    [Register("SettingsServerViewController")]
    public class SettingsServerViewController : NSViewController
    {
        public SettingsServerViewController() : base("SettingsServerView", NSBundle.MainBundle)
        {
            LoadView();
        }
    }

    [Register("SettingsDiscoveryView")]
    public class SettingsDiscoveryView : NSView
    {
        public SettingsDiscoveryView(IntPtr handle) : base(handle) { }
        [Export("initWithCoder:")]
        public SettingsDiscoveryView(NSCoder coder) : base(coder) { }
    }

    [Register("SettingsDiscoveryViewController")]
    public class SettingsDiscoveryViewController : NSViewController
    {
        public SettingsDiscoveryViewController() : base("SettingsDiscoveryView", NSBundle.MainBundle)
        {
            LoadView();
        }
    }

    [Register("SettingsNotificationsView")]
    public class SettingsNotificationsView : NSView
    {
        public SettingsNotificationsView(IntPtr handle) : base(handle) { }
        [Export("initWithCoder:")]
        public SettingsNotificationsView(NSCoder coder) : base(coder) { }
    }

    [Register("SettingsNotificationsViewController")]
    public class SettingsNotificationsViewController : NSViewController
    {
        public SettingsNotificationsViewController() : base("SettingsNotificationsView", NSBundle.MainBundle)
        {
            LoadView();
        }
    }
    #endregion
}
