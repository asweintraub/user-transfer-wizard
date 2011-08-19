using System;
using System.Management;
using System.Collections.ObjectModel;

namespace UserTransferWizard
{
    public class Account
    {
        public String Name { get; set; }
        public String SID { get; set; }

        public Account(String name, string sid)
        {
            Name = name;
            SID = sid;
        }
    }

    public class AccountList : Collection<Account>
    {
        public AccountList()
        {
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
                    this.Add(new Account(account["Caption"] as String, account["SID"] as String));
                }
            }
            if (this.Count == 0)
            {
                this.Add(new Account("You cannot currently modify any of the local accounts", null));
            }
        }
    }
}
