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
	}
}
