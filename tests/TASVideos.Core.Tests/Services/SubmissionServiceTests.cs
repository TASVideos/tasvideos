using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.Core.Services;
using TASVideos.Core.Settings;
using TASVideos.Data.Entity;
using static TASVideos.Data.Entity.SubmissionStatus;

namespace TASVideos.Core.Tests.Services
{
	[TestClass]
	public class SubmissionServiceTests
	{
		private const int MinimumHoursBeforeJudgment = 72;
		private readonly SubmissionService _submissionService;

		private static DateTime TooNewToJudge => DateTime.UtcNow;

		private static DateTime OldEnoughToBeJudged
			=> DateTime.UtcNow.AddHours(-1 - MinimumHoursBeforeJudgment);

		private static readonly IEnumerable<PermissionTo> BasicUserPerms = new[] { PermissionTo.SubmitMovies };
		private static readonly IEnumerable<PermissionTo> JudgePerms = new[] { PermissionTo.SubmitMovies, PermissionTo.JudgeSubmissions };
		private static readonly IEnumerable<PermissionTo> PublisherPerms = new[] { PermissionTo.SubmitMovies, PermissionTo.PublishMovies };
		private static readonly IEnumerable<PermissionTo> Override = new[] { PermissionTo.OverrideSubmissionStatus };

		public SubmissionServiceTests()
		{
			var settings = new AppSettings { MinimumHoursBeforeJudgment = MinimumHoursBeforeJudgment };
			_submissionService = new SubmissionService(settings);
		}

