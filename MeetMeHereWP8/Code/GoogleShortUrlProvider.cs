using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MeetMeHere.Code
{
    public class GoogleShortUrlProvider
    {
        const string googleShortUrlApiAddress = "https://www.googleapis.com/urlshortener/v1/url/?key=AIzaSyCKbA_uTQNar11WU6DLpELROy-74ddVAKs";
        const string googleShortUrlBody = "{\"longUrl\": \"{0}\"}";

        private Action<string> _action; 

        public void GenerateShortUrl(string longUrl, Action<string> action)
        {
            _action = action;

            Uri uri = new Uri(googleShortUrlApiAddress);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.ContentType = "application/json";

            request.BeginGetRequestStream(delegate(IAsyncResult req)
            {
                var outStream = request.EndGetRequestStream(req);
                var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(new { longUrl = longUrl }); 
                using (StreamWriter w = new StreamWriter(outStream))
                    w.Write(jsonData);

                request.BeginGetResponse(delegate(IAsyncResult result)
                {
                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result);

                        using (var stream = response.GetResponseStream())
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                onResponseGot(reader.ReadToEnd());
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        var error = ex.Message; 
                        onResponseGot(null);
                    }

                }, null);

            }, null);
        }

        private void onResponseGot(string result)
        {
            if (string.IsNullOrEmpty(result)) return; 

            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<ShortUrlResponse>(result);
            _action.Invoke(response.id); 
        }

        private class ShortUrlResponse
        {
            public string id { get; set; }
            public string kind { get; set; }
            public string longUrl { get; set; }
        }
    }
}
