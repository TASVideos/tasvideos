namespace TASVideos.Data.Entity.Game
{
	public class GameGameGroup
	{
		public int GameId { get; set; }
		public virtual Game Game { get; set; }

		public int GameGroupId { get; set; }
		public virtual GameGroup GameGroup { get; set; }
	}
}
