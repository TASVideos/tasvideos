namespace TASVideos.Core.Services;

/// <summary>
/// Represents a user's ratings
/// </summary>
public class UserRatings
{
	public int Id { get; init; }
	public string UserName { get; init; } = "";
	public bool PublicRatings { get; init; }

	public IEnumerable<Rating> Ratings { get; set; } = new List<Rating>();

	public class Rating
	{
		public int PublicationId { get; init; }
		public string PublicationTitle { get; init; } = "";
		public bool IsObsolete { get; init; }
		public double? Entertainment { get; init; }
		public double Tech { get; init; }

		public double Average => ((Entertainment ?? 0) + (Entertainment ?? 0) + Tech) / 3.0;
	}
}
