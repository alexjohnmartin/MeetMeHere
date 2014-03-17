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
using System.Windows.Media;
using System.Device.Location;
using System.Windows.Shapes;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;

////based on examples from...

//phone location
//http://developer.nokia.com/community/wiki/Get_Phone_Location_with_Windows_Phone_8

//showing maps control
//http://developer.nokia.com/resources/library/Lumia/maps-and-navigation/guide-to-the-wp8-maps-api.html

//REST API of the Nokia Here Maps
//https://developer.here.com/rest-apis/documentation/enterprise-map-image

//world map image
//http://www.publicdomainpictures.net/view-image.php?image=1967

//setting splash screen
//http://stackoverflow.com/questions/19450446/how-to-set-splash-screen-in-window-phone-8-application-development

//dealing with back button
//http://stackoverflow.com/questions/19578634/windows-phone-back-button-and-page-instance-creation

//live tiles
//http://dotnet.dzone.com/articles/how-add-primary-live-tile-your
//localizing the app tile
//http://msdn.microsoft.com/en-us/library/windowsphone/develop/ff967550%28v=vs.105%29.aspx
//updating tiles
//http://msdn.microsoft.com/en-us/library/windowsphone/develop/ff769548%28v=vs.105%29.aspx

//downloading an image
//http://stackoverflow.com/questions/7712160/how-do-i-download-images-jpg-via-a-webclient-and-save-to-isolated-storage-on-w

//sending an SMS
//http://stackoverflow.com/questions/13587507/programmatically-send-sms-in-windows-phone-8

//app bar images
//C:\Program Files (x86)\Microsoft SDKs\Windows Phone\v8.0\Icons\Dark

namespace MeetMeHereWP8
{
    public partial class MainPage : PhoneApplicationPage
    {
        const int mapStyle = 3; //rendered map style - hybrid view
        const int zoom = 18; //rendered map zoom level
        const int mapWidth = 600; //rendered map width
        const int mapHeight = 600; //rendered map height
        const string HereMapsAppId = "PC3CUQZkDFZ46i8ifPIL";
        const string HereMapsAppCode = "u_JokeYoH5JkfpvqL2CuFA";

        bool loading = true;
        Geolocator geolocator = null;
        GeoCoordinate coordinates = null;
        bool TileScheduleRunning = false; 
        string downloadUrl = string.Empty; 
        ApplicationBarIconButton smsButton;
        ApplicationBarIconButton emailButton;
        IsolatedStorageSettings appSettings;

        public MainPage()
        {
            InitializeComponent();
            appSettings = IsolatedStorageSettings.ApplicationSettings;

            geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 10;

            this.Loaded += MainPage_Loaded;
            ApplicationBar = new ApplicationBar();

            try
            {
                if (appSettings["sendCountDate"] == null ||
                    (string)appSettings["sendCountDate"] != DateTime.Now.Date.ToShortDateString())
                {
                    appSettings["sendCountDate"] = DateTime.Now.Date.ToShortDateString();
                    appSettings["sendCount"] = 0;
                }
            }
            catch
            {
                appSettings["sendCountDate"] = DateTime.Now.Date.ToShortDateString();
                appSettings["sendCount"] = 0;
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (loading)
            {
                loading = false;
                GetLocation_Click(sender, e);
            }
        }

        private async void GetLocation_Click(object sender, EventArgs e)
        {
            //Check for the user agreement in use his position. If not, method returns.
            if ((bool)appSettings["LocationConsent"] != true)
            {
                // The user has opted out of Location.
                LoadingBlock.Visibility = System.Windows.Visibility.Collapsed; 
                ErrorBlock.Visibility = System.Windows.Visibility.Visible;
                ErrorText.Text = AppResources.ErrorLocationDisabled; 
                return;
            }

            HereMap.Layers.Clear();
            BuildLocalizedApplicationBar(false); 
            LoadingBlock.Visibility = System.Windows.Visibility.Visible; 

            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 10;

            try
            {
                Geoposition geoposition = await geolocator.GetGeopositionAsync(
                     maximumAge: TimeSpan.FromMinutes(5),
                     timeout: TimeSpan.FromSeconds(10)
                    );

                coordinates = new GeoCoordinate(geoposition.Coordinate.Latitude, geoposition.Coordinate.Longitude);
                HereMap.Center = coordinates;
                HereMap.ZoomLevel = 18;
                LoadingBlock.Visibility = System.Windows.Visibility.Collapsed; 

                DrawMapMarkers(coordinates);
                BuildLocalizedApplicationBar(true);
                StartDownloadMapImage(coordinates); 
            }
            //If an error is catch 2 are the main causes: the first is that you forgot to include ID_CAP_LOCATION in your app manifest. 
            //The second is that the user doesn't turned on the Location Services
            catch (Exception ex)
            {
                //if ((uint)ex.HResult == 0x80004004)
                //{
                    LoadingBlock.Visibility = System.Windows.Visibility.Collapsed;
                    ErrorBlock.Visibility = System.Windows.Visibility.Visible;
                    ErrorText.Text = AppResources.ErrorLocationDisabledInPhoneSettings; 
                //}
                //else
                //{
                    // something else happened during the acquisition of the location
                //}
            }
        }

        private void StartDownloadMapImage(GeoCoordinate coordinates)
        {
            var downloader = new DownloadAndSaveImage();
            downloader.DownloadMapImages(coordinates.Latitude, coordinates.Longitude, HereMap.ZoomLevel, GetStyleNumber(HereMap.CartographicMode), HereMapsAppId, HereMapsAppCode); 
        }

        private void webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null) return;
                if (e.Cancelled == true) return;
                
