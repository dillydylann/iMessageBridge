using MonoMac.Foundation;
using System;

namespace DylanBriedis.iMessageBridge
{
    internal static class Logging
    {
        public static void Log(string message)
        {
            if (Environment.OSVersion.Version < new Version(16, 0)) // Darwin (kernel) version.
                NSLog.Log(message); // NSLog crashes on macOS Sierra.
            else
                Console.WriteLine(message);
        }
    }
}
