using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Data.Entity;
using TASVideos.Data.Helpers;
using static TASVideos.Data.Entity.SubmissionStatus;

namespace TASVideos.Test.Data.Helpers
{
	[TestClass]
	public class AvailableStatusesTests
	{
		private static DateTime OldEnoughToBeJudged
			=> DateTime.UtcNow.AddHours(-1 - SiteGlobalConstants.MinimumHoursBeforeJudgment);

		private static DateTime TooNewToJudge => DateTime.UtcNow;

		private static IEnumerable<PermissionTo> BasicUserPerms = new[] { PermissionTo.SubmitMovies };

		private static IEnumerable<PermissionTo> Override = new[] { PermissionTo.OverrideSubmissionStatus };

		[TestMethod]
		public void Published_CanNotChange()
		{
			var result = SubmissionHelper.AvailableStatuses(
				Published,
				Override,
				OldEnoughToBeJudged,
				true,
				true);

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(Published, result.Single());
		}

		[DataRow(New, new[] { Cancelled })]
		[DataRow(Delayed, new[] { Cancelled })]
		[DataRow(NeedsMoreInfo, new[] { Cancelled })]
		[DataRow(JudgingUnderWay, new[] { Cancelled })]
		[DataRow(Accepted, new[] { Cancelled })]
		[DataRow(PublicationUnderway, new[] { Cancelled })]
		[DataRow(Rejected, new SubmissionStatus[0])]
		[DataRow(Cancelled, new[] { New })]
		[TestMethod]
		public void Submitter_BasicPerms(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
		{
			var expected = new[] { current }.Concat(canChangeTo);
			var result = SubmissionHelper.AvailableStatuses(
				current,
				BasicUserPerms,
				OldEnoughToBeJudged,
				true,
				false);

			Assert.IsNotNull(result);
			Assert.AreEqual(expected.Count(), result.Count());
			foreach (var status in expected)
			{
				Assert.IsTrue(result.Contains(status));
			}
		}

		
		[TestMethod]
		public void OverrideSubmissions_AnyStatucButPublished()
		{
			var exceptPublished = Enum.GetValues(typeof(SubmissionStatus))
				.Cast<SubmissionStatus>()
				.Except(new[] { Published })
				.OrderBy(s => s);

			foreach (var current in exceptPublished)
			{
				var result = SubmissionHelper.AvailableStatuses(
					current,
					Override,
					TooNewToJudge,
					false,
					false);

				Assert.IsNotNull(result);
				Assert.AreEqual(exceptPublished.Count(), result.Count());
				Assert.IsTrue(result.SequenceEqual(exceptPublished));
			}
		}
	}
}
