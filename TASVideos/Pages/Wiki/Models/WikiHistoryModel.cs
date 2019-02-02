using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Wiki.Models
{
	public class WikiHistoryModel
	{
		public string PageName { get; set; }

		public IEnumerable<WikiRevisionModel> Revisions { get; set; } = new List<WikiRevisionModel>();

		public class WikiRevisionModel
		{
			[Display(Name = "Revision")]
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
}
