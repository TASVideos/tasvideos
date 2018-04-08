namespace TASVideos.Data.Entity.Awards
{
	public class UserAward
	{
		public int Id { get; set; }

		public int UserId { get; set; }
		public virtual User User { get; set; }

		public int AwardId { get; set; }
		public virtual Award Award { get; set; }

		public int Year { get; set; }
	}
}
