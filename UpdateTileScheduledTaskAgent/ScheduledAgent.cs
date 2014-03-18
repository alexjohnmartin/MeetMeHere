using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using System.IO.IsolatedStorage;
using System;
using System.Linq;
using Microsoft.Phone.Shell;

namespace UpdateTileScheduledTaskAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            var appSettings = IsolatedStorageSettings.ApplicationSettings;
            if (appSettings["sendCountDate"] == null ||
                (string)appSettings["sendCountDate"] != DateTime.Now.Date.ToShortDateString())
            {
                appSettings["sendCountDate"] = DateTime.Now.Date.ToShortDateString();
                appSettings["sendCount"] = 0;

                SetTileData(0);
            }

            NotifyComplete();
        }

        private static void SetTileData(int sendCount)
        {
            var tileId = ShellTile.ActiveTiles.FirstOrDefault();
            if (tileId != null)
            {
                //TODO:use AppResources for translatable text
                var tileData = new FlipTileData
                {
                    Title = "Meet Me Here",
                    BackContent = string.Format("Sent {0} today", sendCount),
                    BackgroundImage = new Uri(@"Assets\Tiles\FlipCycleTileMedium.png", UriKind.Relative),
                    WideBackContent = string.Format("Sent {0} today", sendCount),
                    WideBackgroundImage = new Uri(@"Assets\Tiles\FlipCycleTileLarge.png", UriKind.Relative),
                    BackBackgroundImage = new Uri(@"isostore:/Shared/ShellContent/mapview.jpg"),
                    WideBackBackgroundImage = new Uri(@"isostore:/Shared/ShellContent/mapview-wide.jpg"),
                };

                tileId.Update(tileData);
            }
        }
    }
}