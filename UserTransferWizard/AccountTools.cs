using System;
using System.Management;
using System.Collections.ObjectModel;
using System.Security.Principal;

namespace UserTransferWizard
{
    class AccountNotFoundException : Exception
    {
        public AccountNotFoundException() { }
        public AccountNotFoundException(String error) : base(error) { }
    }

    public class AccountTools
    {
        public static Collection<String> getLocalAccountList()
        {
            Collection<String> list = new Collection<String>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_UserAccount WHERE Disabled = FALSE AND Lockout = FALSE AND LocalAccount = TRUE");
            ManagementObjectCollection accounts = searcher.Get();
            foreach (ManagementObject account in accounts)
            {
                ManagementObject profile = null;
                Boolean loaded = false;
                try
                {
                    profile = new ManagementObject("\\\\.\\root\\cimv2:Win32_UserProfile.SID='" + account["SID"] + "'");
                    loaded = (Boolean)profile["Loaded"];
                } catch (Exception) {}

                if (!loaded)
                {
                    list.Add(account["Caption"] as String);
                }
            }
            if (list.Count == 0)
            {
                list.Add("You cannot currently modify any of the local accounts");
            }
            return list;
        }

        public static Collection<String> getProfileList()
        {
            Collection<String> list = new Collection<String>();
            ManagementClass mgmt = new ManagementClass("Win32_UserProfile");

            // Get the resulting collection and loop through it
            foreach (ManagementObject profile in mgmt.GetInstances())
            {
                if ((Boolean)profile["Loaded"])
                {
                    continue;
                }
                String SID = profile["SID"] as String;

                SecurityIdentifier SecID = new SecurityIdentifier(SID);
                try
                {
                    NTAccount nt = SecID.Translate(Type.GetType("System.Security.Principal.NTAccount")) as NTAccount;
                    list.Add(nt.ToString());
                }
                catch (SystemException) {}
            }

            if (list.Count == 0)
            {
                list.Add("No profiles can currently be moved.");
            }

            return list;
        }

        public static String getSIDForNTAccount(String ntaccount)
        {
            NTAccount nt = new NTAccount(ntaccount);
            SecurityIdentifier sid = null;
            try
            {
                sid = nt.Translate(Type.GetType("System.Security.Principal.SecurityIdentifier")) as SecurityIdentifier;
                return sid.ToString();
            }
            catch (SystemException)
            {
                throw new AccountNotFoundException(ntaccount + " is not a valid domain account.");
            }
        }
    }
}
