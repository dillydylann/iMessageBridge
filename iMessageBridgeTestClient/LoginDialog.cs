using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DylanBriedis.UI
{
    public static class LoginDialog
    {
        [DllImport("ole32.dll")]
        static extern void CoTaskMemFree(IntPtr ptr);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct CREDUI_INFO
        {
            public int cbSize;
            public IntPtr hwndParent;
            public string pszMessageText;
            public string pszCaptionText;
            public IntPtr hbmBanner;
        }


        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        private static extern bool CredUnPackAuthenticationBuffer(int dwFlags,
                                                                   IntPtr pAuthBuffer,
                                                                   uint cbAuthBuffer,
                                                                   StringBuilder pszUserName,
                                                                   ref int pcchMaxUserName,
                                                                   StringBuilder pszDomainName,
                                                                   ref int pcchMaxDomainame,
                                                                   StringBuilder pszPassword,
                                                                   ref int pcchMaxPassword);

        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        private static extern int CredUIPromptForWindowsCredentials(ref CREDUI_INFO notUsedHere,
                                                                     int authError,
                                                                     ref uint authPackage,
                                                                     IntPtr InAuthBuffer,
                                                                     uint InAuthBufferSize,
                                                                     out IntPtr refOutAuthBuffer,
                                                                     out uint refOutAuthBufferSize,
                                                                     ref bool fSave,
                                                                     int flags);

        public static LoginDialogInfo Show(IntPtr hwnd, string title, string message, bool canSave)
        {
            CREDUI_INFO info = new CREDUI_INFO();
            info.cbSize = Marshal.SizeOf(info);
            info.pszCaptionText = title;
            info.pszMessageText = message;
            info.hwndParent = hwnd;
            uint authPackage = 0;

            IntPtr buffer;
            uint bufferSize;
            bool save = false;

            CredUIPromptForWindowsCredentials(ref info, 0, ref authPackage, IntPtr.Zero, 0, out buffer, out bufferSize, ref save, 0x1 | (canSave ? 0x2 : 0));

            if (buffer == IntPtr.Zero)
                return null;

            StringBuilder username = new StringBuilder(100);
            int usernameSize = 100;
            StringBuilder password = new StringBuilder(100);
            int passwordSize = 100;
            StringBuilder domain = new StringBuilder(100);
            int domainSize = 100;
            CredUnPackAuthenticationBuffer(0, buffer, bufferSize, username, ref usernameSize, domain, ref domainSize, password, ref passwordSize);

            CoTaskMemFree(buffer);

            return new LoginDialogInfo() { Username = username.ToString(), Password = password.ToString(), SaveChecked = save };
        }
    }

    public class LoginDialogInfo
    {
        internal LoginDialogInfo() { }
        public string Username { get; internal set; }
        public string Password { get; internal set; }
        public bool SaveChecked { get; internal set; }
    }
}
