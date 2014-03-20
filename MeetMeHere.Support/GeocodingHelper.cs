using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MeetMeHere.Support
{
    public class GeocodingHelper
    {
        const string baseUrl = "http://reverse.geocoder.cit.api.here.com/6.2/reversegeocode.xml?prox={0},{1}&mode=retrieveAddresses&app_id=PC3CUQZkDFZ46i8ifPIL&app_code=u_JokeYoH5JkfpvqL2CuFA&gen=4";
        Action<GeocodingInfo> _action; 

        public void GetGeocodingInfo(double latitude, double longitude, Action<GeocodingInfo> action)
        {
            _action = action; 

            var downloadUrl = string.Format(baseUrl, latitude, longitude);
            WebClient client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(LoadXmlCallback);
            client.DownloadStringAsync(new Uri(downloadUrl));
        }

        private void LoadXmlCallback(object sender, DownloadStringCompletedEventArgs e)
        {
            string addressLabel = string.Empty;
            try
            {
                var textData = (string)e.Result;

                if (textData.Contains("<Address>"))
                {
                    var startIndex = textData.IndexOf("<Address>") + 9;
                    var endIndex = textData.IndexOf("</Address>");
                    var addressText = textData.Substring(
                            startIndex,
                            endIndex - startIndex
                        ).Trim();

                    startIndex = addressText.IndexOf("<Label>") + 7;
                    endIndex = addressText.IndexOf("</Label>");
                    addressLabel = addressText.Substring(
                            startIndex,
                            endIndex - startIndex
                        ).Trim();
                }
            }
            catch(Exception ex)
            {
                addressLabel = "????";
            }
             
            _action.Invoke(new GeocodingInfo { AddressLabel = addressLabel });            
        }
    }
}
