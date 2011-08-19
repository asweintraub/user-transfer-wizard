using System;
using System.Management;
using System.Threading;
using System.ComponentModel;
using System.DirectoryServices;

namespace UserTransferWizard {
    public delegate void TransferComplete(TransferStatus status);

    public enum TransferStatus { COMPLETE, PROFILE_EXISTS,
        BAD_USER1, BAD_USER2, SRC_IS_DEST, FAILED_OTHER, NO_PROFILE };

    public class ProfileTransfer
    {
	    public static void TransferWithCallback(String user1, String user2, TransferComplete callback,
            Boolean overwrite = false, Boolean deleteProfile = false,
            String computer = ".", Boolean deleteAccount = false)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                callback((TransferStatus)e.Result);
            };
            bw.DoWork += delegate(Object sender, DoWorkEventArgs args)
            {
                args.Result = Transfer(user1, user2, overwrite, deleteProfile, computer, deleteAccount);
            };
            bw.RunWorkerAsync();
        }

        public static TransferStatus Transfer(String user1, String user2,
            Boolean overwrite = false, Boolean deleteProfile = false,
            String computer = ".", Boolean deleteAccount = false)
        {
                String src, dest;
                try
                {
                    src = AccountTools.getSIDForNTAccount(user1);
                }
                catch (AccountNotFoundException)
                {
                    return TransferStatus.BAD_USER1;
                }

                try
                {
                    dest = AccountTools.getSIDForNTAccount(user2);
                }
                catch (AccountNotFoundException)
                {
                    return TransferStatus.BAD_USER2;
                }

                if (src.Equals(dest))
                {
                    return TransferStatus.SRC_IS_DEST;
                }

                try
                {
                    ManagementObject profile = new ManagementObject("\\\\" + computer + "\\root\\cimv2:Win32_UserProfile.SID='" + src + "'");
                    profile.InvokeMethod("ChangeOwner", new object[] { dest, (overwrite) ? 1 : 0, (deleteProfile) ? 1 : 0 });
                }
                catch (System.Runtime.InteropServices.COMException e)
                {
                    if (e.ErrorCode == -2147024713)
                    {
                        return TransferStatus.PROFILE_EXISTS;
                    }
                    else
                    {
                        return TransferStatus.FAILED_OTHER;
                    }
                }
                catch (System.IO.FileNotFoundException)
                {
                    return TransferStatus.NO_PROFILE;
                }

                if (deleteAccount)
                {
                    try
                    {
                        DirectoryEntry AD = new DirectoryEntry("WinNT://" + computer);
                        AD.Invoke("Delete", new object[] { "user", user1 });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.ReadLine();
                    }
                }

                return TransferStatus.COMPLETE;
        }

        public static TransferStatus CheckAccounts(String user1, String user2)
        {
            String src, dest;
            try
            {
                src = AccountTools.getSIDForNTAccount(user1);
            }
            catch (AccountNotFoundException)
            {
                return TransferStatus.BAD_USER1;
            }

            try
            {
                dest = AccountTools.getSIDForNTAccount(user2);
            }
            catch (AccountNotFoundException)
            {
                return TransferStatus.BAD_USER2;
            }

            if (src.Equals(dest))
            {
                return TransferStatus.SRC_IS_DEST;
            }

            return TransferStatus.COMPLETE;
        }
    }
}