using System;
using MonoMac.ObjCRuntime;

namespace MonoMac.Foundation
{
    public static class Extensions
    {
        public static bool LoadNibNamed(this NSBundle bundle, string nibName, NSObject owner, NSArray topLevelObjects)
        {
            bool result;
            IntPtr name = NSString.CreateNative(nibName);
            result = Messaging.bool_objc_msgSend_IntPtr_IntPtr_IntPtr(bundle.Handle, Selector.GetHandle("loadNibNamed:owner:topLevelObjects:"), name, owner.Handle, topLevelObjects != null ? topLevelObjects.Handle : IntPtr.Zero);
            NSString.ReleaseNative(name);
            return result;
        }
    }
}
