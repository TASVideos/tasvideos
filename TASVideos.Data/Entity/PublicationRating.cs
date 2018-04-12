namespace TASVideos.Data.Entity
{
	public enum PublicationRatingType
	{
		Entertainmnet, TechQuality
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
}
