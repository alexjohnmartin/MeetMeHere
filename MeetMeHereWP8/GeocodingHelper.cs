using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MeetMeHereWP8
{
    internal class GeocodingHelper
    {
        const string baseUrl = "http://reverse.geocoder.cit.api.here.com/6.2/reversegeocode.xml?prox={0},{1}&mode=retrieveAddresses&app_id=PC3CUQZkDFZ46i8ifPIL&app_code=u_JokeYoH5JkfpvqL2CuFA&gen=4";
        Action<GeocodingInfo> _action; 

        public void GetGeocodingInfo(double latitude, double longitude, Action<GeocodingInfo> action)
        {
            _action = action; 

            var downloadUrl = string.Format(baseUrl, latitude, longitude);
            WebClient client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(LoadXmlCallback);
            client.DownloadStringAsync(new Uri("http://www.myurl.com/myFile.txt"));
        }

        private void LoadXmlCallback(object sender, DownloadStringCompletedEventArgs e)
        {
            var textData = (string)e.Result;

            _action.Invoke(new GeocodingInfo()); 
        }
    }
}
