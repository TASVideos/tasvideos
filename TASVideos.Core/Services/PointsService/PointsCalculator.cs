namespace TASVideos.Core.Services;

public static class PointsCalculator
{
	/// <summary>
	/// Calculates the player points a player would receive for a movie
	/// </summary>
	/// <param name="publications">The rating data for a given movie</param>
	/// <param name="averageRatingsPerMovie">The average number of ratings a movie receives, across the entire site</param>
	/// <returns>The player points calculated</returns>
	public static double PlayerPoints(ICollection<Publication> publications, double averageRatingsPerMovie)
	{
		if (!publications.Any())
		{
			return 0;
		}

		var points = publications
			.Select(p => PlayerPointsForMovie(p, averageRatingsPerMovie))
			.Sum();

		return points;
	}

	/// <summary>
	/// Determines the player rank based on the given amount of player points
	/// </summary>
	public static string PlayerRank(decimal points)
	{
		return points switch
		{
			<= 0 => "",
			< 1 => PlayerRanks.FormerPlayer,
			< 250 => PlayerRanks.Player,
			< 500 => PlayerRanks.ActivePlayer,
			< 1000 => PlayerRanks.ExperiencedPlayer,
			< 2000 => PlayerRanks.SkilledPlayer,
			_ => PlayerRanks.ExpertPlayer
		};
	}

	internal static double PlayerPointsForMovie(Publication publication, double averageRatingCount)
	{
		averageRatingCount = Math.Max(averageRatingCount, 0);
		var exp = RatingExponent(publication.RatingCount, averageRatingCount);

		var rawPoints = Math.Pow(Math.Max(publication.AverageRating ?? 0, 0), exp);
		var authorMultiplier = Math.Pow(publication.AuthorCount, -0.5);
		var actual = rawPoints * authorMultiplier * publication.ClassWeight;

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
		public int Id { get; init; }

		// We still factor in obsolete movies but only at a number less than zero
		// for the purpose of determining "former player" rank
		public bool Obsolete { get; init; }
		public double? AverageRating { get; init; }
		public int RatingCount { get; init; }
		public double ClassWeight { get; init; }
		public int AuthorCount { get; init; }
	}
}
