using System;
using System.Collections.Generic;
using System.Linq;

namespace TASVideos.Services
{
	public static class PointsCalculator
	{
		/// <summary>
		/// Calculates the player points a player would receive for a movie
		/// </summary>
		/// <param name="publications">The rating data for a given movie</param>
		/// <param name="averageRatingCount">The average number of ratings a movie receives, across the entire site</param>
		/// <returns>The player points calculated</returns>
		public static double PlayerPoints(ICollection<Publication> publications, double averageRatingCount)
		{
			if (publications == null || !publications.Any())
			{
				return 0;
			}

			var points = publications
				.Select(p => PlayerPointsForMovie(p, averageRatingCount))
				.Sum();

			return points;
		}

		/// <summary>
		/// Determines the player rank based on the given amount of player points
		/// </summary>
		public static string PlayerRank(decimal points)
		{
			if (points <= 0)
			{
				return null;
			}

			if (points < 1)
			{
				return PlayerRanks.FormerPlayer;
			}

			if (points < 250)
			{
				return PlayerRanks.Player;
			}

			if (points < 500)
			{
				return PlayerRanks.ActivePlayer;
			}

			if (points < 1000)
			{
				return PlayerRanks.ExperiencedPlayer;
			}

			if (points < 2000)
			{
				return PlayerRanks.SkilledPlayer;
			}

			return PlayerRanks.ExpertPlayer;
		}

		internal static double PlayerPointsForMovie(Publication publication, double averageRatingCount)
		{
			var exp = RatingExponent(publication.RatingCount, averageRatingCount);

			var rawPoints = Math.Pow(publication.AverageRating, exp);
			var authorMultiplier = Math.Pow(publication.AuthorCount, -0.5);
			var actual = rawPoints * authorMultiplier * publication.TierWeight;

			if (actual < PlayerPointConstants.MinimumPlayerPointsForPublication)
			{
				actual = PlayerPointConstants.MinimumPlayerPointsForPublication;
			}

			if (publication.Obsolete)
			{
				actual *= PlayerPointConstants.ObsoleteMultiplier;
			}

			return actual;
		}

		internal static double RatingExponent(int total, double averageRatingPerPublications)
		{
			if (total == 0)
			{
				return 0;
			}

			var exponent = 2.6 - (0.2 * averageRatingPerPublications / total);
			if (exponent < 1)
			{
				exponent = 1;
			}

			return exponent;
		}

		/// <summary>
		/// Represents all the data necessary from a publication to factor into player points
		/// </summary>
		public class Publication
		{
			public int Id { get; set; }

			// We still factor in obsolete movies but only at a number less than zero
			// for the purpose of determining "former player" rank
			public bool Obsolete { get; set; } 
			public double AverageRating { get; set; }
			public int RatingCount { get; set; }
			public double TierWeight { get; set; }
			public int AuthorCount { get; set; }
		}
	}
}
