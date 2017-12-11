using System.ComponentModel.DataAnnotations;

namespace TASVideos.Models
{
	public class WikiViewModel
	{
		public string PageName { get; set; }
		public string Markup { get; set; }
		public int DbId { get; set; }
	}

	public class WikiEditModel
    {
		[Required]
		public string PageName { get; set; }

		[Required]
		public string Markup { get; set; }

		[Display(Name = "Minor Edit")]
		public bool MinorEdit { get; set; }

		[Required] // Yeah, I did that
		[Display(Name = "Edit Comments")]
		public string RevisionMessage { get; set; }
    }
}
