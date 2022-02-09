namespace TASVideos.Data.Entity.Game;

public class GameSystemFrameRate : BaseEntity
{
	public int Id { get; set; }

	public int GameSystemId { get; set; }
	public virtual GameSystem? System { get; set; }

	public double FrameRate { get; set; }

	[Required]
	[StringLength(8)]
	public string RegionCode { get; set; } = "";
	public bool Preliminary { get; set; }

	public bool Obsolete { get; set; }
}

public static class GameSystemFrameRateExtensions
{
	public static IQueryable<GameSystemFrameRate> ForSystem(this IQueryable<GameSystemFrameRate> query, int systemId)
	{
		return query.Where(sf => sf.GameSystemId == systemId);
	}

	public static IQueryable<GameSystemFrameRate> ForRegion(this IQueryable<GameSystemFrameRate> query, string region)
	{
		return query.Where(sf => sf.RegionCode == region);
	}

	public static IQueryable<GameSystemFrameRate> ThatAreCurrent(this IQueryable<GameSystemFrameRate> query)
	{
		return query.Where(sf => !sf.Obsolete);
	}
}
