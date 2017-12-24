using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TASVideos.Data.Entity;

namespace TASVideos.Models
{
	/// <summary>
	/// Represents a <see cref="TASVideos.Data.Entity.WikiPage"/> for the purpose of creating a new revision
	/// </summary>
	public class WikiEditModel
	{
		[Required]
		[ValidWikiPageName]
		public string PageName { get; set; }

		[Required]
		public string Markup { get; set; }

		[Display(Name = "Minor Edit")]
		public bool MinorEdit { get; set; }

		[Required] // Yeah, I did that
		[Display(Name = "Edit Comments")]
		public string RevisionMessage { get; set; }

		public IEnumerable<WikiReferralModel> Referrals { get; set; } = new List<WikiReferralModel>();
	}

	/// <summary>
	/// Represents the revision history for a wiki page 
	/// </summary>
	public class WikiHistoryModel
	{
		public string PageName { get; set; }

		public IEnumerable<WikiRevisionModel> Revisions { get; set; } = new List<WikiRevisionModel>();

		public class WikiRevisionModel
		{
			public int Revision { get; set; }

			[Display(Name = "Date")]
			public DateTime CreateTimeStamp { get; set; }

			[Display(Name = "Author")]
			public string CreateUserName { get; set; }

			[Display(Name = "Minor Edit")]
			public bool MinorEdit { get; set; }

			[Display(Name = "Revision Message")]
			public string RevisionMessage { get; set; }
		}
	}

	/// <summary>
	/// Represents the data necessary to rename a wiki page
	/// </summary>
	public class WikiMoveModel
	{
		public string OriginalPageName { get; set; }

		[Required]
		[Display(Name = "Destination Page Name")]
		public string DestinationPageName { get; set; }
	}

	/// <summary>
	/// Represents the data necessary to show a wiki page that references another wiki page
	/// </summary>
	public class WikiReferralModel
	{
		public string Link { get; set; }
		public string Excerpt { get; set; }
	}

	/// <summary>
	/// Represents the data necessary to generate a diff between two revisions of a wiki page
	/// </summary>
	public class WikiDiffModel
	{
		public string PageName { get; set; }

		public int LeftRevision { get; set; }
		public string LeftMarkup { get; set; }

		public int RightRevision { get; set; }
		public string RightMarkup { get; set; }
	}

	/// <summary>
	/// Represents a wiki orphan for a list of wiki orphans
	/// Orphans are wiki pages that are not referenced by any other page
	/// </summary>
	public class WikiOrphanModel
	{
		public string PageName { get; set; }
		public DateTime LastUpdateTimeStamp { get; set; }
		public string LastUpdateUserName { get; set; }
	}

	/// <summary>
	/// Represents a wiki change entry for the WikiTextChangeLog module
	/// </summary>
	public class WikiTextChangelogModel
	{
		public DateTime CreateTimestamp { get; set; }
		public string Author { get; set; }
		public string PageName { get; set; }
		public int Revision { get; set; }
		public bool MinorEdit { get; set; }
		public string RevisionMessage { get; set; }
	}

	/// <summary>
	/// Represents a delete page entry on the DeletedPages list
	/// </summary>
	public class DeletedWikiPageDisplayModel
	{
		[Display(Name = "Page Name")]
		public string PageName { get; set; }

		[Display(Name = "Revision Count")]
		public int RevisionCount { get; set; }

		[Display(Name = "Existing Page")]
		public bool HasExistingRevisions { get; set; }
	}

	/// <summary>
	/// Represents a page on the SiteMap page
	/// </summary>
	public class SiteMapModel
	{
		[Display(Name = "Page")]
		public string PageName { get; set; }

		[Display(Name = "Type")]
		public bool IsWiki { get; set; }

		[Display(Name = "Access Restriction")]
		public string AccessRestriction { get; set; }
	}
}