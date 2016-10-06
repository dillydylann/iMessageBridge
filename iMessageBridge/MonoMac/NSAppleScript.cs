//
// NSAppleScript wrapper for MonoMac.
// Copyright © Dylan Briedis 2016
//
using System;
using System.Runtime.InteropServices;
using MonoMac.ObjCRuntime;

namespace MonoMac.Foundation
{
    /// <summary>
    /// The NSAppleScript class provides the ability to load, compile, and execute scripts.
    /// </summary>
    [Register("NSAppleScript", true)]
    public class NSAppleScript : NSObject
    {
        public override IntPtr ClassHandle
        {
            get { return Class.GetHandle("NSAppleScript"); }
        }

        /// <summary>
        /// Initializes a newly allocated script instance from the source identified by the passed URL.
        /// </summary>
        /// <param name="url">A URL that locates a script, in either text or compiled form.</param>
        /// <param name="errorInfo">On return, if an error occurs, a pointer to an error information dictionary.</param>
        [Export("initWithContentsOfURL:error:")]
        public NSAppleScript(NSUrl url, out NSDictionary errorInfo)
            : base(NSObjectFlag.Empty)
        {
            IntPtr errorInfoHandle = Marshal.AllocHGlobal(4);
            if (IsDirectBinding)
                base.Handle = Messaging.IntPtr_objc_msgSend_IntPtr_IntPtr(base.Handle, Selector.GetHandle("initWithContentsOfURL:error:"), url.Handle, errorInfoHandle);
            else
                base.Handle = Messaging.IntPtr_objc_msgSendSuper_IntPtr_IntPtr(base.SuperHandle, Selector.GetHandle("initWithContentsOfURL:error:"), url.Handle, errorInfoHandle);
            IntPtr errorInfoHandle2 = Marshal.ReadIntPtr(errorInfoHandle);
            errorInfo = (NSDictionary)Runtime.GetNSObject(errorInfoHandle2);
            Marshal.FreeHGlobal(errorInfoHandle);
        }

        /// <summary>
        /// Initializes a newly allocated script instance from the passed source.
        /// </summary>
        /// <param name="source">A string containing the source code of a script.</param>
        [Export("initWithSource:")]
        public NSAppleScript(string source)
            : base(NSObjectFlag.Empty)
        {
            IntPtr str = NSString.CreateNative(source);
	        if (IsDirectBinding)
                base.Handle = Messaging.IntPtr_objc_msgSend_IntPtr(base.Handle, Selector.GetHandle("initWithSource:"), str);
	        else
                base.Handle = Messaging.IntPtr_objc_msgSendSuper_IntPtr(base.SuperHandle, Selector.GetHandle("initWithSource:"), str);
            NSString.ReleaseNative(str);
        }

        #region NSObject constructors
        [Export("initWithCoder:")]
        public NSAppleScript(NSCoder coder)
            : base(NSObjectFlag.Empty)
        {
	        if (IsDirectBinding)
		        base.Handle = Messaging.IntPtr_objc_msgSend_IntPtr(base.Handle, Selector.InitWithCoder, coder.Handle);
	        else
		        base.Handle = Messaging.IntPtr_objc_msgSendSuper_IntPtr(base.SuperHandle, Selector.InitWithCoder, coder.Handle);
        }

        public NSAppleScript(NSObjectFlag t) : base(t) { }
        public NSAppleScript(IntPtr handle) : base(handle) { }
        #endregion

        /// <summary>
        /// A Boolean value that indicates whether the receiver's script has been compiled. (read-only)
        /// </summary>
        public bool Compiled
        {
            [Export("compiled")]
            get
            {
                if (IsDirectBinding)
                    return Messaging.bool_objc_msgSend(base.Handle, Selector.GetHandle("compiled"));
                else
                    return Messaging.bool_objc_msgSendSuper(base.SuperHandle, Selector.GetHandle("compiled"));
            }
        }

        /// <summary>
        /// The script source for the receiver. (read-only)
        /// </summary>
        public string Source
        {
            [Export("source")]
            get
            {
                if (IsDirectBinding)
                    return ((NSString)Runtime.GetNSObject(Messaging.IntPtr_objc_msgSend(base.Handle, Selector.GetHandle("source")))).ToString();
                else
                    return ((NSString)Runtime.GetNSObject(Messaging.IntPtr_objc_msgSendSuper(base.SuperHandle, Selector.GetHandle("source")))).ToString();
            }
        }

