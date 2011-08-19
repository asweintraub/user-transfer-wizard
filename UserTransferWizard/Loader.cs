using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UserTransferWizard
{
    class Loader
    {
        [System.STAThreadAttribute()]
        public static void Main(String[] args)
        {
            if (args.Length == 0)
            {
                ConsoleManager.CloseConsole();
                App app = new App();
                app.InitializeComponent();
                app.Run();
            }
            else
            {
                RunConsole(args);
            }
        }

        static void RunConsole(String[] args)
        {
            Boolean deleteAccount = true;
            Boolean overwrite = false;
            Boolean deleteProfile = false;
            String user1 = null;
            String user2 = null;
            String computer = ".";
            foreach (String arg in args)
            {
                switch(arg)
                {
                    case "/?":
                        showHelp();
                        return;
                    case "/k":
                        deleteAccount = false;
                        break;
                    case "/y":
                        overwrite = true;
                        break;
                    case "/d":
                        deleteProfile = true;
                        break;
                    default:
                        if (arg.StartsWith("/c:"))
                        {
                            computer = arg.Substring(3);
                        }
                        else if (user1 == null)
                        {
                            user1 = arg;
                        }
                        else if (user2 == null)
                        {
                            user2 = arg;
                        }
                        else
                        {
                            showHelp();
                            return;
                        }
                        break;
                }
            }
            if (user1 == null)
            {
                showHelp();
                return;
            }
            else if (user1.Contains("/"))
            {
                user1 = user1.Replace("/", "\\");
            }
            else if (!user1.Contains("\\"))
            {
                user1 = Environment.MachineName + "\\" + user1;
            }

            if (user2 == null)
            {
                showHelp();
                return;
            }
            else if (user2.Contains("/"))
            {
                user2 = user2.Replace("/", "\\");
            }
            else if (!user2.Contains("\\"))
            {
                user2 = Environment.MachineName + "\\" + user2;
            }

            Console.Out.WriteLine("Please wait while your user profile is transferred.\n");
            TransferStatus status = ProfileTransfer.Transfer(user1, user2, overwrite, deleteProfile, computer, deleteAccount);

            switch (status)
            {
                case TransferStatus.BAD_USER1:
                    Console.Out.WriteLine("The source account that you entered is not valid.  Please try again.");
                    break;
                case TransferStatus.BAD_USER2:
                    Console.Out.WriteLine("The destination account that you entered is not valid.  Please try again.");
                    break;
                case TransferStatus.SRC_IS_DEST:
                    Console.Out.WriteLine("The source and destination profiles cannot be the same.");
                    break;
                case TransferStatus.PROFILE_EXISTS:
                    Console.Out.WriteLine("The destination account already has a profile. Please use /y to overwrite the destination profile.");
                    break;
                case TransferStatus.FAILED_OTHER:
                    Console.Out.WriteLine("Profile transfer failed.  Make sure that you're running this program as an administrator.");
                    break;
                case TransferStatus.NO_PROFILE:
                    Console.Out.WriteLine("The source user does not have a valid user profile.  Please try again.");
                    break;
                case TransferStatus.COMPLETE:
                    Console.Out.WriteLine("Profile successfully transfered.");
                    break;
            }
        }

        static void showHelp()
        {
            Console.Out.WriteLine("Syntax");
            Console.Out.WriteLine("\tmoveuser.exe [DOMAIN/]user1 [DOMAIN/]user2 [/c:computer] [/k] [/y] [/d]");
            Console.Out.WriteLine("Key:");
            Console.Out.WriteLine("\tuser1\tThe existing user (who has a local profile)");
            Console.Out.WriteLine("\t\tSpecify domain users in 'DOMAIN/user' format");
            Console.Out.WriteLine("\t\tor just 'user' for a local account.");
            Console.Out.WriteLine("\tuser2\tThe user account that will inherit the user1 profile.");
            Console.Out.WriteLine("\t\tThis account must already exist.");
            Console.Out.WriteLine("\t\tSpecify domain users in DOMAIN/user format");
            Console.Out.WriteLine("\t\tor just 'user' for a local account.");
            Console.Out.WriteLine("\t/c:computer\tThe computer on which to make the changes.");
            Console.Out.WriteLine("\t/k\tKeep user account user1 (only applies to local users)");
            Console.Out.WriteLine("\t/y\tOverwrite an existing profile for user2");
            Console.Out.WriteLine("\t/d\tDelete an existing profile for user2");
            Console.Out.WriteLine("\t\t(only works in conjunction with /y)");
        }
    }
}
