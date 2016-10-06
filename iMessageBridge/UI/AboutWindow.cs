using System;
using MonoMac.AppKit;
using MonoMac.Foundation;
using System.Diagnostics;

namespace DylanBriedis.iMessageBridge.UI
{
    [Register("AboutWindow")]
    public class AboutWindow : NSWindow
    {
        public AboutWindow(IntPtr handle) : base(handle) { }
        [Export("initWithCoder:")]
        public AboutWindow(NSCoder coder) : base(coder) { }

        [Outlet("versionLabel")]
        NSTextField VersionLabel { get; set; }

        public override void AwakeFromNib()
        {
            Level = NSWindowLevel.Floating;
            VersionLabel.StringValue = "Version " + ServerInfo.BridgeVersion;
        }

        [Action("help:")]
        void Help(NSObject sender)
        {
            Close();
            Process.Start("http://help.dylanbriedis.com/iMessageBridge");
        }

        [Action("visitMyWebsite:")]
        void VisitMyWebsite(NSObject sender)
        {
            Close();
            Process.Start("http://www.dylanbriedis.com/");
        }

        [Action("viewOnGitHub:")]
        void ViewOnGitHub(NSObject sender)
        {
            Close();
            Process.Start("https://github.com/3dflash/iMessageBridge");
        }
    }

    [Register("AboutWindowController")]
    public class AboutWindowController : NSWindowController
    {
        public AboutWindowController() : base("AboutWindow")
        {
            LoadWindow();
        }
    }
}
