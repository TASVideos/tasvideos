using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TASVideos.Data.Entity.Game
{
	public enum RomTypes
	{
		Unknown,
		Good,
		Hack,
		Bad
	}

	public class GameRom : BaseEntity
	{
		public int Id { get; set; }

		public int GameId { get; set; }
		public virtual Game Game { get; set; }

		public ICollection<Publication> Publications { get; set; } = new HashSet<Publication>();
		public ICollection<Submission> Submissions { get; set; } = new HashSet<Submission>();

		[StringLength(32)]
		public string Md5 { get; set; }

		[StringLength(40)]
		public string Sha1 { get; set; }

		[StringLength(255)]
		public string Name { get; set; }

		public RomTypes Type { get; set; }

		public string Region { get; set; }
		public string Version { get; set; }
	}

	public static class RomExtensions
	{
		public static IQueryable<GameRom> ForGame(this IQueryable<GameRom> query, int gameId)
		{
			return query.Where(g => g.GameId == gameId);
		}

		public static IQueryable<GameRom> ForSystem(this IQueryable<GameRom> query, int systemId)
		{
			return query.Where(g => g.Game.SystemId == systemId);
		}
	}
}
