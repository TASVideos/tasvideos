using TASVideos.Data.Entity;

// ReSharper disable StyleCop.SA1202
namespace TASVideos.Data.SeedData
{
	public class WikiPageSeedData
	{
		public const string InitialCreate = "Initial Create";
		public const string Import = "Modified by Import process";

		// These update existing revisions to be relevant to the new system, or create new pages all-together
		public static readonly WikiPage[] NewRevisions =
		{
			new WikiPage
			{
				PageName = "SystemPages",
				RevisionMessage = Import,
				Markup = @"This page is obsolete. Use [System] instead.

[TODO]: Update referrer pages to link to System"
			},
			new WikiPage
			{
				PageName = "System/FilesEditingHelp",
				RevisionMessage = Import,
				Markup = @"! Screenshot guidelines

It is discouraged to reuse screenshots for more than one publication.  In some cases, this may work - mostly for very minor improvements with little overall change in the strategy of the movie.  But overall, it is ideal for each publication to have its own screenshot.

[PublisherGuidelines|Choose good screenshots].%%%
[Screenshots|Remember to optimize your PNG files].%%%
[=Forum/Topics/6052|Screenshot discussion].%%%"
			},
			new WikiPage
			{
				PageName = "SiteCodingStandards",
				RevisionMessage = Import,
				Markup = "[TODO]: Document coding standards"
			},
			new WikiPage
			{
				PageName = "SandBox",
				RevisionMessage = Import,
				Markup = "A page for tryout out wiki markup."
			},
			new WikiPage
			{
				PageName = "JudgeCounts",
				RevisionMessage = Import,
				Markup = "This page is obsolete.  See [Activity] instead."
			},
			new WikiPage
			{
				PageName = "PubCounts",
				RevisionMessage = Import,
				Markup = "This page is obsolete.  See [Activity] instead."
			},
			new WikiPage
			{
				PageName = "System/ActivitySummary",
				RevisionMessage = Import,
				Markup = @"This page tracks the number of publications as well as the most recent publication for all users that have published a movie."
			},
			new WikiPage
			{
				PageName = "System/AvatarRequirements",
				RevisionMessage = Import,
				Markup = @"Displays a small graphic image below your details in posts.
__The size limits for this file are 100x100 pixels and 9 kB.
Images bigger than that are deleted without warning.__
Do not use images hosted at Angelfire, or Imageshack, because those images may suddenly turn into huge ""site bandwidth exhausted"" error."
			},
			new WikiPage
			{
				PageName = "System/SupportedMovieTypes",
				RevisionMessage = Import,
				Markup = @"The following file types are supported for site [Subs-List|submission]

[module:supportedmovietypes]"
			},
			new WikiPage
			{
				PageName = "MediaPosts",
				RevisionMessage = Import,
				Markup = @"Below is a list of recent media post activity (Announcements etc that have went to places like IRC, Discord, etc)

[module:mediaposts|limit=50]"
			},
			new WikiPage
			{
				PageName = "System/PlayersHeader",
				RevisionMessage = Import,
				Markup = "Here we remember the names of all the authors that have ever contributed a published movie to this site.%%%"
			}, 
			new WikiPage
			{
				PageName = "System/SubmissionHeader",
				RevisionMessage = Import,
				Markup =
@"This page lists movies that have been submitted for review.

Some of them will eventually end up on the [movies] page, while some of them will [RejectedSubmissions|not make it].

Submitted movies can be published by [Users|certain people].
"
			}, 
			new WikiPage
			{
				PageName = "HomePages",
				RevisionMessage = Import,
				Markup =
@"[TODO]: make this page nicer
This is a list of homepages for various users.

All users, by default, have the ability to create and edit their homepage.  Log in, and navigate to HomePages/[[your username]] and click the edit button.
"
			},
			new WikiPage
			{
				PageName = "System/WikiEditHelp",
				RevisionMessage = Import,
				Markup =
@"Refer to the [TextFormattingRules] for markup and formatting help.%%%
Make sure all edits conform to the [EditorGuidelines]"
			},
			new WikiPage
			{
				PageName = "System",
				RevisionMessage = InitialCreate,
				Markup =
@"This page lists all the system pages. System pages are wiki pages that are embedded into non-editable pages or automatically appended to certain types of wiki pages.

These pages must not be renamed, because some code in the wiki system refers to them by name"
			},
			new WikiPage
			{
				PageName = "System/SubmitMovieHeader",
				RevisionMessage = InitialCreate,
				Markup =
@"__Please read the [Submission Instructions] before submitting a movie.__

__In particular, please ensure that your movie is complete (not a work in progress), and that it beats all known records if going for speed. Works in progress may be posted in the [=forum/t/10152|WIP thread] or other places. If you are simply looking for a place to store your movie files for distribution to other members, please consider [=userfiles|user files].__

Ignoring these instructions may result in your submission being rejected.

After clicking on ""Save"", please wait for the submission to appear. If you cancel this, the discussion topic will not be created."
			},
			new WikiPage
			{
				PageName = "System/SubmissionWarning",
				RevisionMessage = Import,
				Markup =
@"%%%
Before pressing the Save button, please verify that you have read the [Movie Rules] and [Guidelines]. Your movie must be complete and must beat all existing records where applicable. Submissions that do not follow these rules will be rejected.

This submission form is only for movies that are intended to become a publication on this site. Do ''not'' submit if this is not your goal. If you simply want to upload a movie file, use [=userfiles|userfiles] instead."
			},
			new WikiPage
			{
				PageName = "System/GameResourcesHeader",
				RevisionMessage = Import,
				Markup =
@"This page documents information about [module:GameName]. Many of the tricks demonstrated here are near impossible in real time and documented for the purposes of creating [WelcomeToTASVideos|Tool-assisted Speedruns]."
			},
			new WikiPage
			{
				PageName = "System/GameResourcesFooter",
				RevisionMessage = Import,
				Markup = 
@"!!!See also
* [Game Resources] - we have resource pages for other games too!
* [GameResources/Common Tricks|Common Tricks] - tricks common to many games
* [GameResources/BossFightingGuide|Boss Fighting Guide] - tricks specific to boss fights"
			},
			new WikiPage
			{
				PageName = "GameResources/A7800",
				RevisionMessage = Import,
				Markup = @"Here is listing of game-specific resource pages for Atari 7800"
			},
			new WikiPage
			{
				PageName = "GameResources/DOS",
				RevisionMessage = Import,
				Markup = @"Here is listing of game-specific resource pages for DOS (PC)"
			},
			new WikiPage
			{
				PageName = "GameResources/DS",
				RevisionMessage = Import,
				Markup = @"Here is listing of game-specific resource pages for Nintendo DS"
			},
			new WikiPage
			{
				PageName = "GameResources/GBx",
				RevisionMessage = Import,
				Markup = @"Here is listing of game-specific resource pages for Game Boy, Super Game Boy, Game Boy Color and Game Boy Advance"
			},
			new WikiPage
			{
				PageName = "GameResources/GC",
				RevisionMessage = Import,
				Markup = @"Here is listing of game-specific resource pages for the Nintendo GameCube"
			},
			new WikiPage
			{
				PageName = "GameResources/Genesis",
				RevisionMessage = Import,
				Markup = @"Here is listing of game-specific resource pages for Sega Genesis"
			},
			new WikiPage
			{
				PageName = "GameResources/N64",
				RevisionMessage = Import,
				Markup = @"Here is listing of game-specific resource pages for Nintendo 64"
			},
			new WikiPage
			{
				PageName = "GameResources/NES",
				RevisionMessage = Import,
				Markup = @"Here is listing of game-specific resource pages for NES"
			},
			new WikiPage
			{
				PageName = "GameResources/PSX",
				RevisionMessage = Import,
				Markup = @"Here is listing of game-specific resource pages for Sony Playstation"
			},
			new WikiPage
			{
				PageName = "GameResources/SMS",
				RevisionMessage = Import,
				Markup = @"Here is listing of game-specific resource pages for Sega Master System"
			},
			new WikiPage
			{
				PageName = "GameResources/SNES",
				RevisionMessage = Import,
				Markup = @"Here is listing of game-specific resource pages for Super Nintendo Entertainment System"
			},
			new WikiPage
			{
				PageName = "GameResources/Wii",
				RevisionMessage = Import,
				Markup = @"Here is listing of game-specific resource pages for the Nintendo Wii"
			},
			new WikiPage
			{
				PageName = "GameResources/Windows",
				RevisionMessage = Import,
				Markup = @"Here is listing of game-specific resource pages for Windows games"
			},
			new WikiPage
			{
				PageName = "System/IsSystemPage",
				RevisionMessage = Import,
				Markup = @"!! This page is a system resource not a page intended for standalone viewing.
See the [System] page for details."
			},
			new WikiPage
			{
				PageName = "System/SystemFooter",
				RevisionMessage = Import,
				Markup = @"Editing System Pages requires permissions beyond a regular editor. Editors, make sure pages that utilize this page are tested when making edits"
			},
		};
	}
}
