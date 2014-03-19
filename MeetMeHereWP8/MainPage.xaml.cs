using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Maps.Controls;
using System.Windows.Media;
using System.Device.Location;
using System.Windows.Shapes;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Phone.UserData;
using System.Windows.Controls.Primitives;
using Microsoft.Phone.Scheduler;
using System.IO;
using MeetMeHere.Support;

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

//getting phone contacts
//http://msdn.microsoft.com/en-us/library/windowsphone/develop/hh286416%28v=vs.105%29.aspx

//custom message box
//http://shawnoster.com/2012/10/welcome-custommessagebox-to-the-windows-phone-toolkit/
//custom popup
//http://developer.nokia.com/community/wiki/How_to_use_Pop-Ups_in_Windows_Phone

//getting windows theme color
//http://msdn.microsoft.com/en-us/library/windowsphone/develop/ff769545%28v=vs.105%29.aspx

//emailing an image
//http://kodierer.blogspot.ca/2010/12/sending-windows-phone-screenshots-in.html

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
        string downloadUrl = string.Empty;
        Popup contactsPopup = new Popup(); 
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
                ErrorText.Text = MeetMeHere.Support.MeetMeHereResources.ErrorLocationDisabled; 
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
            catch // (Exception ex)
            {
                //if ((uint)ex.HResult == 0x80004004)
                //{
                    LoadingBlock.Visibility = System.Windows.Visibility.Collapsed;
                    ErrorBlock.Visibility = System.Windows.Visibility.Visible;
                    ErrorText.Text = MeetMeHere.Support.MeetMeHereResources.ErrorLocationDisabledInPhoneSettings; 
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

        private void DrawMapMarkers(GeoCoordinate coordinates)
        {
            MapLayer mapLayer = new MapLayer();
         
            // Draw marker for current position
            if (coordinates != null)
            {
                //DrawAccuracyRadius(mapLayer);
                var currentAccentColorHex = (Color)Application.Current.Resources["PhoneAccentColor"];
                DrawMapMarker(coordinates, currentAccentColorHex, mapLayer);
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
                // Create a new button and set the text value to the localized string from MeetMeHere.Support.MeetMeHereResources.
                var relocateButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.refresh.png", UriKind.Relative));
                relocateButton.Text = MeetMeHere.Support.MeetMeHereResources.AppBarRefreshButtonText;
                relocateButton.Click += GetLocation_Click;
                ApplicationBar.Buttons.Add(relocateButton);

                var emailButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.email.png", UriKind.Relative));
                emailButton.Text = MeetMeHere.Support.MeetMeHereResources.AppBarEmailButtonText;
                emailButton.Click += SendEmail_Click;
                ApplicationBar.Buttons.Add(emailButton);

                var smsButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.phone.png", UriKind.Relative));
                smsButton.Text = MeetMeHere.Support.MeetMeHereResources.AppBarSmsButtonText;
                smsButton.Click += SendSms_Click; 
                ApplicationBar.Buttons.Add(smsButton);
            }

            // Create a new menu item with the localized string from MeetMeHere.Support.MeetMeHereResources.
            var aboutMenuItem = new ApplicationBarMenuItem(MeetMeHere.Support.MeetMeHereResources.AppBarAboutMenuItemText);
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
            geocoding.GetGeocodingInfo(coordinates.Latitude, coordinates.Longitude, FindSmsContacts); 
        }

        private void SendEmail_Click(object sender, EventArgs e)
        {
            IncrementSendCount();
            var geocoding = new GeocodingHelper();
            geocoding.GetGeocodingInfo(coordinates.Latitude, coordinates.Longitude, FindEmailContacts);
        }

        private void FindSmsContacts(GeocodingInfo info)
        {
            Contacts cons = new Contacts();
            cons.SearchCompleted += new EventHandler<ContactsSearchEventArgs>(SelectSmsContacts);
            cons.SearchAsync(String.Empty, FilterKind.None, info);
        }

        private void FindEmailContacts(GeocodingInfo info)
        {
            Contacts cons = new Contacts();
            cons.SearchCompleted += new EventHandler<ContactsSearchEventArgs>(SelectEmailContacts);
            cons.SearchAsync(String.Empty, FilterKind.None, info);
        }

        private void SelectSmsContacts(object sender, ContactsSearchEventArgs e)
        {
            GeocodingInfo info = (GeocodingInfo)e.State;
            ShowContactsPopup(e.Results.Where(c => c.PhoneNumbers.Any(p => p.Kind == PhoneNumberKind.Mobile)), info, SendSms);
        }

        private void SelectEmailContacts(object sender, ContactsSearchEventArgs e)
        {
            GeocodingInfo info = (GeocodingInfo)e.State;
            ShowContactsPopup(e.Results.Where(c => c.EmailAddresses.Any()), info, SendEmail); 
        }

        private void SendSms(IEnumerable<Contact> contacts, GeocodingInfo info)
        {
            contactsPopup.IsOpen = false;

            SmsComposeTask smsComposeTask = new SmsComposeTask();
            smsComposeTask.To = string.Join<Contact>(";", contacts);
            smsComposeTask.Body = string.Format(MeetMeHere.Support.MeetMeHereResources.SmsTemplate, info.AddressLabel, coordinates.Latitude, coordinates.Longitude);
            smsComposeTask.Show();
        }

        private void SendEmail(IEnumerable<Contact> contacts, GeocodingInfo info)
        {
            contactsPopup.IsOpen = false; 

            var mapCoordinates = HereMap.Center;
            var mapStyle = GetStyleNumber(HereMap.CartographicMode);
            var mapZoom = HereMap.ZoomLevel;

            var emailSubject = MeetMeHere.Support.MeetMeHereResources.EmailSubject; 
            var emailTo = string.Join<Contact>(";", contacts);
            var emailBody = string.Format(MeetMeHere.Support.MeetMeHereResources.EmailBody, 
                mapCoordinates.Latitude, 
                mapCoordinates.Longitude, 
                mapStyle, 
                mapZoom, 
                mapWidth,
                mapHeight, 
                HereMapsAppId, 
                HereMapsAppCode, 
                info.AddressLabel);

            var email = new EmailComposeTask { To = emailTo, Subject = emailSubject, Body = emailBody };
            email.Show();
            //SendEmailScreenshot(HereMap, emailTo, emailSubject); 
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
            MessageBoxResult result = MessageBox.Show(MeetMeHere.Support.MeetMeHereResources.LocationPrivacyPolicyBody, MeetMeHere.Support.MeetMeHereResources.LocationPrivacyPolicyTitle, MessageBoxButton.OKCancel);
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

        internal void ShowContactsPopup(IEnumerable<Contact> contacts, GeocodingInfo info, Action<IEnumerable<Contact>, GeocodingInfo> action)
        {
            if (contacts.Count() == 0)
            {
                action(new List<Contact>(), info);
                return; 
            }

            Border border = new Border();
            border.BorderBrush = new SolidColorBrush(Colors.White);
            border.BorderThickness = new Thickness(2);
            border.Margin = new Thickness(10, 10, 10, 10);

            StackPanel skt_pnl_outter = new StackPanel();
            skt_pnl_outter.Background = new SolidColorBrush(Colors.Black);
            skt_pnl_outter.Orientation = System.Windows.Controls.Orientation.Vertical;

            TextBlock txt_blk1 = new TextBlock();
            txt_blk1.Text = "Pick contacts to send to"; //TODO:add to resources
            txt_blk1.TextAlignment = TextAlignment.Center;
            txt_blk1.FontSize = 25;
            txt_blk1.Margin = new Thickness(10, 0, 10, 0);
            txt_blk1.Foreground = new SolidColorBrush(Colors.White);

            ListBox listbox = new ListBox();
            foreach (var contact in contacts) listbox.Items.Add(contact.DisplayName);
            listbox.FontSize = 20;
            //listbox.BorderBrush = new SolidColorBrush(Colors.White);
            //listbox.BorderThickness = new Thickness(1);
            listbox.SelectedIndex = 0;
            listbox.MaxHeight = 400; 
            listbox.Margin = new Thickness(20, 20, 20, 20);

            //Adding control to stack panel
            skt_pnl_outter.Children.Add(txt_blk1);
            skt_pnl_outter.Children.Add(listbox);

            StackPanel skt_pnl_inner = new StackPanel();
            skt_pnl_inner.Orientation = System.Windows.Controls.Orientation.Horizontal;

            Button btn_continue = new Button();
            btn_continue.Content = "send"; //TODO:add to resources
            btn_continue.Width = 215;
            btn_continue.Click += new RoutedEventHandler((s, e) => action(new[] { contacts.ElementAt(listbox.SelectedIndex) }, info));

            Button btn_cancel = new Button();
            btn_cancel.Content = "cancel"; //TODO:add to resources
            btn_cancel.Width = 215;
            btn_cancel.Click += new RoutedEventHandler((s,e) => contactsPopup.IsOpen = false);

            skt_pnl_inner.Children.Add(btn_continue);
            skt_pnl_inner.Children.Add(btn_cancel);


            skt_pnl_outter.Children.Add(skt_pnl_inner);

            // Adding stackpanel  to border
            border.Child = skt_pnl_outter;

            // Adding border to pup-up
            contactsPopup.Child = border;

            contactsPopup.VerticalOffset = 100; // 400;
            contactsPopup.HorizontalOffset = 10;

            contactsPopup.IsOpen = true;
        }

        //private static void SendEmailScreenshot(FrameworkElement element, string to, string subject)
        //{
        //    // Render the element at the maximum possible size
        //    ScaleTransform transform = null;
        //    if (element.ActualWidth * element.ActualHeight > 240 * 400)
        //    {
        //        // Calculate a uniform scale with the minimum possible size
        //        var scaleX = 240.0 / element.ActualWidth;
        //        var scaleY = 400.0 / element.ActualHeight;
        //        var scale = scaleX < scaleY ? scaleX : scaleY;
        //        transform = new ScaleTransform { ScaleX = scale, ScaleY = scale };
        //    }
        //    var wb = new WriteableBitmap(element, transform);

        //    using (var memoryStream = new MemoryStream())
        //    {
        //        // Encode the screenshot as JPEG with a quality of 60%
        //        wb.SaveJpeg(memoryStream, wb.PixelWidth, wb.PixelHeight, 0, 60);
        //        memoryStream.Seek(0, SeekOrigin.Begin);

        //        // Convert binary data to Base64 string
        //        var bytes = memoryStream.ToArray();
        //        var base64String = Convert.ToBase64String(bytes);

        //        // Invoke email task
        //        var emailComposeTask = new EmailComposeTask
        //        {
        //            Subject = subject,
        //            Body = base64String,
        //            To = "alex.john.martin@gmail.com", //to,
        //        };
        //        emailComposeTask.Show();
        //    }
        //}
    }
}