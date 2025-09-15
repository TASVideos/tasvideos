using TASVideos.Core;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Pages.Messages;

namespace TASVideos.RazorPages.Tests.Pages.Messages;

[TestClass]
public class InboxModelTests : BasePageModelTests
{
	private readonly IPrivateMessageService _privateMessageService = Substitute.For<IPrivateMessageService>();
	private readonly InboxModel _model;

	public InboxModelTests()
	{
		_model = new InboxModel(_privateMessageService);
	}

	[TestMethod]
	public async Task OnGet_ReturnsMessagesFromService()
	{
		var expectedMessages = new PageOf<InboxEntry>(
		[
			new InboxEntry(1, "Test Subject 1", "Sender1", DateTime.UtcNow, false),
			new InboxEntry(2, "Test Subject 2", "Sender2", DateTime.UtcNow.AddDays(-1), true)
		],
		new PagingModel());

		_privateMessageService.GetInbox(Arg.Any<int>(), Arg.Any<PagingModel>()).Returns(expectedMessages);

		await _model.OnGet();

		Assert.AreEqual(2, _model.Messages.Count());
		Assert.IsTrue(_model.Messages.Any(m => m.Subject == "Test Subject 1"));
		Assert.IsTrue(_model.Messages.Any(m => m.Subject == "Test Subject 2"));
	}

	[TestMethod]
	public async Task OnPostSave_NullId_ReturnsNotFound()
	{
		_model.Id = null;
		var result = await _model.OnPostSave();
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPostSave_IdHasValue_CallsServiceAndRedirects()
	{
		const int userId = 123;
		const int messageId = 456;
		var user = _db.AddUser(userId, "TestUser").Entity;
		await _db.SaveChangesAsync();
		_privateMessageService.SaveMessage(userId, messageId).Returns(SaveResult.Success);
		AddAuthenticatedUser(_model, user, [PermissionTo.Unpublish]);
		_model.Id = messageId;

		var result = await _model.OnPostSave();

		await _privateMessageService.Received(1).SaveMessage(userId, messageId);
		AssertRedirect(result, "Savebox");
		Assert.AreEqual("success", _model.MessageType);
	}

	[TestMethod]
	public async Task OnPostSave_SaveFails_SetsErrorMessageAndRedirects()
	{
		_privateMessageService.SaveMessage(Arg.Any<int>(), Arg.Any<int>()).Returns(SaveResult.UpdateFailure);
		_model.Id = 456;

		var result = await _model.OnPostSave();

		AssertRedirect(result, "Savebox");
		Assert.AreEqual("danger", _model.MessageType);
	}

	[TestMethod]
	public async Task OnPostDelete_NullId_ReturnsNotFound()
	{
		_model.Id = null;
		var result = await _model.OnPostDelete();
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnPostDelete_IdHasValue_CallsServiceAndRedirects()
	{
		const int userId = 123;
		const int messageId = 456;
		var user = _db.AddUser(userId, "TestUser").Entity;
		await _db.SaveChangesAsync();

		_privateMessageService.DeleteMessage(userId, messageId).Returns(SaveResult.Success);

		_model.Id = messageId;
		AddAuthenticatedUser(_model, user, []);

		var result = await _model.OnPostDelete();

		await _privateMessageService.Received(1).DeleteMessage(userId, messageId);
		AssertRedirect(result, "Inbox");
		Assert.AreEqual("success", _model.MessageType);
	}

	[TestMethod]
	public async Task OnPostDelete_DeleteFails_SetsErrorMessageAndRedirects()
	{
		_privateMessageService.DeleteMessage(Arg.Any<int>(), Arg.Any<int>()).Returns(SaveResult.UpdateFailure);
		_model.Id = 456;

		var result = await _model.OnPostDelete();

		AssertRedirect(result, "Inbox");
		Assert.AreEqual("danger", _model.MessageType);
	}
}
