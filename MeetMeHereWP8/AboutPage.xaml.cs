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
using RateMyApp.Helpers;

//handling screen orientation changes
//http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj207002%28v=vs.105%29.aspx

namespace MeetMeHereWP8
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();
            OrientationChanged += AboutPage_OrientationChanged;
        }

        void AboutPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (e.Orientation == PageOrientation.Landscape ||
                e.Orientation == PageOrientation.LandscapeLeft ||
                e.Orientation == PageOrientation.LandscapeRight)
            {
                Grid.SetColumn(EmailButton, 2);
                Grid.SetRow(EmailButton, 0);

                Grid.SetColumn(StoreButton, 0);
                Grid.SetRow(StoreButton, 1);

                Grid.SetColumn(CreditsButton, 1);
                Grid.SetRow(CreditsButton, 1);
            }
            else
            {
                Grid.SetColumn(EmailButton, 0);
                Grid.SetRow(EmailButton, 1);

                Grid.SetColumn(StoreButton, 1);
                Grid.SetRow(StoreButton, 1);

                Grid.SetColumn(CreditsButton, 0);
                Grid.SetRow(CreditsButton, 2);
            }
        }

        public void TwitterButton_Click(object sender, EventArgs e)
        {
            var task = new WebBrowserTask
            {
                Uri = new Uri("https://twitter.com/AlexJohnMartin", UriKind.Absolute)
            };
            task.Show(); 
        }

        public void StoreButton_Click(object sender, EventArgs e)
        {
            var currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture; 
            var task = new WebBrowserTask
            {
                Uri = new Uri(string.Format("http://www.windowsphone.com/{0}/store/publishers?publisherId=nocturnal%2Btendencies&appId=63cb6767-4940-4fa1-be8c-a7f58e455c3b", currentCulture.Name), UriKind.Absolute)
            };
            task.Show();
        }

        public void ReviewButton_Click(object sender, EventArgs e)
        {
            FeedbackHelper.Default.Reviewed();
            var marketplace = new MarketplaceReviewTask();
            marketplace.Show();
        }

        public void EmailButton_Click(object sender, EventArgs e)
        {
            var email = new EmailComposeTask();
            email.Subject = "Feedback for the Meet Me Here application";
            email.Show();
        }
 
        public void CreditsButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/CreditsPage.xaml", UriKind.Relative)); 
        }
    }
}