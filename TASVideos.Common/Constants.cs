namespace TASVideos
{
	public static class LinkConstants
	{
		public const string SubmissionWikiPage = "InternalSystem/SubmissionContent/S";
		public const string PublicationWikiPage = "InternalSystem/PublicationContent/M";
		public const string GameWikiPage = "InternalSystem/GameContent/G";
	}

	public static class Durations
	{
		public const int ThirtySecondsInSeconds = 30;
		public const int OneMinuteInSeconds = 60;
		public const int FiveMinutesInSeconds = 60 * 5;
		public const int OneDayInSeconds = 60 * 60 * 24;
		public const int OneWeekInSeconds = 60 * 60 * 24 * 7;
		public const int OneYearInSeconds = 60 * 60 * 24 * 7 * 52;
	}

	public static class CacheKeys
	{
		public const string CurrentWikiCache = "WikiCache";
		public const string AwardsCache = "AwardsCache";
		public const string UnreadMessageCount = "UnreadMessageCountCache-";
		public const string MovieTokens = "MovieTokenData";
		public const string MovieRatingKey = "OverallRatingForMovieViewModel-";
	}

	// These perform site functions, maybe they should be in the database?
	public static class SiteGlobalConstants
	{
		public const string TASVideoAgent = "TASVideoAgent";
		public const int TASVideoAgentId = 505;

		public const string TASVideosGrue = "TASVideosGrue";
		public const int TASVideosGrueId = 3788;
		public const int GrueFoodForumId = 24;
		public const int WorkbenchForumId = 7;
		public const int PublishedMoviesForumId = 16;

		public const string NewPublicationPostSubject = "Movie published";
		public const string NewPublicationPost = @"[b]This movie has been published.[/b]
The posts before this message apply to the submission, and posts after this message apply to the published movie.

----
[movie]{PublicationId}[/movie]";

		public const string NewSubmissionPost = @"This topic is for the purpose of discussing ";
		public const string PollQuestion = @"Vote: Did you like watching this movie? (Vote after watching!)";
		public const string PollOptionYes = "Yes";
		public const string PollOptionNo = "No";
		public const string PollOptionsMeh = "Meh";

		public const string DefaultPublicationText = "''[TODO]: describe this movie here''";

		public const int MaximumMovieSize = 500 * 1024;
		public const int UserFileStorageLimit = 1000 * 1000 * 50; // 50 MB
	}

	public static class PlayerPointConstants
	{
		public const double ObsoleteMultiplier = 0.000001;
		public const int MinimumPlayerPointsForPublication = 5;
	}

	public static class PlayerRanks
	{
		public const string FormerPlayer = "Former player";
		public const string Player = "Player";
		public const string ActivePlayer = "Active player";
		public const string ExperiencedPlayer = "Experienced player";
		public const string SkilledPlayer = "Skilled player";
		public const string ExpertPlayer = "Expert player";
	}

	public static class ForumConstants
	{
		public const int PostsPerPage = 25;
		public const int TopicsPerForum = 50;

		public const int WorkBenchForumId = 7;
	}

	public static class PostGroups
	{
		public const string Forum = "Forum";
		public const string Wiki = "Wiki";
		public const string Submission = "Submission";
		public const string UserManagement = "UserManagement";
		public const string UserFiles = "UserFiles";
		public const string Publication = "Publication";
	}

	// TODO: this is bootstrap specific, maybe it should go in the MVC project
	// TODO: a better name
	public static class Styles
	{
		public const string Info = "info";
		public const string Success = "success";
		public const string Warning = "warning";
		public const string Danger = "danger";
	}

	public static class CustomClaimTypes
	{
		public const string Permission = "P";
	}

	public static class AvatarRequirements
	{
		public const int Width = 100;
		public const int Height = 100;
	}
}