		[TestMethod]
		public void Published_CanNotChange()
		{
			var result = _submissionService.AvailableStatuses(
				Published,
				Override,
				OldEnoughToBeJudged,
				true,
				true,
				true).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
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
			var expected = new[] { current }.Concat(canChangeTo).ToList();
			var result = _submissionService.AvailableStatuses(
				current,
				BasicUserPerms,
				OldEnoughToBeJudged,
				isAuthorOrSubmitter: true,
				isJudge: false,
				isPublisher: false).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(expected.Count, result.Count);
			foreach (var status in expected)
			{
				Assert.IsTrue(result.Contains(status));
			}
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
		public void Submitter_IsJudge(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
		{
			var expected = new[] { current }.Concat(canChangeTo).ToList();
			var result = _submissionService.AvailableStatuses(
				current,
				JudgePerms,
				OldEnoughToBeJudged,
				true,
				false,
				false).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(expected.Count, result.Count);
			foreach (var status in expected)
			{
				Assert.IsTrue(result.Contains(status));
			}
		}

		[DataRow(New, new[] { Cancelled })]
		[DataRow(Delayed, new[] { Cancelled })]
		[DataRow(NeedsMoreInfo, new[] { Cancelled })]
		[DataRow(JudgingUnderWay, new[] { Cancelled })]
		[DataRow(Accepted, new[] { PublicationUnderway, Cancelled })]
		[DataRow(PublicationUnderway, new[] { Cancelled })]
		[DataRow(Rejected, new SubmissionStatus[0])]
		[DataRow(Cancelled, new[] { New })]
		[TestMethod]
		public void Submitter_IsPublisher(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
		{
			var expected = new[] { current }.Concat(canChangeTo).ToList();
			var result = _submissionService.AvailableStatuses(
				current,
				PublisherPerms,
				OldEnoughToBeJudged,
				true,
				false,
				false).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(expected.Count, result.Count);
			foreach (var status in expected)
			{
				Assert.IsTrue(result.Contains(status));
			}
		}

		[DataRow(New, new[] { JudgingUnderWay, Cancelled })]
		[DataRow(Delayed, new[] { New, JudgingUnderWay, Cancelled })]
		[DataRow(NeedsMoreInfo, new[] { New, JudgingUnderWay, Cancelled })]
		[DataRow(JudgingUnderWay, new[] { New, Cancelled })]
		[DataRow(Accepted, new[] { New, JudgingUnderWay, Cancelled })]
		[DataRow(PublicationUnderway, new[] { New, JudgingUnderWay, Cancelled })]
		[DataRow(Rejected, new[] { New, JudgingUnderWay })]
		[DataRow(Cancelled, new[] { New, JudgingUnderWay })]
		[TestMethod]
		public void Judge_ButNotSubmitter_BeforeAllowedJudgmentWindow(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
		{
			var expected = new[] { current }.Concat(canChangeTo).ToList();
			var result = _submissionService.AvailableStatuses(
				current,
				JudgePerms,
				TooNewToJudge,
				isAuthorOrSubmitter: false,
				isJudge: true,
				isPublisher: false).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(expected.Count, result.Count);
			foreach (var status in expected)
			{
				Assert.IsTrue(result.Contains(status));
			}
		}

		[DataRow(New, new[] { JudgingUnderWay, Cancelled })]
		[DataRow(Delayed, new[] { New, NeedsMoreInfo, JudgingUnderWay, Accepted, Rejected, Cancelled })]
		[DataRow(NeedsMoreInfo, new[] { New, Delayed, JudgingUnderWay, Accepted, Rejected, Cancelled })]
		[DataRow(JudgingUnderWay, new[] { New, Delayed, NeedsMoreInfo, Accepted, Rejected, Cancelled })]
		[DataRow(Accepted, new[] { New, Delayed, NeedsMoreInfo, JudgingUnderWay, Rejected, Cancelled })]
		[DataRow(PublicationUnderway, new[] { New, Delayed, NeedsMoreInfo, JudgingUnderWay, Accepted, Rejected, Cancelled })]
		[DataRow(Rejected, new[] { New, JudgingUnderWay })]
		[DataRow(Cancelled, new[] { New, JudgingUnderWay })]
		[TestMethod]
		public void Judge_ButNotSubmitter_AfterAllowedJudgmentWindow(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
		{
			var expected = new[] { current }.Concat(canChangeTo).ToList();
			var result = _submissionService.AvailableStatuses(
				current,
				JudgePerms,
				OldEnoughToBeJudged,
				isAuthorOrSubmitter: false,
				isJudge: true,
				isPublisher: false).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(expected.Count, result.Count);
			foreach (var status in expected)
			{
				Assert.IsTrue(result.Contains(status));
			}
		}

		[DataRow(New, new SubmissionStatus[0])]
		[DataRow(Delayed, new SubmissionStatus[0])]
		[DataRow(NeedsMoreInfo, new SubmissionStatus[0])]
		[DataRow(JudgingUnderWay, new SubmissionStatus[0])]
		[DataRow(Accepted, new[] { PublicationUnderway })]
		[DataRow(PublicationUnderway, new[] { Accepted })]
		[DataRow(Rejected, new SubmissionStatus[0])]
		[DataRow(Cancelled, new SubmissionStatus[0])]
		[TestMethod]
		public void Publisher_ButNotSubmitter_BeforeAllowedJudgmentWindow_CanNotChangeStatus(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
		{
			var expected = new[] { current }.Concat(canChangeTo).ToList();
			var result = _submissionService.AvailableStatuses(
				current,
				PublisherPerms,
				TooNewToJudge,
				isAuthorOrSubmitter: false,
				isJudge: false,
				isPublisher: true).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(expected.Count, result.Count);
			foreach (var status in expected)
			{
				Assert.IsTrue(result.Contains(status));
			}
		}

		[DataRow(New, new SubmissionStatus[0])]
		[DataRow(Delayed, new SubmissionStatus[0])]
		[DataRow(NeedsMoreInfo, new SubmissionStatus[0])]
		[DataRow(JudgingUnderWay, new SubmissionStatus[0])]
		[DataRow(Accepted, new[] { PublicationUnderway })]
		[DataRow(PublicationUnderway, new[] { Accepted })]
		[DataRow(Rejected, new SubmissionStatus[0])]
		[DataRow(Cancelled, new SubmissionStatus[0])]
		[TestMethod]
		public void Publisher_ButNotSubmitter_AfterAllowedJudgmentWindow(SubmissionStatus current, IEnumerable<SubmissionStatus> canChangeTo)
		{
			var expected = new[] { current }.Concat(canChangeTo).ToList();
			var result = _submissionService.AvailableStatuses(
				current,
				PublisherPerms,
				OldEnoughToBeJudged,
				isAuthorOrSubmitter: false,
				isJudge: false,
				isPublisher: true).ToList();

			Assert.IsNotNull(result);
			Assert.AreEqual(expected.Count, result.Count);
			foreach (var status in expected)
			{
				Assert.IsTrue(result.Contains(status));
			}
		}

		[TestMethod]
		public void OverrideSubmissions_AnyStatusButPublished()
		{
			var exceptPublished = Enum.GetValues(typeof(SubmissionStatus))
				.Cast<SubmissionStatus>()
				.Except(new[] { Published })
				.OrderBy(s => s)
				.ToList();

			foreach (var current in exceptPublished)
			{
				var result = _submissionService.AvailableStatuses(
					current,
					Override,
					TooNewToJudge,
					false,
					false,
					false).ToList();

				Assert.IsNotNull(result);
				Assert.AreEqual(exceptPublished.Count, result.Count);
				Assert.IsTrue(result.SequenceEqual(exceptPublished));
			}
		}
	}
}
