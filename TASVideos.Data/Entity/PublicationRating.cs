using System.Linq;

namespace TASVideos.Data.Entity
{
	public enum PublicationRatingType
	{
		Entertainment, TechQuality
	}

	public class PublicationRating
	{
		public int UserId { get; set; }
		public virtual User User { get; set; }

		public int PublicationId { get; set; }
		public virtual Publication Publication { get; set; }

		public PublicationRatingType Type { get; set; }

		public double Value { get; set; }
	}

	public static class PublicationRatingExtensions
	{
		public static IQueryable<PublicationRating> ForPublication(this IQueryable<PublicationRating> query, int publicationId)
		{
			return query.Where(pr => pr.PublicationId == publicationId);
		}

		public static IQueryable<PublicationRating> ForUser(this IQueryable<PublicationRating> query, int userId)
		{
			return query.Where(pr => pr.UserId == userId);
		}
	}
}
