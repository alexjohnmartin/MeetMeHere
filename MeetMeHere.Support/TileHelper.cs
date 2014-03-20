using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetMeHere.Support
{
    public class TileHelper
    {
        public static void SetTileData(int sendCount)
        {
            var tileId = ShellTile.ActiveTiles.FirstOrDefault();
            if (tileId != null)
            {
                var tileData = new FlipTileData
                {
                    Title = MeetMeHere.Support.Resources.AppResources.ApplicationName,
                    BackContent = string.Format(MeetMeHere.Support.Resources.AppResources.TileSentCount, sendCount),
                    BackgroundImage = new Uri(@"Assets\Tiles\FlipCycleTileMedium.png", UriKind.Relative),
                    WideBackContent = string.Format(MeetMeHere.Support.Resources.AppResources.TileSentCount, sendCount),
                    WideBackgroundImage = new Uri(@"Assets\Tiles\FlipCycleTileLarge.png", UriKind.Relative),
                    BackBackgroundImage = new Uri(@"isostore:/Shared/ShellContent/mapview.jpg"),
                    WideBackBackgroundImage = new Uri(@"isostore:/Shared/ShellContent/mapview-wide.jpg"),
                };

                tileId.Update(tileData);
            }
        }
    }
}
