namespace TASVideos.Data.Entity
{
	public class PublicationAuthor
	{
		public int UserId { get; set; }
		public virtual User Author { get; set; }

		public int PublicationId { get; set; }
		public virtual Publication Publication { get; set; }
	}
}
