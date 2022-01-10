using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TASVideos.WikiEngine
{
	public static class WikiModules
	{
		public const string Addresses = "addresses";
		public const string Awards = "awards";
		public const string BrokenLinks = "brokenlinks";
		public const string ActiveTab = "activetab";
		public const string AviEncodes = "aviencodes";
		public const string CurrentEditLink = "wikigetcurrenteditlink";
		public const string DisplayGameName = "displaygamename";
		public const string DisplayMiniMovie = "displayminimovie";
		public const string DisplayMovies = "displaymovie";
		public const string EditorActivity = "editoractivity";
		public const string FeedLog = "feedlog";
		public const string FirstEditionTas = "firsteditiontas";
		public const string Frames = "frames";
		public const string FrontpageSubmissionList = "frontpagesubmissionlist";
		public const string GameName = "gamename";
		public const string GameSubPages = "gamesubpages";
		public const string ListLanguages = "listlanguages";
		public const string MediaPosts = "mediaposts";
		public const string MovieChangeLog = "moviechangelog";
		public const string MoviesByAuthor = "moviesbyplayer";
		public const string MoviesByCategory = "moviesearch_category";
		public const string MoviesGameList = "moviesgamelist";
		public const string MoviesList = "movieslist";
		public const string MovieStatistics = "moviestatistics";
		public const string MovieMaintenanceLog = "moviemaintlog";
		public const string NoEmulator = "noemulator";
		public const string NoGameName = "nogame";
		public const string NoRom = "norom";
		public const string PlatformAuthorList = "platformtaserlists";
		public const string PlatformFramerates = "platformframerates";
		public const string PlayerPointsTable = "playerpointstable";
		public const string RejectedSubmissions = "rejectedsubmissions";
		public const string SupportedMovieTypes = "submittableformats";
		public const string TabularMovieList = "tabularmovielist";
		public const string TimeSinceDate = "timesincedate";
		public const string TopicFeed = "topicfeed";
		public const string UnmirroredMovies = "unmirroredmovies";
		public const string UserGetWikiName = "usergetwikiname";
		public const string UserMaintenanceLogs = "usermaintlog";
		public const string UserMovies = "usermovies";
		public const string UserName = "user_name";
		public const string Welcome = "welcome";
		public const string WikiLink = "__wikilink";
		public const string WikiOrphans = "wikiorphans";
		public const string WikiTextChangeLog = "wikitextchangelog";
		public const string WikiUsers = "wikiusers";
		public const string Youtube = "youtube";

		private static readonly HashSet<string> Modules = typeof(WikiModules)
			.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
			.Where(fi => fi.IsLiteral && !fi.IsInitOnly)
			.Select(fi => fi.GetRawConstantValue()?.ToString() ?? "")
			.ToHashSet(StringComparer.InvariantCultureIgnoreCase);

		public static bool IsModule(string name)
		{
			return Modules.Contains(name);
		}
	}
}
