using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Services;

// ReSharper disable InconsistentNaming
namespace TASVideos.Test.Services
{
	[TestClass]
	public class PointsCalculatorTests
	{
		[TestMethod]
		public void PlayerPoints_Null()
		{
			var actual = PointsCalculator.PlayerPoints(null);
			Assert.AreEqual(0, actual);
		}

		[TestMethod]
		public void PlayerPoints_NoPublications()
		{
			var actual = PointsCalculator.PlayerPoints(new List<PointsCalculator.Publication>());
			Assert.AreEqual(0, actual);
		}

		[TestMethod]
		public void PlayerPoints_PublicationMinimum_IfNotObsolete()
		{
			var publications = new[]
			{
				new PointsCalculator.Publication
				{
					Obsolete = false,
					TierWeight = 0,
					RatingCount = 0,
					AverageRating = 0
				}
			};

			var expected = publications.Length * SiteGlobalConstants.MinimumPlayerPointsForPublication;
			var actual = PointsCalculator.PlayerPoints(publications);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		[DataRow(-1, null)]
		[DataRow(0, null)]
		[DataRow(0.0001, PlayerRanks.FormerPlayer)]
		[DataRow(0.9, PlayerRanks.FormerPlayer)]
		[DataRow(1, PlayerRanks.Player)]
		[DataRow(249, PlayerRanks.Player)]
		[DataRow(250, PlayerRanks.ActivePlayer)]
		[DataRow(499, PlayerRanks.ActivePlayer)]
		[DataRow(500, PlayerRanks.ExperiencedPlayer)]
		[DataRow(999, PlayerRanks.ExperiencedPlayer)]
		[DataRow(1000, PlayerRanks.SkilledPlayer)]
		[DataRow(1999, PlayerRanks.SkilledPlayer)]
		[DataRow(2000, PlayerRanks.ExpertPlayer)]
		[DataRow(int.MaxValue, PlayerRanks.ExpertPlayer)]
		public void PlayerRank(double points, string expected)
		{
			var actual = PointsCalculator.PlayerRank((decimal)points);
			Assert.AreEqual(expected, actual);
		}
	}
}
