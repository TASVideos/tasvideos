using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Publications.Models
{
	public class PublicationTierEditModel
	{
		public string Title { get; set; }

		[Display(Name = "Tier")]
		public int TierId { get; set; }
	}
}
