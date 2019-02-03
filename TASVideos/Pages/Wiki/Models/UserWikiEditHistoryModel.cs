using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Wiki.Models
{
	public class UserWikiEditHistoryModel
	{
		public string UserName { get; set; }

		public IEnumerable<EditEntry> Edits { get; set; } = new List<EditEntry>();

		public class EditEntry
		{
			[Display(Name = "Revision")]
			public int Revision { get; set; }

			[Display(Name = "Date")]
			public DateTime CreateTimeStamp { get; set; }

			[Display(Name = "Page")]
			public string PageName { get; set; }

			[Display(Name = "Minor Edit")]
			public bool MinorEdit { get; set; }

			[Display(Name = "Revision Message")]
			public string RevisionMessage { get; set; }
		}
	}
}
