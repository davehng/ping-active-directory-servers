using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

using System.DirectoryServices.ActiveDirectory;

namespace PingAllDcs
{
    public class Program
    {
        private static string Username { get; set; }
        private static string UserDomain { get; set; }
        private static SecureString Password { get; set; }

        static void Main(string[] args)
        {
            if (!PromptForCredentials())
            {
                return;
            }

            // every ~5 minutes
            const int delay = 300000;

            while (true)
            {
                Console.WriteLine("Timestamp,DomainControllerName,IsGlobalCatalog,OS,Site,UserValidates");

                foreach (var dc in FindAllDomainControllers())
                {
                    Console.Write(string.Empty + DateTime.Now + "," + dc.Name + ",");

                    Console.Write(dc.IsGlobalCatalog().ToString());
                    Console.Write(",");

                    Console.Write(dc.OSVersion);
                    Console.Write(",");

                    Console.Write(dc.SiteName);
                    Console.Write(",");

                    var result = ValidateUserOnDomainController(dc.Name);
                    Console.Write(result.Value);
                    if (result != ValidationResult.Success && result.Error != null)
                    {
                        Console.Write(":");
                        Console.Write(result.Error.ToString().Replace(Environment.NewLine, "* ")); // save exception but try not to run into multiple lines
                    }

                    Console.WriteLine();
                }

                System.Threading.Thread.Sleep(delay);
            }
        }

        private static bool PromptForCredentials()
        {
            var credentialInfo = new Win32.CredUIInfo(
                Process.GetCurrentProcess().MainWindowHandle, 
                "Credentials",
                "Please enter the credentials of the user to test", 
                null);

            var username = new StringBuilder(Win32.CREDUI_MAX_USERNAME_LENGTH);
            var password = new StringBuilder(Win32.CREDUI_MAX_PASSWORD_LENGTH);
            var saveChecked = false;

            const Win32.UserCredentialsDialogFlags flags = 
                Win32.UserCredentialsDialogFlags.GenericCredentials |
                Win32.UserCredentialsDialogFlags.AlwaysShowUI |
                Win32.UserCredentialsDialogFlags.ExpectConfirmation |
                Win32.UserCredentialsDialogFlags.ExcludesCertificates |
                Win32.UserCredentialsDialogFlags.DoNotPersist;
            
            try
            {
                var result = Win32.CredUIPromptForCredentials(
                    ref credentialInfo,
                    "PingAllDcs",
                    IntPtr.Zero,
                    0,
                    username,
                    Win32.CREDUI_MAX_USERNAME_LENGTH,
                    password,
                    Win32.CREDUI_MAX_PASSWORD_LENGTH,
                    ref saveChecked,
                    flags);

                switch (result)
                {
                    case Win32.CredUIReturnCodes.NO_ERROR:
                        SetUsername(username);
                        SetPassword(password);
                        return true;
                    case Win32.CredUIReturnCodes.ERROR_CANCELLED:
                        return false;
                    default:
                        throw new InvalidOperationException("Failed: " + result);
                }
            }
            finally
            {
                // overwrite username and password data within stringbuilders
                username.Remove(0, username.Length);
                password.Remove(0, password.Length);
            }
        }

        private static void SetPassword(StringBuilder password)
        {
            Password = new SecureString();

            for (int i = 0; i < password.Length; i++)
            {
                Password.AppendChar(password[i]);
            }

            Password.MakeReadOnly();
        }

        private static void SetUsername(StringBuilder username)
        {
            var user = new StringBuilder(Win32.CREDUI_MAX_USERNAME_LENGTH);
            var domain = new StringBuilder(Win32.CREDUI_MAX_DOMAIN_TARGET_LENGTH);
            var result = Win32.CredUIParseUserNameW(
                username.ToString(), 
                user, 
                Win32.CREDUI_MAX_USERNAME_LENGTH, 
                domain, 
                Win32.CREDUI_MAX_DOMAIN_TARGET_LENGTH);

            if (result == Win32.CredUIReturnCodes.NO_ERROR)
            {
                Username = user.ToString();
                UserDomain = domain.ToString();
            }
            else
            {
                Username = username.ToString();
                UserDomain = Environment.MachineName;
            }
        }

        private static IEnumerable<DomainController> FindAllDomainControllers()
        {
            return Domain.GetCurrentDomain()
                .FindAllDomainControllers()
                .Cast<DomainController>();
        }

        private static ValidationResult ValidateUserOnDomainController(string dc)
        {
            var passwordUnicodeStringPtr = Marshal.SecureStringToGlobalAllocUnicode(Password);

            try
            {
                using (var pc = new PrincipalContext(ContextType.Domain, dc))
                {
                    return pc.ValidateCredentials(Username, Marshal.PtrToStringUni(passwordUnicodeStringPtr))
                        ? ValidationResult.Success
                        : ValidationResult.Failed;
                }
            }
            catch (Exception ex)
            {
                return new ValidationResult(ex);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(passwordUnicodeStringPtr);
            }
        }
    }
}
