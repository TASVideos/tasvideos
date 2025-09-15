namespace TASVideos;

public static class LinkConstants
{
	public const string SubmissionWikiPage = "InternalSystem/SubmissionContent/S";
	public const string PublicationWikiPage = "InternalSystem/PublicationContent/M";
	public const string GameWikiPage = "InternalSystem/GameContent/G";
	public const string HomePages = "HomePages/";
}

public static class Durations
{
	public static readonly TimeSpan OneMinute = TimeSpan.FromMinutes(1);
	public static readonly TimeSpan FiveMinutes = TimeSpan.FromMinutes(5);
	public static readonly TimeSpan OneDay = TimeSpan.FromDays(1);
	public static readonly TimeSpan OneWeek = TimeSpan.FromDays(7);
	public static readonly TimeSpan OneYear = TimeSpan.FromDays(365);
}

public static class CacheKeys
{
	public const string CurrentWikiCache = "WikiCache";
	public const string AwardsCache = "AwardsCache";
	public const string MovieTokens = "MovieTokenData";
	public const string UsersWithCustomLocale = "UsersWithCustomLocale";
	public const string CustomUserLocalePrefix = "CustomUserLocale-";
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
	public const int PlaygroundForumId = 74;
	public const int WelcomeToTasvideosPostId = 515883;
	public const int PublishedAuthorRoleAddedPostId = 516348;
	public const int AutoAssignedRolePostId = 516349;
	public const int SpamTopicId = 13101;
	public const int SpamForumId = 28;

	public const string NewPublicationPostSubject = "Movie published";
	public const string NewPublicationPost =
		"""
		[b]This movie has been published.[/b]
		The posts before this message apply to the submission, and posts after this message apply to the published movie.

		----
		[movie]{PublicationId}[/movie]
		""";

	public const string UnpublishSubject = "Publication Reset To Workbench";
	public const string UnpublishPost =
		"""
		[b]This movie has been unpublished and reset to a pending submission.[/b]
		The posts after this message will continue to apply to the submission.

		""";

	public const string NewSubmissionPost = "This topic is for the purpose of discussing ";
	public const string PollQuestion = "Vote: Did you like watching this movie? (Vote after watching!)";
	public const string PollOptionYes = "Yes";
	public const string PollOptionNo = "No";
	public const string PollOptionsMeh = "Meh";

	public const string DefaultPublicationText = "''[TODO]: describe this movie here''";

	public const int MaximumMovieSize = 2 * 1024 * 1024;
	public const string MaximumMovieSizeHumanReadable = "2 MiB";
	public const int UserFileStorageLimit = 1000 * 1000 * 50; // 50 MB

	public const int GamesForumCategory = 5;
	public const int OtherGamesForum = 5;

	public const int MetaDescriptionLength = 350;

	public const int YearsOfBanDisplayedAsIndefinite = 2;

	public const string MainIvatarDomain = "seccdn.libravatar.org"; // keep in sync with profile-settings.js
}

public static class ForumConstants
{
	public const int PostsPerPage = 25;
	public const int WorkBenchForumId = 7;
	public const int NewsTopicId = 8694;
	public const int DaysPostsCountAsActive = 14;
}

public static class CustomClaimTypes
{
	public const string Permission = "P";
}
