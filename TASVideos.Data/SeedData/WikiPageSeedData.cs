using TASVideos.Data.Entity;

// ReSharper disable StyleCop.SA1202
namespace TASVideos.Data.SeedData
{
	public class WikiPageSeedData
	{
		public const string InitialCreate = "Initial Create";
		public const string Import = "Modified by Import process";

		// These update exisint revisions to be relevant to the new system, or create new pages alltogether
		public static readonly WikiPage[] NewRevisions =
		{
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

These pages must not be renamed, because some code in the wiki system refers to them by name

[module:listsubpages]"
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
			}
		};
	}
}
