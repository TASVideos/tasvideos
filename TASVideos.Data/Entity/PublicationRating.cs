namespace TASVideos.Data.Entity;

public class PublicationRating
{
	public int UserId { get; set; }
	public User? User { get; set; }

	public int PublicationId { get; set; }
	public Publication? Publication { get; set; }

	public double Value { get; set; }
}

public static class PublicationRatingExtensions
{
	public static IQueryable<PublicationRating> ForPublication(this IQueryable<PublicationRating> query, int publicationId)
		=> query.Where(pr => pr.PublicationId == publicationId);

	public static IQueryable<PublicationRating> ForUser(this IQueryable<PublicationRating> query, int userId)
		=> query.Where(pr => pr.UserId == userId);

	public static IQueryable<PublicationRating> IncludeObsolete(this IQueryable<PublicationRating> query, bool includeObsolete)
		=> !includeObsolete
			? query.Where(pr => pr.Publication!.ObsoletedById == null)
			: query;
}
