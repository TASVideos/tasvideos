namespace TASVideos.Data.Entity.Awards
{
	public enum AwardType
	{
		User = 1,
		Movie
	}

	public class Award
	{
		public int Id { get; set; }
		public AwardType Type { get; set; }
		public string ShortName { get; set; }
		public string Description { get; set; }
	}
}
