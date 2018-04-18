namespace TASVideos.Data.Entity
{
	public class PublicationFlag
	{
		public int PublicationId { get; set; }
		public virtual Publication Publication { get; set; }

		public int FlagId { get; set; }
		public virtual Flag Flag { get; set; }
	}
}
