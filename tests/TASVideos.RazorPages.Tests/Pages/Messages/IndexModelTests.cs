using TASVideos.Core.Services;
using TASVideos.Pages.Messages;

namespace TASVideos.RazorPages.Tests.Pages.Messages;

[TestClass]
public class IndexModelTests : BasePageModelTests
{
	private readonly IPrivateMessageService _privateMessageService = Substitute.For<IPrivateMessageService>();

	private readonly IndexModel _model;

	public IndexModelTests()
	{
		_model = new IndexModel(_privateMessageService);
	}

	[TestMethod]
	public async Task OnGet_MessageNotFound_ReturnsNotFound()
	{
		_privateMessageService.GetMessage(Arg.Any<int>(), Arg.Any<int>()).Returns((Message?)null);
		var result = await _model.OnGet();
		Assert.IsInstanceOfType(result, typeof(NotFoundResult));
	}

	[TestMethod]
	public async Task OnGet_MessageFound_ReturnsPageWithMessage()
	{
		const int userId = 123;
		const int messageId = 456;
		var user = _db.AddUser(userId, "TestUser").Entity;
		await _db.SaveChangesAsync();

		var expectedMessage = new Message(
			"Test Subject",
			DateTime.UtcNow,
			"Test message content",
			1,
			"Sender",
			userId,
			"TestUser",
			true,
			false,
			false);

		_privateMessageService.GetMessage(userId, messageId).Returns(expectedMessage);

		_model.Id = messageId;
		AddAuthenticatedUser(_model, user, []);

		var result = await _model.OnGet();

		Assert.IsInstanceOfType(result, typeof(PageResult));
		Assert.AreEqual("Test Subject", _model.PrivateMessage.Subject);
		Assert.AreEqual("Test message content", _model.PrivateMessage.Text);
	}

	[TestMethod]
	public void HasAuthorizeAttribute() => AssertRequiresAuthorization(typeof(IndexModel));
}
