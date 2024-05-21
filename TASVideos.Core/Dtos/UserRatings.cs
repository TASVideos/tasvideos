namespace TASVideos.Core.Services;

/// <summary>
/// Represents a user's ratings
/// </summary>
public class UserRatings
{
	public int Id { get; init; }
	public string UserName { get; init; } = "";
	public bool PublicRatings { get; init; }

	public RatingPageOf<Rating> Ratings { get; set; } = new([]);

	public class Rating
	{
		[TableIgnore]
		public int PublicationId { get; init; }

		[Sortable]
		public string PublicationTitle { get; init; } = "";

		[Sortable]
		public double Value { get; init; }

		[Sortable]
		public bool IsObsolete { get; init; }
	}
}

public class RatingRequest : PagingModel
{
	public RatingRequest()
	{
		PageSize = 50;
		Sort = "-Value";
	}

	public bool IncludeObsolete { get; set; }
}

public class RatingPageOf<T>(IEnumerable<T> items) : PageOf<T>(items)
{
	public bool IncludeObsolete { get; set; }
}
