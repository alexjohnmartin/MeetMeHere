using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MeetMeHereWP8.Resources;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Maps.Controls;

namespace MeetMeHereWP8
{
    public partial class MainPage : PhoneApplicationPage
    {
        Geolocator geolocator = null;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();

            geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 10;

            this.Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            OneShotLocation_Click(); 
        }

        private async void OneShotLocation_Click()
        {
            //Check for the user agreement in use his position. If not, method returns.
            if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
            {
                // The user has opted out of Location.
                return;
            }

            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            try
            {
                Geoposition geoposition = await geolocator.GetGeopositionAsync(
                     maximumAge: TimeSpan.FromMinutes(5),
                     timeout: TimeSpan.FromSeconds(10)
                    );

                //With this 2 lines of code, the app is able to write on a Text Label the Latitude and the Longitude, given by {{Icode|geoposition}}

                HereMap.Center = new System.Device.Location.GeoCoordinate(geoposition.Coordinate.Latitude, geoposition.Coordinate.Longitude);
                HereMap.ZoomLevel = 15;
                LoadingText.Visibility = System.Windows.Visibility.Collapsed; 

                //TODO:put pin on the map with this current location


            }
            //If an error is catch 2 are the main causes: the first is that you forgot to include ID_CAP_LOCATION in your app manifest. 
            //The second is that the user doesn't turned on the Location Services
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    MessageBox.Show("location is disabled in phone settings.");
                }
                //else
                {
                    // something else happened during the acquisition of the location
                }
            }
        }



        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}



        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
            {
                //User already gave us his agreement for using his position
                if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] == true)

                    return;
                //If he didn't we ask for it
                else
                {
                    MessageBoxResult result =
                                MessageBox.Show("Can I use your position?",
                                "Location",
                                MessageBoxButton.OKCancel);

                    if (result == MessageBoxResult.OK)
                    {
                        IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;
                    }
                    else
                    {
                        IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = false;
                    }

                    IsolatedStorageSettings.ApplicationSettings.Save();
                }
            }

                //Ask for user agreement in using his position
            else
            {
                MessageBoxResult result =
                            MessageBox.Show("Can I use your position?",
                            "Location",
                            MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = false;
                }

                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }


    }
}