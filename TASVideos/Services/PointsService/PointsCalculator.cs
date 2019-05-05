using System.Collections.Generic;
using System.Linq;

namespace TASVideos.Services
{
	public static class PointsCalculator
	{
		public static decimal PlayerPoints(ICollection<Publication> ratings)
		{
			if (ratings == null || !ratings.Any())
			{
				return 0;
			}

			return ratings.Count * SiteGlobalConstants.MinimumPlayerPointsForPublication;
		}

		// TODO
		public static string PlayerRank(decimal points)
		{
			if (points < 1)
			{
				return "Former Player";
			}

			return "Player";
		}

		/// <summary>
		/// Represents all the data necessary from a publication to factor into player points
		/// </summary>
		public class Publication
		{
			// We still factor in obsolete movies but only at a number less than zero
			// for the purpose of determining "former player" rank
			public bool Obsolete { get; set; } 
			public decimal AverageRating { get; set; }
			public int RatingCount { get; set; }
			public decimal TierWeight { get; set; }

			// We count ratings from "important" users more than so than normal users (eww)
			// Also, when people abuse ratings their weight gets set to near zero
			public decimal UserWeight { get; set; } 
		}
	}
}
