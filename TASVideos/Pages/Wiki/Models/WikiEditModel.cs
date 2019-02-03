using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Wiki.Models
{
	public class WikiEditModel
	{
		[Required]
		public string Markup { get; set; }

		[Display(Name = "Minor Edit")]
		public bool MinorEdit { get; set; }

		[Required] // Yeah, I did that
		[Display(Name = "Edit Comments")]
		[MaxLength(100)]
		public string RevisionMessage { get; set; }
	}
}
