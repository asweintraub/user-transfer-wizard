using System;
using System.Security.Principal;

namespace UserTransferWizard
{
    public class DomainAccount
    {
        public String Caption { get; set; }
        public SecurityIdentifier SID { get; set; }
        public DomainAccount(String domain, String uname)
        {
            Caption = domain + "\\" + uname;
            NTAccount nt = new NTAccount(Caption);
            SecurityIdentifier sid = null;
            try{
                sid = nt.Translate(Type.GetType("System.Security.Principal.SecurityIdentifier")) as SecurityIdentifier;
            } catch (SystemException) {
                sid = null;
            }
            SID = sid;
        }
    }
}