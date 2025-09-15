using TASVideos.Core.Services;
using TASVideos.Pages.Messages;

namespace TASVideos.RazorPages.Tests.Pages.Messages;

[TestClass]
public class CreateModelTests : BasePageModelTests
{
	private readonly IPrivateMessageService _privateMessageService = Substitute.For<IPrivateMessageService>();
	private readonly IUserManager _userManager = Substitute.For<IUserManager>();
	private readonly CreateModel _model;

	public CreateModelTests()
	{
		_model = new CreateModel(_userManager, _privateMessageService);
	}

	[TestMethod]
	public async Task OnGet_NoReply_SetsDefaultValues()
	{
		_privateMessageService.AllowedRoles().Returns(["admin", "moderator"]);
		_model.DefaultToUser = "TestUser";

		await _model.OnGet();

		Assert.AreEqual("TestUser", _model.ToUser);
		Assert.IsFalse(_model.IsReply);
		Assert.IsNull(_model.ReplyingTo);
		Assert.AreEqual(3, _model.AvailableGroupRoles.Count); // 2 roles + default entry
	}

	[TestMethod]
	public async Task OnGet_WithReply_SetsReplyValues()
	{
		const int userId = 123;
		const int replyToId = 456;
		var user = _db.AddUser(userId, "TestUser").Entity;
		await _db.SaveChangesAsync();

		var replyMessage = new Message(
			"Original Subject",
			DateTime.UtcNow.AddDays(-1),
			"Original message text",
			999,
			"OriginalSender",
			userId,
			"TestUser",
			true,
			false,
			false);

		_privateMessageService.GetMessage(userId, replyToId).Returns(replyMessage);
		_privateMessageService.AllowedRoles().Returns(["admin"]);

		_model.ReplyTo = replyToId;
		AddAuthenticatedUser(_model, user, [PermissionTo.SendPrivateMessages]);

		await _model.OnGet();

		Assert.IsTrue(_model.IsReply);
		Assert.IsNotNull(_model.ReplyingTo);
		Assert.AreEqual("Re: Original Subject", _model.Subject);
		Assert.AreEqual("OriginalSender", _model.ReplyingTo.FromUserName);
		Assert.AreEqual("Original message text", _model.ReplyingTo.Text);
	}

	[TestMethod]
	public async Task OnGet_ReplyToNonExistentMessage_DoesNotSetReply()
	{
		const int userId = 123;
		var user = _db.AddUser(userId, "TestUser").Entity;
		await _db.SaveChangesAsync();

		_privateMessageService.GetMessage(userId, 999).Returns((Message?)null);
		_privateMessageService.AllowedRoles().Returns([]);

		_model.ReplyTo = 999;
		AddAuthenticatedUser(_model, user, [PermissionTo.SendPrivateMessages]);

		await _model.OnGet();

		Assert.IsFalse(_model.IsReply);
		Assert.IsNull(_model.ReplyingTo);
		Assert.AreEqual("", _model.Subject);
	}

	[TestMethod]
	public async Task OnPost_SendToSelf_ReturnsPageWithModelError()
	{
		const int userId = 123;
		var user = _db.AddUser(userId, "TestUser").Entity;
		await _db.SaveChangesAsync();

		_model.Subject = "Test Subject";
		_model.MessageBody = "Test message body";
		_model.ToUser = "TestUser";
		AddAuthenticatedUser(_model, user, [PermissionTo.SendPrivateMessages]);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
		Assert.IsTrue(_model.ModelState.ContainsKey(nameof(_model.ToUser)));
	}

	[TestMethod]
	public async Task OnPost_InvalidModelState_ReturnsPage()
	{
		_model.ToUser = "AnotherUser";
		_model.ModelState.AddModelError("", "Some Error");

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsFalse(_model.ModelState.IsValid);
	}

	[TestMethod]
	public async Task OnPost_SendToRole_CallsServiceAndRedirects()
	{
		const int userId = 123;
		var user = _db.AddUser(userId, "TestUser").Entity;
		await _db.SaveChangesAsync();

		_privateMessageService.AllowedRoles().Returns(["admin", "moderator"]);

		_model.Subject = "Test Subject";
		_model.MessageBody = "Test message body";
		_model.ToUser = "admin";
		AddAuthenticatedUser(_model, user, [PermissionTo.SendPrivateMessages]);

		var result = await _model.OnPost();

		await _privateMessageService.Received(1).SendMessageToRole(userId, "admin", "Test Subject", "Test message body");
		AssertRedirect(result, "Inbox");
	}

	[TestMethod]
	public async Task OnPost_SendToValidUser_CallsServiceAndRedirects()
	{
		const int userId = 123;
		var user = _db.AddUser(userId, "TestUser").Entity;
		await _db.SaveChangesAsync();

		_privateMessageService.AllowedRoles().Returns(["admin"]);
		_userManager.Exists("RecipientUser").Returns(true);

		_model.Subject = "Test Subject";
		_model.MessageBody = "Test message body";
		_model.ToUser = "RecipientUser";
		AddAuthenticatedUser(_model, user, [PermissionTo.SendPrivateMessages]);

		var result = await _model.OnPost();

		await _privateMessageService.Received(1).SendMessage(userId, "RecipientUser", "Test Subject", "Test message body");
		AssertRedirect(result, "Inbox");
	}

	[TestMethod]
	public async Task OnPost_SendToNonExistentUser_ReturnsPageWithModelError()
	{
		const int userId = 123;
		var user = _db.AddUser(userId, "TestUser").Entity;
		await _db.SaveChangesAsync();

		_privateMessageService.AllowedRoles().Returns([]);
		_userManager.Exists("NonExistentUser").Returns(false);

		_model.Subject = "Test Subject";
		_model.MessageBody = "Test message body";
		_model.ToUser = "NonExistentUser";
		AddAuthenticatedUser(_model, user, [PermissionTo.SendPrivateMessages]);

		var result = await _model.OnPost();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.IsTrue(_model.ModelState.ContainsKey(nameof(_model.ToUser)));
		await _privateMessageService.DidNotReceive().SendMessage(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
	}

	[TestMethod]
	public void RequiresPermission() => AssertHasPermission(typeof(CreateModel), PermissionTo.SendPrivateMessages);
}
