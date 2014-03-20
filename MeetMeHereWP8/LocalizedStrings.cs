namespace MeetMeHereWP8
{
    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    public class LocalizedStrings
    {
        private static MeetMeHere.Support.Resources.AppResources _localizedResources = new MeetMeHere.Support.Resources.AppResources();

        public MeetMeHere.Support.Resources.AppResources LocalizedResources { get { return _localizedResources; } }
    }
}