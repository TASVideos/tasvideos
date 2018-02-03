namespace TASVideos.Data.Entity.Game
{
    public class GameSystemFrameRate : BaseEntity
    {
		public int Id { get; set; }

		public int GameSystemId { get; set; }
		public virtual GameSystem System { get; set; }

		public double FrameRate { get; set; }
		public string RegionCode { get; set; }
		public bool Preliminary { get; set; }
    }
}
