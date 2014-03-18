using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Resources;

namespace MeetMeHereWP8
{
    internal class DownloadAndSaveImage
    {
        const int tileHeight = 336;
        const int wideTileWidth = 691;
        const int normalTileWidth = 336;

        PeriodicTask periodicTask;
        string periodicTaskName = "ScheduledAgent";
        public bool agentsAreEnabled = true;

        private const string baseDownloadUrl = "http://image.maps.cit.api.here.com/mia/1.6/mapview?app_id={4}&app_code={5}&c={0},{1}&z={2}&w={6}&h={7}&t={3}";
        IsolatedStorageFile MyStore = IsolatedStorageFile.GetUserStoreForApplication();
        private bool loadedNormalImage = false;
        private bool loadedWideImage = false;
        private int iterationCount = 0;
        private const int MaxIterations = 100; 

        internal void DownloadMapImages(double latitude, double longitude, double zoom, int cartographicMode, string hereMapsAppId, string hereMapsAppCode)
        {
            var downloadUrl = string.Format(baseDownloadUrl, latitude, longitude, zoom, cartographicMode, hereMapsAppId, hereMapsAppCode, wideTileWidth, tileHeight);
            WebClient client = new WebClient();
            client.OpenReadCompleted += new OpenReadCompletedEventHandler(client_OpenReadCompleted_wide);
            client.OpenReadAsync(new Uri(downloadUrl), client);

            var downloadNormalUrl = string.Format(baseDownloadUrl, latitude, longitude, zoom, cartographicMode, hereMapsAppId, hereMapsAppCode, normalTileWidth, tileHeight);
            WebClient client2 = new WebClient();
            client2.OpenReadCompleted += new OpenReadCompletedEventHandler(client_OpenReadCompleted_normal);
            client2.OpenReadAsync(new Uri(downloadNormalUrl), client2);

            var tileUpdateThread = new Thread(SetTileData);
            tileUpdateThread.Start(); 
        }

        void client_OpenReadCompleted_wide(object sender, OpenReadCompletedEventArgs e)
        {
            var resInfo = new StreamResourceInfo(e.Result, null);
            var reader = new StreamReader(resInfo.Stream);
            byte[] contents;
            using (BinaryReader bReader = new BinaryReader(reader.BaseStream))
            {
                contents = bReader.ReadBytes((int)reader.BaseStream.Length);
            }
            IsolatedStorageFileStream stream = MyStore.CreateFile(@"Shared\ShellContent\mapview-wide.jpg");
            stream.Write(contents, 0, contents.Length);
            stream.Close();
            loadedWideImage = true; 
        }

        void client_OpenReadCompleted_normal(object sender, OpenReadCompletedEventArgs e)
        {
            var resInfo = new StreamResourceInfo(e.Result, null);
            var reader = new StreamReader(resInfo.Stream);
            byte[] contents;
            using (BinaryReader bReader = new BinaryReader(reader.BaseStream))
            {
                contents = bReader.ReadBytes((int)reader.BaseStream.Length);
            }
            IsolatedStorageFileStream stream = MyStore.CreateFile(@"Shared\ShellContent\mapview.jpg");
            stream.Write(contents, 0, contents.Length);
            stream.Close();
            loadedNormalImage = true; 
        }

        private void SetTileData()
        {
            while (iterationCount < MaxIterations)
            {
                if (loadedNormalImage && loadedWideImage)
                {
                    var appSettings = IsolatedStorageSettings.ApplicationSettings; 
                    TileHelper.SetTileData((int)appSettings["sendCount"]);
                    StartPeriodicAgent();
                    return;
                }

                Thread.Sleep(1000); 
                iterationCount++;
            }
        }


        private void StartPeriodicAgent()
        {
            // Variable for tracking enabled status of background agents for this app.
            agentsAreEnabled = true;

            // Obtain a reference to the period task, if one exists
            periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;

            // If the task already exists and background agents are enabled for the
            // application, you must remove the task and then add it again to update 
            // the schedule
            if (periodicTask != null)
            {
                RemoveAgent(periodicTaskName);
            }

            periodicTask = new PeriodicTask(periodicTaskName);

            // The description is required for periodic agents. This is the string that the user
            // will see in the background services Settings page on the device.
            periodicTask.Description = "This demonstrates a periodic task.";

            // Place the call to Add in a try block in case the user has disabled agents.
            try
            {
                ScheduledActionService.Add(periodicTask);

                // If debugging is enabled, use LaunchForTest to launch the agent in one minute.
#if(DEBUG)
                ScheduledActionService.LaunchForTest(periodicTaskName, TimeSpan.FromSeconds(60));
#endif
            }
            catch (InvalidOperationException exception)
            {

            }
            catch (SchedulerServiceException)
            {

            }
        }

        private void RemoveAgent(string name)
        {
            try
            {
                ScheduledActionService.Remove(name);
            }
            catch (Exception)
            {
            }
        }
    }
}