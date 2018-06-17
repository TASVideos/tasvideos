namespace TASVideos.Data.Constants
{
	public static class LinkConstants
	{
		public const string SubmissionWikiPage = "InternalSystem/SubmissionContent/S";
		public const string PublicationWikiPage = "InternalSystem/PublicationContent/M";
		public const string GameWikiPage = "InternalSystem/GameContent/G";
	}

	public static class DurationConstants
	{
		public const int OneMinuteInSeconds = 60;
		public const int OneDayInSeconds = 60 * 60 * 24;
		public const int OneWeekInSeconds = 60 * 60 * 24 * 7;
	}

	// These perform site functions, maybe they should be in the database?
	public static class SiteGlobalConstants
	{
		public const int VestedPostCount = 3; // Minimum number of posts to become an experienced forum user
		public const int MinimumHoursBeforeJudgement = 72; // Minimum number of hours before a judge can set a submission to accepted/rejected
	}
}
