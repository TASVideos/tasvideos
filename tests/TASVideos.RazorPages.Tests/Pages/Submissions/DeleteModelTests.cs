using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Pages.Submissions;
using TASVideos.Services;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Submissions;

[TestClass]
public class DeleteModelTests : TestDbBase
{
	private readonly IQueueService _queueService;
	private readonly IExternalMediaPublisher _publisher;
	private readonly DeleteModel _page;

	public DeleteModelTests()
	{
		_queueService = Substitute.For<IQueueService>();
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_page = new DeleteModel(_queueService, _publisher);
	}

	[TestMethod]
	public async Task OnGet_SubmissionNotFound_ReturnsNotFound()
	{
		_page.Id = 999;
		_queueService.CanDeleteSubmission(999).Returns(new DeleteSubmissionResult(
			DeleteSubmissionResult.DeleteStatus.NotFound,
			"",
			""));

		var actual = await _page.OnGet();

		Assert.IsInstanceOfType<NotFoundResult>(actual);
	}

	[TestMethod]
	public async Task OnGet_SubmissionNotAllowed_ReturnsBadRequest()
	{
		_page.Id = 1;
		_queueService.CanDeleteSubmission(1).Returns(new DeleteSubmissionResult(
			DeleteSubmissionResult.DeleteStatus.NotAllowed,
			"Test Submission",
			"Cannot delete published submission"));

		var actual = await _page.OnGet();

		Assert.IsInstanceOfType<BadRequestObjectResult>(actual);
		var badRequest = (BadRequestObjectResult)actual;
		Assert.AreEqual("Cannot delete published submission", badRequest.Value);
	}

	[TestMethod]
	public async Task OnGet_SubmissionCanBeDeleted_PopulatesTitleAndReturnsPage()
	{
		_page.Id = 1;
		_queueService.CanDeleteSubmission(1).Returns(new DeleteSubmissionResult(
			DeleteSubmissionResult.DeleteStatus.Success,
			"Test Submission",
			""));

		var actual = await _page.OnGet();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.AreEqual("Test Submission", _page.Title);
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_page.ModelState.AddModelError("Error", "Test error");

		var actual = await _page.OnPost();

		Assert.IsInstanceOfType<PageResult>(actual);
		Assert.IsFalse(_page.ModelState.IsValid);
	}

	[TestMethod]
	public async Task OnPost_SubmissionNotFound_RedirectsToViewWithError()
	{
		_page.Id = 999;
		_queueService.DeleteSubmission(999).Returns(new DeleteSubmissionResult(
			DeleteSubmissionResult.DeleteStatus.NotFound,
			"",
			""));

		var actual = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(actual);
		var redirect = (RedirectToPageResult)actual;
		Assert.AreEqual("View", redirect.PageName);
		Assert.AreEqual(999, redirect.RouteValues!["Id"]);
		await _publisher.DidNotReceive().Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_SubmissionNotAllowed_RedirectsToViewWithError()
	{
		_page.Id = 1;
		_queueService.DeleteSubmission(1).Returns(new DeleteSubmissionResult(
			DeleteSubmissionResult.DeleteStatus.NotAllowed,
			"Test Submission",
			"Cannot delete published submission"));

		var actual = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectToPageResult>(actual);
		var redirect = (RedirectToPageResult)actual;
		Assert.AreEqual("View", redirect.PageName);
		Assert.AreEqual(1, redirect.RouteValues!["Id"]);
		await _publisher.DidNotReceive().Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPost_SuccessfulDeletion_AnnouncesAndRedirectsToSubmissionsList()
	{
		_page.Id = 1;
		_queueService.DeleteSubmission(1).Returns(new DeleteSubmissionResult(
			DeleteSubmissionResult.DeleteStatus.Success,
			"Test Submission Title",
			""));

		var actual = await _page.OnPost();

		Assert.IsInstanceOfType<RedirectResult>(actual);
		var redirect = (RedirectResult)actual;
		Assert.AreEqual("/Subs-List", redirect.Url);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(DeleteModel), PermissionTo.DeleteSubmissions);
}
