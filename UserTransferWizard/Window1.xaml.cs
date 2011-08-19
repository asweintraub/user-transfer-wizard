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
        private Window2 progress = null;

        private void init()
        {
            domainRadioButton.IsChecked = true;
            Profile.ItemsSource = AccountTools.getProfileList();
            DestAcct.ItemsSource = AccountTools.getLocalAccountList();
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
            String SrcName = Profile.SelectedItem as String;

            String DestName = null;
            if (_domain)
            {
                DestName = domain.Text + "\\" + uname.Text;
            }
            else
            {
                DestName = DestAcct.SelectedItem as String;
            }

            TransferStatus status = ProfileTransfer.CheckAccounts(SrcName, DestName);
            if (status != TransferStatus.COMPLETE)
            {
                TransferComplete(status);
                return;
            }

            MessageBoxResult result = MessageBox.Show(this, "The profile belonging to " + SrcName + " will be transfered to " + DestName
                + ". Are you sure you want to continue?", "Confirm", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                progress = new Window2();
                progress.Owner = this;
                progress.Show();
                ProfileTransfer.TransferWithCallback(SrcName, DestName, TransferComplete,
                    _replace.IsChecked.GetValueOrDefault(), _deleteProfile.IsChecked.GetValueOrDefault(),
                    ".", _deleteAcct.IsChecked.GetValueOrDefault());
            }
        }

        public void TransferComplete(TransferStatus status)
        {
            if (progress != null)
            {
                progress.Hide();
                progress.Close();
                progress = null;
            }
            switch (status)
            {
                case TransferStatus.BAD_USER1:
                    MessageBox.Show(this, "Please choose a valid profile from which to copy settings.");
                    break;
                case TransferStatus.BAD_USER2:
                    if (_domain)
                    {
                        MessageBox.Show(this, "The domain account that you entered cannot be found. " +
                            "Make sure you are connected to the proper domain and try again.");
                    }
                    else
                    {
                        MessageBox.Show(this, "Please choose a valid local account to copy settings to.");
                    }
                    break;
                case TransferStatus.SRC_IS_DEST:
                    MessageBox.Show(this, "The source and destination profiles cannot be the same.");
                    break;
                case TransferStatus.PROFILE_EXISTS:
                    MessageBox.Show(this, "The destination account already has a profile. Please check the option to overwrite the destination profile and try again."
                        + " (Note that the existing profile will NOT be deleted - only made inactive.)");
                    break;
                case TransferStatus.FAILED_OTHER:
                    MessageBox.Show(this, "Profile transfer failed.  Make sure that you're running this program as an administrator.");
                    break;
                case TransferStatus.NO_PROFILE:
                    MessageBox.Show(this, "The source user does not have a valid user profile.  Please try again.");
                    break;
                case TransferStatus.COMPLETE:
                    MessageBox.Show(this, "Profile successfully transfered.");
                    init();
                    break;
            }
        }

        private void _replace_Click(object sender, RoutedEventArgs e)
        {
            if ((e.Source as CheckBox).IsChecked == false)
            {
                _deleteProfile.IsChecked = false;
            }
        }
    }
}
