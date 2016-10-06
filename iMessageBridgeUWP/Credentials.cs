using System.Net;

namespace DylanBriedis.iMessageBridge
{
    public sealed class Credentials
    {
        public Credentials(string username, string password)
        {
            UserName = username;
            Password = password;
        }

        public string UserName { get; private set; }
        public string Password { get; private set; }

        internal NetworkCredential ToNetworkCredentials()
        {
            return new NetworkCredential(UserName, Password);
        }
    }
}
