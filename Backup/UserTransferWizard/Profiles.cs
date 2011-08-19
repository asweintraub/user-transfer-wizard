using System;
using System.Management;
using System.Collections.ObjectModel;
using System.Security.Principal;

namespace UserTransferWizard
{
    public class Profile
    {
        public String SID {get; set;}
        public String Owner {get; set;}

        public Profile(String SID, String Owner)
        {
            this.SID = SID;
            this.Owner = Owner;
        }
    }

    public class ProfileList : Collection<Profile>
    {
        public ProfileList()
        {
            ManagementClass mgmt = new ManagementClass("Win32_UserProfile");

            // Get the resulting collection and loop through it
            foreach (ManagementObject profile in mgmt.GetInstances())
            {
                if ((Boolean)profile["Loaded"])
                {
                    continue;
                }
                String SID = profile["SID"] as String;
                /*ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_UserAccount WHERE SID = \"" + SID + "\" AND Disabled = FALSE");
                ManagementObjectCollection accounts = searcher.Get();
                foreach (ManagementObject account in accounts)
                {
                    this.Add(new Profile(SID, account["Caption"] as String));
                    break;
                }*/
                SecurityIdentifier SecID = new SecurityIdentifier(SID);
                NTAccount nt = null;
                try
                {
                    nt = SecID.Translate(Type.GetType("System.Security.Principal.NTAccount")) as NTAccount;
                }
                catch (SystemException)
                {
                    nt = null;
                }
                if (nt != null)
                {
                    this.Add(new Profile(SID, nt.ToString()));
                }

            }

            if (this.Count == 0)
            {
                this.Add(new Profile(null, "No profiles can currently be moved"));
            }
        }
    }
}
