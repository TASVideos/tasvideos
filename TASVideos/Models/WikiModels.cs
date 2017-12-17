using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Models
{
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

	public class WikiMoveModel
	{
		public string OriginalPageName { get; set; }

		[Required]
		[Display(Name = "Destination Page Name")]
		public string DestinationPageName { get; set; }
	}

	public class WikiReferralModel
	{
		public string Link { get; set; }
		public string Excerpt { get; set; }
	}

	public class WikiDiffModel
	{
		public string PageName { get; set; }

		public int LeftRevision { get; set; }
		public string LeftMarkup { get; set; }

		public int RightRevision { get; set; }
		public string RightMarkup { get; set; }
	}

	public class WikiOrphanModel
	{
		public string PageName { get; set; }
		public DateTime LastUpdateTimeStamp { get; set; }
		public string LastUpdateUserName { get; set; }
	}
}