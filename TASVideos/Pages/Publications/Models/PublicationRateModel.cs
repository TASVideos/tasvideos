using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Publications.Models
{
	public class PublicationRateModel
	{
		public string Title { get; set; }

		[Display(Name = "Tech Rating")]
		public double? TechRating { get; set; }

		[Display(Name = "Entertainment Rating")]
		public double? EntertainmentRating { get; set; }
	}
}