                var image = new BitmapImage(new Uri("" + downloadUrl));
                var wb = new WriteableBitmap(image);
                using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream isostream = iso.CreateFile(@"Assets\Tiles\mapview.jpg"))
                    {
                        Extensions.SaveJpeg(wb, isostream, wb.PixelWidth, wb.PixelHeight, 0, 85);
                        isostream.Close();
                    }
                }
            }
            catch
            {
            }
        }

        private void DrawMapMarkers(GeoCoordinate coordinates)
        {
            MapLayer mapLayer = new MapLayer();
         
            // Draw marker for current position
            if (coordinates != null)
            {
                //DrawAccuracyRadius(mapLayer);
                DrawMapMarker(coordinates, Colors.Red, mapLayer);
            }
         
            HereMap.Layers.Add(mapLayer);
        }

        private void DrawMapMarker(GeoCoordinate coordinate, Color color, MapLayer mapLayer)
        {
            // Create a map marker
            //Polygon polygon = new Polygon();
            //polygon.Points.Add(new Point(0, 0));
            //polygon.Points.Add(new Point(0, 75));
            //polygon.Points.Add(new Point(25, 0));
            //polygon.Fill = new SolidColorBrush(color);

            // Enable marker to be tapped for location information
            //polygon.Tag = new GeoCoordinate(coordinate.Latitude, coordinate.Longitude);
            //polygon.MouseLeftButtonUp += new MouseButtonEventHandler(Marker_Click);

            int scale = 15; 

            var circle1 = new Ellipse();
            circle1.Width = 3 * scale;
            circle1.Height = 3 * scale;
            circle1.Fill = new SolidColorBrush(color);
            
            var circle2 = new Ellipse();
            circle2.Width = 2 * scale;
            circle2.Height = 2 * scale;
            circle2.Fill = new SolidColorBrush(Colors.White);

            var circle3 = new Ellipse();
            circle3.Width = 1 * scale;
            circle3.Height = 1 * scale;
            circle3.Fill = new SolidColorBrush(color); 

            // Create a MapOverlay and add marker
            MapOverlay overlay1 = new MapOverlay();
            overlay1.Content = circle1;
            overlay1.GeoCoordinate = new GeoCoordinate(coordinate.Latitude, coordinate.Longitude);
            overlay1.PositionOrigin = new Point(0.5, 0.5);
            mapLayer.Add(overlay1);
            MapOverlay overlay2 = new MapOverlay();
            overlay2.Content = circle2;
            overlay2.GeoCoordinate = new GeoCoordinate(coordinate.Latitude, coordinate.Longitude);
            overlay2.PositionOrigin = new Point(0.5, 0.5);
            mapLayer.Add(overlay2);
            MapOverlay overlay3 = new MapOverlay();
            overlay3.Content = circle3;
            overlay3.GeoCoordinate = new GeoCoordinate(coordinate.Latitude, coordinate.Longitude);
            overlay3.PositionOrigin = new Point(0.5, 0.5);
            mapLayer.Add(overlay3);
        }

        private void BuildLocalizedApplicationBar(bool locationFound)
        {
            ApplicationBar = new ApplicationBar();

            if (locationFound)
            {
                // Create a new button and set the text value to the localized string from AppResources.
                var relocateButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.refresh.png", UriKind.Relative));
                relocateButton.Text = AppResources.AppBarRefreshButtonText;
                relocateButton.Click += GetLocation_Click;
                ApplicationBar.Buttons.Add(relocateButton);

                var emailButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.email.png", UriKind.Relative));
                emailButton.Text = AppResources.AppBarEmailButtonText;
                emailButton.Click += SendEmail_Click;
                ApplicationBar.Buttons.Add(emailButton);

                var smsButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.phone.png", UriKind.Relative));
                smsButton.Text = AppResources.AppBarSmsButtonText;
                smsButton.Click += SendSms_Click; 
                ApplicationBar.Buttons.Add(smsButton);
            }

            // Create a new menu item with the localized string from AppResources.
            var aboutMenuItem = new ApplicationBarMenuItem(AppResources.AppBarAboutMenuItemText);
            aboutMenuItem.Click += aboutMenuItem_Click;
            ApplicationBar.MenuItems.Add(aboutMenuItem);

            //TODO:settings
        }

        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private void SendSms_Click(object sender, EventArgs e)
        {
            IncrementSendCount(); 
            var geocoding = new GeocodingHelper();
            geocoding.GetGeocodingInfo(coordinates.Latitude, coordinates.Longitude, SendSms); 
        }

        private void SendSms(GeocodingInfo info)
        {
            SmsComposeTask smsComposeTask = new SmsComposeTask();

            //smsComposeTask.To = "";
            smsComposeTask.Body = string.Format(AppResources.SmsTemplate, info.AddressLabel); 
            smsComposeTask.Show();
        }

        private void SendEmail_Click(object sender, EventArgs e)
        {
            IncrementSendCount(); 
            var geocoding = new GeocodingHelper();
            geocoding.GetGeocodingInfo(coordinates.Latitude, coordinates.Longitude, SendEmail); 
        }

        private void SendEmail(GeocodingInfo info)
        {
            var mapCoordinates = HereMap.Center;
            var mapStyle = GetStyleNumber(HereMap.CartographicMode);
            var mapZoom = HereMap.ZoomLevel; 

            var email = new EmailComposeTask();
            email.Subject = AppResources.EmailSubject;
            email.Body = string.Format(AppResources.EmailBody, 
                mapCoordinates.Latitude, 
                mapCoordinates.Longitude, 
                mapStyle, 
                mapZoom, 
                mapWidth,
                mapHeight, 
                HereMapsAppId, 
                HereMapsAppCode, 
                info.AddressLabel);

            email.Show();
        }

        private void IncrementSendCount()
        {
            appSettings["sendCount"] = (int)appSettings["sendCount"] + 1;
            TileHelper.SetTileData((int)appSettings["sendCount"]); 
        }
        
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
                {
                    //User already gave us agreement for using their position
                    if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] == true)
                    {
                        return;
                    }
                }

                PromptIfWeCanUseUsersLocation();
            }
            if (e.NavigationMode == NavigationMode.Back)
            {
                // do anything specific for back navigation here.
            }
        }

        private void PromptIfWeCanUseUsersLocation()
        {
            //If they didn't we ask for it
            MessageBoxResult result = MessageBox.Show(AppResources.LocationPrivacyPolicyBody, AppResources.LocationPrivacyPolicyTitle, MessageBoxButton.OKCancel);
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

        private int GetStyleNumber(MapCartographicMode mapCartographicMode)
        {
            switch (mapCartographicMode)
            {
                case MapCartographicMode.Road: return 0;
                case MapCartographicMode.Aerial: return 1;
                case MapCartographicMode.Hybrid: return 3;
                case MapCartographicMode.Terrain: return 2;
                default: return 3;
            }
        }
    }
}