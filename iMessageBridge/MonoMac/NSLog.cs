//
// NSLog.cs
// Created by William Kent <wjk011@gmail.com>
// Copyright (c) 2013 Sunburst Solutions. All rights reserved.
//
using System;
using System.Runtime.InteropServices;

namespace MonoMac.Foundation
{
    public static class NSLog
    {
        [DllImport("/System/Library/Frameworks/Foundation.framework/Foundation", EntryPoint = "NSLog")]
        private static extern void Log(IntPtr text);

        [DllImport("/System/Library/Frameworks/Foundation.framework/Foundation", EntryPoint = "NSLog")]
        private static extern void Log(IntPtr text, IntPtr args);

        /// <summary>
        /// Writes a message to <c>NSLog()</c>.
        /// </summary>
        /// <param name="format">A format string. This string uses <see cref="string.Format"/>-style format arguments.</param>
        /// <param name="arguments">A series of format arguments.</param>
        public static void Log(string format, params object[] arguments)
        {
            NSString cocoaFormat = new NSString(string.Format(format, arguments));
            Log(cocoaFormat.Handle);
        }

        /// <summary>
        /// Writes a message to <c>NSLog()</c>.
        /// </summary>
        /// <param name="format">A format string. This string uses Cocoa-style (<c>%@</c>, etc.) format arguments.</param>
        /// <param name="arguments">A series of format arguments. These must be NSObjects or classes convertible into NSObjects.</param>
        public static void CocoaLog(string format, params object[] arguments)
        {
            NSString cocoaFormat = new NSString(format);

            if (arguments == null || arguments.Length == 0)
            {
                Log(cocoaFormat.Handle);
                return;
            }

            var nativeArray = Marshal.AllocHGlobal(arguments.Length * IntPtr.Size);
            for (int i = 0; i < arguments.Length; i++)
            {
                object value = arguments[i];
                NSObject cocoaValue = null;

                if (value is string)
                {
                    cocoaValue = new NSString((string)value);
                }
                else if (value is int || value is long)
                {
                    cocoaValue = new NSNumber((long)value);
                }
                else if (value is uint || value is ulong)
                {
                    cocoaValue = new NSNumber((ulong)value);
                }
                else if (value is float || value is double)
                {
                    cocoaValue = new NSNumber((double)value);
                }
                else if (value is NSObject)
                {
                    cocoaValue = (NSObject)value;
                }
                else
                {
                    cocoaValue = new NSString(value.ToString());
                }

                Marshal.WriteIntPtr(nativeArray, i * IntPtr.Size, cocoaValue.Handle);
            }

            Log(cocoaFormat, nativeArray);
            Marshal.FreeHGlobal(nativeArray);
        }
    }
}