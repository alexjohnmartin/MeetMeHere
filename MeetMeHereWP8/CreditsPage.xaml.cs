using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace MeetMeHereWP8
{
    public partial class CreditsPage : PhoneApplicationPage
    {
        public CreditsPage()
        {
            InitializeComponent();
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton button = (HyperlinkButton)sender; 
            var task = new WebBrowserTask();
            task.Uri = new Uri(button.Tag.ToString(), UriKind.Absolute);
            task.Show();
        }
    }
}