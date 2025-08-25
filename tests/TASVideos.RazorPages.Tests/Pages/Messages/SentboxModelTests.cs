using TASVideos.Core;
using TASVideos.Core.Services;
using TASVideos.Pages.Messages;

namespace TASVideos.RazorPages.Tests.Pages.Messages;

[TestClass]
public class SentboxModelTests : BasePageModelTests
{
	private readonly IPrivateMessageService _privateMessageService = Substitute.For<IPrivateMessageService>();
	private readonly SentboxModel _model;

	public SentboxModelTests()
	{
		_model = new SentboxModel(_privateMessageService);
	}

	[TestMethod]
	public async Task OnGet_NoSavedMessages_ReturnsEmptyCollection()
	{
		_privateMessageService.GetSentInbox(Arg.Any<int>(), Arg.Any<PagingModel>()).Returns(new PageOf<SentboxEntry>([], new()));
		await _model.OnGet();
		Assert.AreEqual(0, _model.SentBox.Count());
	}

	[TestMethod]
	public async Task OnGet_WithSavedMessages_ReturnsMessagesCollection()
	{
		const int userId = 123;
		var user = _db.AddUser(userId, "TestUser").Entity;
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_model, user, []);

		var sentBoxItems = new List<SentboxEntry>
		{
			new(1, "First Sent Message", "Receiver1", DateTime.UtcNow.AddDays(-1), true),
			new(2, "Second Sent Message", "Receiver2", DateTime.UtcNow.AddDays(-2), true)
		}.AsQueryable();
		_privateMessageService.GetSentInbox(userId, Arg.Any<PagingModel>()).Returns(new PageOf<SentboxEntry>(sentBoxItems, new()));

		await _model.OnGet();

		Assert.AreEqual(2, _model.SentBox.Count());
		Assert.IsTrue(_model.SentBox.Any(m => m.Subject == "First Sent Message"));
		Assert.IsTrue(_model.SentBox.Any(m => m.Subject == "Second Sent Message"));
	}

	[TestMethod]
	public void HasAuthorizeAttribute() => AssertRequiresAuthorization(typeof(SentboxModel));
}
