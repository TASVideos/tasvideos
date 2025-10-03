using TASVideos.Pages.Submissions.RejectionReasons;
using TASVideos.Tests.Base;

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
		var reason1 = _db.AddRejectionReason("Invalid Input").Entity;
		var reason2 = _db.AddRejectionReason("Suboptimal").Entity;
		_db.AddRejectionReason("Bad ROM");
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

		AssertAccessDenied(result);
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

		_db.AddRejectionReason("Existing Reason");
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

		AssertAccessDenied(result);
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

		var reason = _db.AddRejectionReason("To Delete").Entity;
		await _db.SaveChangesAsync();

		var result = await _model.OnPostDelete(reason.Id);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var redirect = (RedirectToPageResult)result;
		Assert.AreEqual("Index", redirect.PageName);

		var deletedReason = await _db.SubmissionRejectionReasons.FindAsync(reason.Id);
		Assert.IsNull(deletedReason);
	}

	[TestMethod]
	public async Task Initialize_PopulatesReasonsCorrectly()
	{
		var reason1 = _db.AddRejectionReason("Reason 1").Entity;
		var reason2 = _db.AddRejectionReason("Reason 2").Entity;
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
}
