using Microsoft.EntityFrameworkCore;
using TASVideos.Data.Entity;
using TASVideos.Pages.Submissions.RejectionReasons;
using TASVideos.Tests.Base;
using static TASVideos.RazorPages.Tests.RazorTestHelpers;

namespace TASVideos.RazorPages.Tests.Pages.Submissions.RejectionReasons;

[TestClass]
public class IndexModelTests : TestDbBase
{
	private readonly IndexModel _model;

	public IndexModelTests()
	{
		_model = new IndexModel(_db);
	}

	[TestMethod]
	public async Task OnGet_WithEmptyDatabase_ReturnsEmptyReasonsList()
	{
		await _model.OnGet();

		Assert.AreEqual(0, _model.Reasons.Count);
	}

	[TestMethod]
	public async Task OnGet_WithRejectionReasons_LoadsReasonsWithSubmissionCounts()
	{
		// Create rejection reasons
		var reason1 = new SubmissionRejectionReason { DisplayName = "Invalid Input" };
		var reason2 = new SubmissionRejectionReason { DisplayName = "Suboptimal" };
		var reason3 = new SubmissionRejectionReason { DisplayName = "Bad ROM" };
		_db.SubmissionRejectionReasons.AddRange(reason1, reason2, reason3);
		await _db.SaveChangesAsync();

		// Create submissions with different statuses
		var submission1 = _db.AddSubmission().Entity;
		submission1.Status = SubmissionStatus.Rejected;
		submission1.RejectionReasonId = reason1.Id;

		var submission2 = _db.AddSubmission().Entity;
		submission2.Status = SubmissionStatus.Rejected;
		submission2.RejectionReasonId = reason1.Id;

		var submission3 = _db.AddSubmission().Entity;
		submission3.Status = SubmissionStatus.Rejected;
		submission3.RejectionReasonId = reason2.Id;

		// This submission should not be counted as it's not rejected
		var submission4 = _db.AddSubmission().Entity;
		submission4.Status = SubmissionStatus.New;
		submission4.RejectionReasonId = reason2.Id;

		await _db.SaveChangesAsync();

		await _model.OnGet();

		Assert.AreEqual(3, _model.Reasons.Count);

		var loadedReason1 = _model.Reasons.First(r => r.Reason == "Invalid Input");
		Assert.AreEqual(2, loadedReason1.SubmissionCount);

		var loadedReason2 = _model.Reasons.First(r => r.Reason == "Suboptimal");
		Assert.AreEqual(1, loadedReason2.SubmissionCount);

		var loadedReason3 = _model.Reasons.First(r => r.Reason == "Bad ROM");
		Assert.AreEqual(0, loadedReason3.SubmissionCount);
	}

	[TestMethod]
	public async Task OnPost_WithoutPermission_ReturnsAccessDenied()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, []);

		var result = await _model.OnPost("New Reason");

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Account/AccessDenied", redirect.PageName);
	}

	[TestMethod]
	public async Task OnPost_WithPermission_CreatesNewReasonAndRedirects()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.RejectionReasonMaintenance]);

		var result = await _model.OnPost("New Reason");

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("Index", redirect.PageName);

		var createdReason = _db.SubmissionRejectionReasons.Single(r => r.DisplayName == "New Reason");
		Assert.IsNotNull(createdReason);
		Assert.AreEqual("New Reason", createdReason.DisplayName);
	}

	[TestMethod]
	public async Task OnPost_WithDuplicateName_AddsModelErrorAndReturnsPage()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.RejectionReasonMaintenance]);

		// Create existing reason
		_db.SubmissionRejectionReasons.Add(new SubmissionRejectionReason
		{
			DisplayName = "Existing Reason"
		});
		await _db.SaveChangesAsync();

		var result = await _model.OnPost("Existing Reason");

		Assert.IsInstanceOfType<PageResult>(result);
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsTrue(_model.ModelState.ContainsKey("displayName"));
		Assert.IsTrue(_model.ModelState["displayName"]!.Errors[0].ErrorMessage.Contains("already exists"));

		// Should not create duplicate
		Assert.AreEqual(1, _db.SubmissionRejectionReasons.Count(r => r.DisplayName == "Existing Reason"));
	}

	[TestMethod]
	public async Task OnPostDelete_WithoutPermission_ReturnsAccessDenied()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, []);

		var result = await _model.OnPostDelete(1);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("/Account/AccessDenied", redirect.PageName);
	}

	[TestMethod]
	public async Task OnPostDelete_WithNonExistentId_ReturnsNotFound()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.RejectionReasonMaintenance]);

		var result = await _model.OnPostDelete(999);

		Assert.IsInstanceOfType<NotFoundResult>(result);
	}

	[TestMethod]
	public async Task OnPostDelete_WithValidId_DeletesReasonAndRedirects()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.RejectionReasonMaintenance]);

		// Create reason to delete
		var reason = new SubmissionRejectionReason { DisplayName = "To Delete" };
		_db.SubmissionRejectionReasons.Add(reason);
		await _db.SaveChangesAsync();

		var result = await _model.OnPostDelete(reason.Id);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("Index", redirect.PageName);

		// Verify deletion
		var deletedReason = await _db.SubmissionRejectionReasons
			.SingleOrDefaultAsync(r => r.Id == reason.Id);
		Assert.IsNull(deletedReason);
	}

	[TestMethod]
	public async Task Initialize_PopulatesReasonsCorrectly()
	{
		// Create reasons with different submission counts
		var reason1 = new SubmissionRejectionReason { DisplayName = "Reason 1" };
		var reason2 = new SubmissionRejectionReason { DisplayName = "Reason 2" };
		_db.SubmissionRejectionReasons.AddRange(reason1, reason2);
		await _db.SaveChangesAsync();

		// Add submissions for testing counts
		var submission1 = _db.AddSubmission().Entity;
		submission1.Status = SubmissionStatus.Rejected;
		submission1.RejectionReasonId = reason1.Id;

		var submission2 = _db.AddSubmission().Entity;
		submission2.Status = SubmissionStatus.Published;  // Should not be counted
		submission2.RejectionReasonId = reason1.Id;

		await _db.SaveChangesAsync();

		// Call Initialize through OnGet
		await _model.OnGet();

		Assert.AreEqual(2, _model.Reasons.Count);
		var testReason1 = _model.Reasons.First(r => r.Reason == "Reason 1");
		Assert.AreEqual(reason1.Id, testReason1.Id);
		Assert.AreEqual("Reason 1", testReason1.Reason);
		Assert.AreEqual(1, testReason1.SubmissionCount);

		var testReason2 = _model.Reasons.First(r => r.Reason == "Reason 2");
		Assert.AreEqual(reason2.Id, testReason2.Id);
		Assert.AreEqual("Reason 2", testReason2.Reason);
		Assert.AreEqual(0, testReason2.SubmissionCount);
	}

	[TestMethod]
	public void Rejection_Record_HasCorrectProperties()
	{
		var rejection = new IndexModel.Rejection(123, "Test Reason", 5);

		Assert.AreEqual(123, rejection.Id);
		Assert.AreEqual("Test Reason", rejection.Reason);
		Assert.AreEqual(5, rejection.SubmissionCount);
	}

	[TestMethod]
	public async Task OnPost_WithWhitespaceDisplayName_CreatesReasonWithTrimmedName()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_model, user, [PermissionTo.RejectionReasonMaintenance]);

		await _model.OnPost("  Spaced Reason  ");

		var createdReason = _db.SubmissionRejectionReasons.Single();
		Assert.AreEqual("  Spaced Reason  ", createdReason.DisplayName);
	}
}
