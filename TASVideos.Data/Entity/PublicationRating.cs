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
	extension(IQueryable<PublicationRating> query)
	{
		public IQueryable<PublicationRating> ForPublication(int publicationId)
			=> query.Where(pr => pr.PublicationId == publicationId);

		public IQueryable<PublicationRating> ForUser(int userId)
			=> query.Where(pr => pr.UserId == userId);

		public IQueryable<PublicationRating> IncludeObsolete(bool includeObsolete)
			=> !includeObsolete
				? query.Where(pr => pr.Publication!.ObsoletedById == null)
				: query;
	}
}
