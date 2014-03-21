using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;

namespace MeetMeHereWP8
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            this.Loaded += SettingsPage_Loaded;
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            AllowLocationCheckbox.IsChecked = (bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"]; 
        }

        private void AllowLocationCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = AllowLocationCheckbox.IsChecked;
        }

        private void AllowLocationCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = AllowLocationCheckbox.IsChecked;
        }
    }
}