using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PingAllDcs
{
    public static class Win32
    {
        // copied from here: http://weblogs.asp.net/hernandl/usercredentialsdialog

        [Flags]
        internal enum UserCredentialsDialogFlags
        {
            Default = GenericCredentials | ShowSaveCheckbox | AlwaysShowUI | ExpectConfirmation,
            None = 0x0,
            IncorrectPassword = 0x1,
            DoNotPersist = 0x2,
            RequestAdministrator = 0x4,
            ExcludesCertificates = 0x8,
            RequireCertificate = 0x10,
            ShowSaveCheckbox = 0x40,
            AlwaysShowUI = 0x80,
            RequireSmartCard = 0x100,
            PasswordOnlyOk = 0x200,
            ValidateUsername = 0x400,
            CompleteUserName = 0x800,
            Persist = 0x1000,
            ServerCredential = 0x4000,
            ExpectConfirmation = 0x20000,
            GenericCredentials = 0x40000,
            UsernameTargetCredentials = 0x80000,
            KeepUsername = 0x100000
        }

        internal const int CREDUI_MAX_MESSAGE_LENGTH = 100;
        internal const int CREDUI_MAX_CAPTION_LENGTH = 100;
        internal const int CREDUI_MAX_GENERIC_TARGET_LENGTH = 100;
        internal const int CREDUI_MAX_DOMAIN_TARGET_LENGTH = 100;
        internal const int CREDUI_MAX_USERNAME_LENGTH = 100;
        internal const int CREDUI_MAX_PASSWORD_LENGTH = 100;
        internal const int CREDUI_BANNER_HEIGHT = 60;
        internal const int CREDUI_BANNER_WIDTH = 320;

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        internal static extern bool DeleteObject(IntPtr hObject);

        [DllImport("credui.dll", EntryPoint = "CredUIPromptForCredentialsW", SetLastError = true, CharSet = CharSet.Unicode)]
        internal extern static CredUIReturnCodes CredUIPromptForCredentials(
            ref CredUIInfo creditUR,
            string targetName,
            IntPtr reserved1,
            int iError,
            StringBuilder userName,
            int maxUserName,
            StringBuilder password,
            int maxPassword,
            ref bool iSave,
            UserCredentialsDialogFlags flags);

        [DllImport("credui.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal extern static CredUIReturnCodes CredUIParseUserNameW(
            string userName,
            StringBuilder user,
            int userMaxChars,
            StringBuilder domain,
            int domainMaxChars);

        [DllImport("credui.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal extern static CredUIReturnCodes CredUIConfirmCredentialsW(string targetName, bool confirm);

        internal enum CredUIReturnCodes
        {
            NO_ERROR = 0,
            ERROR_CANCELLED = 1223,
            ERROR_NO_SUCH_LOGON_SESSION = 1312,
            ERROR_NOT_FOUND = 1168,
            ERROR_INVALID_ACCOUNT_NAME = 1315,
            ERROR_INSUFFICIENT_BUFFER = 122,
            ERROR_INVALID_PARAMETER = 87,
            ERROR_INVALID_FLAGS = 1004
        }

        internal struct CredUIInfo
        {
            internal CredUIInfo(IntPtr owner, string caption, string message, Image banner)
            {
                this.cbSize = Marshal.SizeOf(typeof(CredUIInfo));
                this.hwndParent = owner;
                this.pszCaptionText = caption;
                this.pszMessageText = message;

                if (banner != null)
                {
                    this.hbmBanner = new Bitmap(banner, Win32.CREDUI_BANNER_WIDTH, Win32.CREDUI_BANNER_HEIGHT).GetHbitmap();
                }
                else
                {
                    this.hbmBanner = IntPtr.Zero;
                }
            }

            internal int cbSize;
            internal IntPtr hwndParent;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pszMessageText;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string pszCaptionText;
            internal IntPtr hbmBanner;
        }
    }
}
