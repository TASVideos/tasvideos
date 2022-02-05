using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Ratings.Models;

public class PublicationRatingsModel
{
	public string PublicationTitle { get; set; } = "";

	public IEnumerable<RatingEntry> Ratings { get; set; } = new List<RatingEntry>();

	public double OverallRating { get; set; }

	public class RatingEntry
	{
		[Display(Name = "UserName")]
		public string UserName { get; set; } = "";

		[Display(Name = "Rating")]
		public double Rating { get; set; }

		public bool IsPublic { get; set; }
	}
}
