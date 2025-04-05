using System.Reflection;

namespace TASVideos;

public static class UiDefaults
{
	public const string DefaultDropdownText = "---";

	public static readonly SelectListItem[] DefaultEntry =
	[
		new() { Text = DefaultDropdownText, Value = "" }
	];

	public static readonly SelectListItem[] AnyEntry = [new() { Text = "Any", Value = "" }];
}

public static class SystemWiki
{
	public const string ActivitySummary = "System/ActivitySummary";
	public const string AvatarRequirements = "System/AvatarRequirements";
	public const string BannedUserNotice = "System/BannedUserNotice";
	public const string ClassEditingHelp = "System/ClassEditingHelp";
	public const string EditMovieHeader = "System/EditMovieHeader";
	public const string EmailConfirmationSentMessage = "System/EmailConfirmationSentMessage";
	public const string Error = "System/Error";
	public const string FilesEditingHelp = "System/FilesEditingHelp";
	public const string ForumHeader = "System/ForumHeader";
	public const string FrontPage = "System/FrontPage";
	public const string GameResourcesFooter = "System/GameResourcesFooter";
	public const string GameResourcesHeader = "System/GameResourcesHeader";
	public const string HomePageCannotBeEdited = "System/HomePageCannotBeEdited";
	public const string HomePageDoesNotExist = "System/HomePageDoesNotExist";
	public const string IsSystemPage = "System/IsSystemPage";
	public const string MoodAvatarRequirements = "System/MoodAvatarRequirements";
	public const string MovieLinkInstruction = "System/MovieLinkInstruction";
	public const string MovieRatingGuidelines = "System/MovieRatingGuidelines";
	public const string NameChanges = "System/NameChanges";
	public const string PlayersHeader = "System/PlayersHeader";
	public const string RejectionReasonsHeader = "System/RejectionReasonsHeader";
	public const string SearchTerms = "System/SearchTerms";
	public const string SubmissionHeader = "System/SubmissionHeader";
	public const string SubmissionImportant = "System/SubmissionImportant";
	public const string SubmissionZipFailure = "System/SubmissionZipFailure";
	public const string SubmitMovieHeader = "System/SubmitMovieHeader";
	public const string SupportedMovieTypes = "System/SupportedMovieTypes";
	public const string SystemFooter = "System/SystemFooter";
	public const string UserEditRole = "System/UserEditRole";
	public const string UserFileUploadHeader = "System/UserFileUploadHeader";
	public const string WelcomeText = "System/WelcomeText";
	public const string WikiEditHelp = "System/WikiEditHelp";
	public const string WikiEditNote = "System/WikiEditNote";

	public static readonly HashSet<string> Pages = typeof(SystemWiki)
		.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
		.Where(fi => fi.IsLiteral && !fi.IsInitOnly)
		.Select(fi => fi.GetRawConstantValue()?.ToString() ?? "")
		.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
}
