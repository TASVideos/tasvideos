using System.ComponentModel.DataAnnotations;
using TASVideos.RazorPages.Models;

namespace TASVideos.RazorPages.Pages.Wiki.Models
{
	public class WikiMoveModel
	{
		public string OriginalPageName { get; set; } = "";

		[Required]
		[ValidWikiPageName]
		[Display(Name = "Destination Page Name")]
		public string DestinationPageName { get; set; } = "";
	}
}
