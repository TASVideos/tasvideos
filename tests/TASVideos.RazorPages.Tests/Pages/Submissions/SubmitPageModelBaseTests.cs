using TASVideos.Pages.Submissions;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Submissions;

[TestClass]
public class SubmitPageModelBaseTests : TestDbBase
{
	private readonly TestSubmitPageModel _page = new();

	[TestMethod]
	public void CanEditSubmission_UserHasEditPermission_ReturnsTrue()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.EditSubmissions]);

		var result = _page.CanEditSubmission("OtherUser", ["AnotherUser"]);

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void CanEditSubmission_UserIsOriginalSubmitter_ReturnsTrue()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.SubmitMovies]);

		var result = _page.CanEditSubmission("TestUser", ["OtherUser"]);

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void CanEditSubmission_UserIsAuthor_ReturnsTrue()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.SubmitMovies]);

		var result = _page.CanEditSubmission("OtherUser", ["TestUser", "AnotherUser"]);

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void CanEditSubmission_UserNotAuthorOrSubmitter_ReturnsFalse()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.SubmitMovies]);

		var result = _page.CanEditSubmission("OtherUser", ["AnotherUser", "ThirdUser"]);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void CanEditSubmission_UserIsSubmitterButHasNoSubmitPermission_ReturnsFalse()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, []); // No permissions

		var result = _page.CanEditSubmission("TestUser", ["TestUser"]);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void CanEditSubmission_AnonymousUser_ReturnsFalse()
	{
		var result = _page.CanEditSubmission("TestUser", ["TestUser"]);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void CanEditSubmission_WithEditAndSubmitPermissions_ReturnsTrue()
	{
		var user = _db.AddUser("TestUser").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.EditSubmissions, PermissionTo.SubmitMovies]);

		var result = _page.CanEditSubmission("OtherUser", ["AnotherUser"]);

		Assert.IsTrue(result);
	}

	// Test helper class to access protected methods
	private class TestSubmitPageModel
		: SubmitPageModelBase
	{
		public new bool CanEditSubmission(string? submitter, ICollection<string> authors)
			=> base.CanEditSubmission(submitter, authors);
	}
}
