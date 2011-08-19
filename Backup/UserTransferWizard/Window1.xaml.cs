using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;

namespace UserTransferWizard
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private Boolean _domain = true;

        private void init()
        {
            domainRadioButton.IsChecked = true;
            Profile.ItemsSource = new ProfileList();
            DestAcct.ItemsSource = new AccountList();
            Profile.SelectedIndex = 0;
            DestAcct.SelectedIndex = 0;
        }

        public Window1()
        {
            InitializeComponent();
            init();
        }

        private void domainRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _domain = true;
            DestAcct.IsEnabled = false;
            domain.IsEnabled = true;
            uname.IsEnabled = true;
        }

        private void localRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _domain = false;
            DestAcct.IsEnabled = true;
            domain.IsEnabled = false;
            uname.IsEnabled = false;
        }

        private void executeButton_Click(object sender, RoutedEventArgs e)
        {
            Profile src = Profile.SelectedItem as Profile;
            String SrcSID = src.SID;
            String SrcName = src.Owner;

            if (SrcSID == null)
            {
                MessageBox.Show(this, "Please choose a valid account from which to copy settings.");
                return;
            }

            String DestSID = null;
            String DestName = null;
            if (_domain)
            {
                DomainAccount da = new DomainAccount(domain.Text, uname.Text);
                if (da.SID == null)
                {
                    MessageBox.Show(this, "The domain account that you entered cannot be found. " +
                        "Make sure you are connected to the proper domain and try again.");
                    return;
                }
                DestSID = da.SID.ToString();
                DestName = da.Caption;
            }
            else
            {
                Account acctDest = DestAcct.SelectedItem as Account;
                DestSID = acctDest.SID;
                DestName = acctDest.Name;
                if (DestSID == null)
                {
                    MessageBox.Show(this, "Please choose a valid local account to copy settings to.");
                    return;
                }
            }

            if (SrcSID == DestSID)
            {
                MessageBox.Show("The source and destination profiles cannot be the same.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(this, "The profile belonging to " + SrcName + " will be transfered to " + DestName
                + ". Are you sure you want to continue?", "Confirm", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                int transferResult = transferProfile(SrcSID, DestSID);
                if (transferResult == 0)
                {
                    MessageBox.Show(this, "Profile successfully transfered.");
                    init();
                }
                else if (transferResult == 1)
                {
                    result = MessageBox.Show("The destination account already has a profile.  Do you want to replace it?"
                        + " (Note that the existing profile will NOT be deleted - only made inactive.)", "Confirm", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        if (transferProfile(SrcSID, DestSID, true) == 0)
                        {
                            MessageBox.Show(this, "Profile successfully transfered.");
                            init();
                        }
                        else
                        {
                            MessageBox.Show("Profile transfer failed.  Make sure that you're running this program as an administrator.");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Profile transfer failed.  Make sure that you're running this program as an administrator.");
                }
            }
        }

        private int transferProfile(String src, String dest)
        {
            return transferProfile(src, dest, false);
        }

        private int transferProfile(String src, String dest, bool force)
        {
            try
            {
                ManagementObject profile = new ManagementObject("\\\\.\\root\\cimv2:Win32_UserProfile.SID='" + src + "'");
                profile.InvokeMethod("ChangeOwner", new object[]{dest, (force)?1:0});
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                if (e.ErrorCode == -2147024713)
                {
                    return 1;
                }
                return -1;
            }
            return 0;
        }
    }
}
