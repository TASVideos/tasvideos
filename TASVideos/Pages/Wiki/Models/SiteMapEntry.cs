using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Wiki.Models
{
	public class SiteMapEntry
	{
		[Display(Name = "Page")]
		public string PageName { get; set; }

		[Display(Name = "Type")]
		public bool IsWiki { get; set; }

		[Display(Name = "Access Restriction")]
		public string AccessRestriction { get; set; }
	}
}