        /// <summary>
        /// Compiles the receiver, if it is not already compiled.
        /// </summary>
        /// <param name="errorInfo">On return, if an error occurs, a pointer to an error information dictionary.</param>
        /// <returns>"true" for success or if the script was already compiled, "false" otherwise.</returns>
        [Export("compileAndReturnError:")]
        public bool CompileAndReturnError(out NSDictionary errorInfo)
        {
            bool result;
            IntPtr errorInfoHandle = Marshal.AllocHGlobal(4);
            if (IsDirectBinding)
                result = Messaging.bool_objc_msgSend_IntPtr(base.Handle, Selector.GetHandle("compileAndReturnError:"), errorInfoHandle);
            else
                result = Messaging.bool_objc_msgSendSuper_IntPtr(base.SuperHandle, Selector.GetHandle("compileAndReturnError:"), errorInfoHandle);
            if (!result)
            {
                IntPtr errorInfoHandle2 = Marshal.ReadIntPtr(errorInfoHandle);
                errorInfo = (NSDictionary)Runtime.GetNSObject(errorInfoHandle2);
            }
            else
                errorInfo = null;
            Marshal.FreeHGlobal(errorInfoHandle);
            return result;
        }

        /// <summary>
        /// Executes the receiver, compiling it first if it is not already compiled.
        /// </summary>
        /// <param name="errorInfo">On return, if an error occurs, a pointer to an error information dictionary.</param>
        /// <returns>The result of executing the event, or null if an error occurs.</returns>
        [Export("executeAndReturnError:")]
        public NSAppleEventDescriptor ExecuteAndReturnError(out NSDictionary errorInfo)
        {
            NSAppleEventDescriptor result;
            IntPtr errorInfoHandle = Marshal.AllocHGlobal(4);
            if (IsDirectBinding)
                result = (NSAppleEventDescriptor)Runtime.GetNSObject(Messaging.IntPtr_objc_msgSend_IntPtr(base.Handle, Selector.GetHandle("executeAndReturnError:"), errorInfoHandle));
            else
                result = (NSAppleEventDescriptor)Runtime.GetNSObject(Messaging.IntPtr_objc_msgSendSuper_IntPtr(base.SuperHandle, Selector.GetHandle("executeAndReturnError:"), errorInfoHandle));
            if (result == null)
            {
                IntPtr errorInfoHandle2 = Marshal.ReadIntPtr(errorInfoHandle);
                errorInfo = (NSDictionary)Runtime.GetNSObject(errorInfoHandle2);
            }
            else
                errorInfo = null;
            Marshal.FreeHGlobal(errorInfoHandle);
            return result;
        }

        /// <summary>
        /// Executes an Apple event in the context of the receiver, as a means of allowing the application to invoke a handler in the script.
        /// </summary>
        /// <param name="event">The Apple event to execute.</param>
        /// <param name="errorInfo">On return, if an error occurs, a pointer to an error information dictionary.</param>
        /// <returns>The result of executing the event, or null if an error occurs.</returns>
        [Export("executeAppleEvent:error:")]
        public NSAppleEventDescriptor ExecuteAppleEvent(NSAppleEventDescriptor @event, out NSDictionary errorInfo)
        {
            NSAppleEventDescriptor result;
            IntPtr errorInfoHandle = Marshal.AllocHGlobal(4);
            if (IsDirectBinding)
                result = (NSAppleEventDescriptor)Runtime.GetNSObject(Messaging.IntPtr_objc_msgSend_IntPtr_IntPtr(base.Handle, Selector.GetHandle("executeAppleEvent:error:"), @event.Handle, errorInfoHandle));
            else
                result = (NSAppleEventDescriptor)Runtime.GetNSObject(Messaging.IntPtr_objc_msgSendSuper_IntPtr_IntPtr(base.SuperHandle, Selector.GetHandle("executeAppleEvent:error:"), @event.Handle, errorInfoHandle));
            if (result == null)
            {
                IntPtr errorInfoHandle2 = Marshal.ReadIntPtr(errorInfoHandle);
                errorInfo = (NSDictionary)Runtime.GetNSObject(errorInfoHandle2);
            }
            else
                errorInfo = null;
            Marshal.FreeHGlobal(errorInfoHandle);
            return result;
        }
    }
}
