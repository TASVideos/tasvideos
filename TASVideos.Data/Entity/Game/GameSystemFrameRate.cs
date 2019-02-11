using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TASVideos.Data.Entity.Game
{
	public class GameSystemFrameRate : BaseEntity
	{
		public int Id { get; set; }

		public int GameSystemId { get; set; }
		public virtual GameSystem System { get; set; }

		public double FrameRate { get; set; }

		[Required]
		[StringLength(8)]
		public string RegionCode { get; set; }
		public bool Preliminary { get; set; }
	}

	public static class GameSystemExtensions
	{
		public static IQueryable<GameSystemFrameRate> ForSystem(this IQueryable<GameSystemFrameRate> query, int systemId)
		{
			return query.Where(g => g.GameSystemId == systemId);
		}
	}
}
