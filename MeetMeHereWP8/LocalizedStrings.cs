namespace MeetMeHereWP8
{
    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    public class LocalizedStrings
    {
        private static MeetMeHere.Support.MeetMeHereResources _localizedResources = new MeetMeHere.Support.MeetMeHereResources();

        public MeetMeHere.Support.MeetMeHereResources LocalizedResources { get { return _localizedResources; } }
    }
}