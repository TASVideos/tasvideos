using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Wiki.Models
{
	public class DeletedWikiPageDisplayModel
	{
		[Display(Name = "Page Name")]
		public string PageName { get; set; }

		[Display(Name = "Revision Count")]
		public int RevisionCount { get; set; }

		[Display(Name = "Existing Page")]
		public bool HasExistingRevisions { get; set; }
	}
}
