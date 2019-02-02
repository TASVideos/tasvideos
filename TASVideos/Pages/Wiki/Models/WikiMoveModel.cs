using System.ComponentModel.DataAnnotations;
using TASVideos.Models;

namespace TASVideos.Pages.Wiki.Models
{
	public class WikiMoveModel
	{
		public string OriginalPageName { get; set; }

		[Required]
		[ValidWikiPageName]
		[Display(Name = "Destination Page Name")]
		public string DestinationPageName { get; set; }
	}
}
