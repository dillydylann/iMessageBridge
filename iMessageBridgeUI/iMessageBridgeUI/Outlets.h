// This header file is used to connect actions and outlets from Interface Builder files.

#import <Cocoa/Cocoa.h>
#import <WebKit/WebKit.h>

@interface AboutWindowController : NSWindowController
@end

@interface AboutWindow : NSWindow
{
    IBOutlet NSTextField* versionLabel;
}

- (IBAction)help:(id)sender;
- (IBAction)visitMyWebsite:(id)sender;
- (IBAction)viewOnGitHub:(id)sender;

@end


@interface FeedbackWindowController : NSWindowController
@end

@interface FeedbackWindow : NSWindow
{
    IBOutlet WebView* webView;
}

- (IBAction)closeClick:(id)sender;

@end


@interface SettingsWindowController : NSWindowController
@end

@interface SettingsWindow : NSWindow

- (IBAction)tabBtnClick:(NSToolbarItem*)sender;

@end

// Setting views

@interface SettingsServerViewController : NSViewController
@end

@interface SettingsServerView : NSView
@end

@interface SettingsDiscoveryViewController : NSViewController
@end

@interface SettingsDiscoveryView : NSView
@end

@interface SettingsNotificationsViewController: NSViewController
@end

@interface SettingsNotificationsView : NSView
@end
