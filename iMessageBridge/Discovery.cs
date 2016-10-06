using MonoMac.Foundation;

namespace DylanBriedis.iMessageBridge
{
    internal static class Discovery
    {
        static NSNetService service;

        public static void Register()
        {
            Logging.Log("[Discovery] Registering...");
            service = new NSNetService("local", "_imb._tcp", NSUserDefaults.StandardUserDefaults.StringForKey("DiscoveryDisplayName"), 9080);
            service.Publish();
            Logging.Log("[Discovery] Registered");
        }

        public static void Unregister()
        {
            Logging.Log("[Discovery] Unregistering...");
            service.Stop();
            service.Dispose();
            Logging.Log("[Discovery] Unregistered");
        }
    }
}
