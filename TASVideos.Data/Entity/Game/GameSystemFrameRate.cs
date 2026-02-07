namespace TASVideos.Data.Entity.Game;

public class GameSystemFrameRate : BaseEntity
{
	public int Id { get; set; }

	public int GameSystemId { get; set; }
	public GameSystem? System { get; set; }

	public double FrameRate { get; set; }

	public string RegionCode { get; set; } = "";
	public bool Preliminary { get; set; }

	public bool Obsolete { get; set; }

	public ICollection<Submission> Submissions { get; init; } = [];
	public ICollection<Publication> Publications { get; init; } = [];
}

public static class GameSystemFrameRateExtensions
{
	extension(IQueryable<GameSystemFrameRate> query)
	{
		public IQueryable<GameSystemFrameRate> ForSystem(int systemId)
			=> query.Where(sf => sf.GameSystemId == systemId);

		public IQueryable<GameSystemFrameRate> ForRegion(string region)
			=> query.Where(sf => sf.RegionCode == region);

		public IQueryable<GameSystemFrameRate> ThatAreCurrent()
			=> query.Where(sf => !sf.Obsolete);
	}
}
