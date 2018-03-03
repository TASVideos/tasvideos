namespace TASVideos.Data.Entity
{
	public class PublicationTag
	{
		public int PublicationId { get; set; }
		public virtual Publication Publication { get; set; }

		public int TagId { get; set; }
		public virtual Tag Tag { get; set; }
	}
}
