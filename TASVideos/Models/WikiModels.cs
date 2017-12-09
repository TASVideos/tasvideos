using System.ComponentModel.DataAnnotations;

namespace TASVideos.Models
{
    public class WikiEditModel
    {
		public string PageName { get; set; }

		
		public string Markup { get; set; }

		[Display(Name = "Minor Edit")]
		public bool MinorEdit { get; set; }

		[Required] // Yeah, I did that
		[Display(Name = "Edit Comments")]
		public string EditComments { get; set; }
    }
}
