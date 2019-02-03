using System.Collections.Generic;

namespace TASVideos.Models
{
	/// <summary>
	/// Represents a user's ratings
	/// </summary>
	public class UserRatingsModel
	{
		public int Id { get; set; }
		public string UserName { get; set; }
		public bool PublicRatings { get; set; }

		public IEnumerable<Rating> Ratings { get; set; } = new List<Rating>();

		public class Rating
		{
			public int PublicationId { get; set; }
			public string PublicationTitle { get; set; }
			public bool IsObsolete { get; set; }
			public double? Entertainment { get; set; }
			public double Tech { get; set; }

			public double Average => ((Entertainment ?? 0) + (Entertainment ?? 0) + Tech) / 3.0;
		}
	}
}
