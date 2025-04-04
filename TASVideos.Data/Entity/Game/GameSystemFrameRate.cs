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
	public static IQueryable<GameSystemFrameRate> ForSystem(this IQueryable<GameSystemFrameRate> query, int systemId)
		=> query.Where(sf => sf.GameSystemId == systemId);

	public static IQueryable<GameSystemFrameRate> ForRegion(this IQueryable<GameSystemFrameRate> query, string region)
		=> query.Where(sf => sf.RegionCode == region);

	public static IQueryable<GameSystemFrameRate> ThatAreCurrent(this IQueryable<GameSystemFrameRate> query)
		=> query.Where(sf => !sf.Obsolete);
}
