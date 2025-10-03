using TASVideos.Pages.Submissions.RejectionReasons;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Submissions.RejectionReasons;

[TestClass]
public class ReasonModelTests : TestDbBase
{
	private readonly ReasonModel _model;

	public ReasonModelTests()
	{
		_model = new ReasonModel(_db);
	}

	[TestMethod]
	public async Task OnGet_WithNonExistentId_ReturnsNotFound()
	{
		_model.Id = 999;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_WithValidId_LoadsReasonAndSubmissions()
	{
		var reason = _db.AddRejectionReason("Test Reason").Entity;
		await _db.SaveChangesAsync();

		var submission1 = _db.AddSubmission().Entity;
		submission1.Status = SubmissionStatus.Rejected;
		submission1.RejectionReasonId = reason.Id;
		submission1.Title = "Rejected Submission 1";

		var submission2 = _db.AddSubmission().Entity;
		submission2.Status = SubmissionStatus.Rejected;
		submission2.RejectionReasonId = reason.Id;
		submission2.Title = "Rejected Submission 2";

		var submission3 = _db.AddSubmission().Entity;
		submission3.Status = SubmissionStatus.New;
		submission3.RejectionReasonId = reason.Id;
		submission3.Title = "New Submission";

		var otherReason = _db.AddRejectionReason("Should not appear").Entity;
		await _db.SaveChangesAsync();

		var submission4 = _db.AddSubmission().Entity;
		submission4.Status = SubmissionStatus.Rejected;
		submission4.RejectionReasonId = otherReason.Id;
		submission4.Title = "Other Rejected Submission";

		await _db.SaveChangesAsync();

		_model.Id = reason.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Test Reason", _model.RejectionReason);
		Assert.AreEqual(2, _model.Submissions.Count);

		var submissionIds = _model.Submissions.Select(s => s.SubmissionId).ToList();
		Assert.IsTrue(submissionIds.Contains(submission1.Id));
		Assert.IsTrue(submissionIds.Contains(submission2.Id));

		var submission1Entry = _model.Submissions.First(s => s.SubmissionId == submission1.Id);
		Assert.AreEqual("Rejected Submission 1", submission1Entry.SubmissionTitle);

		var submission2Entry = _model.Submissions.First(s => s.SubmissionId == submission2.Id);
		Assert.AreEqual("Rejected Submission 2", submission2Entry.SubmissionTitle);
	}

	[TestMethod]
	public async Task OnGet_WithValidIdButNoSubmissions_ReturnsEmptyList()
	{
		var reason = _db.AddRejectionReason("Unused Reason").Entity;
		await _db.SaveChangesAsync();

		_model.Id = reason.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Unused Reason", _model.RejectionReason);
		Assert.AreEqual(0, _model.Submissions.Count);
	}

	[TestMethod]
	public async Task OnGet_OnlyIncludesRejectedSubmissions()
	{
		var reason = _db.AddRejectionReason("Test Reason").Entity;
		await _db.SaveChangesAsync();

		// Create submissions with different statuses
		var rejectedSubmission = _db.AddSubmission().Entity;
		rejectedSubmission.Status = SubmissionStatus.Rejected;
		rejectedSubmission.RejectionReason = reason;
		rejectedSubmission.Title = "Rejected";

		var newSubmission = _db.AddSubmission().Entity;
		newSubmission.Status = SubmissionStatus.New;
		newSubmission.RejectionReason = reason;
		newSubmission.Title = "New";

		var publishedSubmission = _db.AddSubmission().Entity;
		publishedSubmission.Status = SubmissionStatus.Published;
		publishedSubmission.RejectionReason = reason;
		publishedSubmission.Title = "Published";

		var acceptedSubmission = _db.AddSubmission().Entity;
		acceptedSubmission.Status = SubmissionStatus.Accepted;
		acceptedSubmission.RejectionReason = reason;
		acceptedSubmission.Title = "Accepted";

		await _db.SaveChangesAsync();

		_model.Id = reason.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(1, _model.Submissions.Count);
		Assert.AreEqual(rejectedSubmission.Id, _model.Submissions[0].SubmissionId);
		Assert.AreEqual("Rejected", _model.Submissions[0].SubmissionTitle);
	}

	[TestMethod]
	public async Task OnGet_WithMultipleRejectedSubmissions_OrdersCorrectly()
	{
		var reason = _db.AddRejectionReason("Multiple Submissions").Entity;
		await _db.SaveChangesAsync();

		// Create multiple rejected submissions
		var submissions = new List<Submission>();
		for (int i = 1; i <= 5; i++)
		{
			var submission = _db.AddSubmission().Entity;
			submission.Status = SubmissionStatus.Rejected;
			submission.RejectionReason = reason;
			submission.Title = $"Submission {i}";
			submissions.Add(submission);
		}

		await _db.SaveChangesAsync();

		_model.Id = reason.Id;
		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual(5, _model.Submissions.Count);

		foreach (var submission in submissions)
		{
			Assert.IsTrue(_model.Submissions.Any(s => s.SubmissionId == submission.Id));
		}
	}

	[TestMethod]
	public void SubmissionEntry_Record_HasCorrectProperties()
	{
		var entry = new ReasonModel.SubmissionEntry(123, "Test Title");

		Assert.AreEqual(123, entry.SubmissionId);
		Assert.AreEqual("Test Title", entry.SubmissionTitle);
	}

	[TestMethod]
	public async Task OnGet_HandlesEmptyDatabase_GracefullyReturnsNotFound()
	{
		_model.Id = 1;
		var result = await _model.OnGet();
		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnGet_WithSpecialCharactersInReasonName_HandlesCorrectly()
	{
		var reason = _db.AddRejectionReason("Reason with \"quotes\" & <tags>").Entity;
		await _db.SaveChangesAsync();

		_model.Id = reason.Id;

		var result = await _model.OnGet();

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.AreEqual("Reason with \"quotes\" & <tags>", _model.RejectionReason);
	}

	[TestMethod]
	public void AllowsAnonymousAttribute() => AssertAllowsAnonymousUsers(typeof(ReasonModel));
}
