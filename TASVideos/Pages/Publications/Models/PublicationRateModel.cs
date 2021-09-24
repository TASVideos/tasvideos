using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Publications.Models
{
	public class PublicationRateModel
	{
		public string Title { get; set; } = "";

		[Display(Name = "Tech Rating")]
		[Range(0, 10.0)]
		public double? TechRating { get; set; }

		[Display(Name = "Entertainment Rating")]
		[Range(0, 10.0)]
		public double? EntertainmentRating { get; set; }
	}
}
