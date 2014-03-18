using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.IO.IsolatedStorage;
using System.Linq; 

namespace MeetMeHereWP8
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        protected override void OnInvoke(ScheduledTask task)
        {
            var appSettings = IsolatedStorageSettings.ApplicationSettings;
            if (appSettings["sendCountDate"] == null ||
                (string)appSettings["sendCountDate"] != DateTime.Now.Date.ToShortDateString())
            {
                appSettings["sendCountDate"] = DateTime.Now.Date.ToShortDateString();
                appSettings["sendCount"] = 0;

                TileHelper.SetTileData(0);
            }

#if DEBUG
  ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(60));
#endif

            // Call NotifyComplete to let the system know the agent is done working.
            NotifyComplete();
        }
    }
}
