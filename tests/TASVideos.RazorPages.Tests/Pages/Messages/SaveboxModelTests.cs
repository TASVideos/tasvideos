using TASVideos.Core.Services;
using TASVideos.Pages.Messages;

namespace TASVideos.RazorPages.Tests.Pages.Messages;

[TestClass]
public class SaveboxModelTests : BasePageModelTests
{
	private readonly IPrivateMessageService _privateMessageService = Substitute.For<IPrivateMessageService>();
	private readonly SaveboxModel _model;

	public SaveboxModelTests()
	{
		_model = new SaveboxModel(_privateMessageService);
	}

	[TestMethod]
	public async Task OnGet_NoSavedMessages_ReturnsEmptyCollection()
	{
		_privateMessageService.GetSavebox(Arg.Any<int>()).Returns([]);
		await _model.OnGet();

		Assert.AreEqual(0, _model.SaveBox.Count);
	}

	[TestMethod]
	public async Task OnGet_WithSavedMessages_ReturnsMessagesCollection()
	{
		const int userId = 123;
		var user = _db.AddUser(userId, "TestUser").Entity;
		await _db.SaveChangesAsync();
		AddAuthenticatedUser(_model, user, []);
		_privateMessageService.GetSavebox(userId).Returns(
		[
			new(1, "First Saved Message", "Sender1", "Recipient1", DateTime.UtcNow.AddDays(-1)),
			new(2, "Second Saved Message", "Sender2", "Recipient2", DateTime.UtcNow.AddDays(-2))
		]);

		await _model.OnGet();

		Assert.AreEqual(2, _model.SaveBox.Count);
		Assert.IsTrue(_model.SaveBox.Any(m => m.Subject == "First Saved Message"));
		Assert.IsTrue(_model.SaveBox.Any(m => m.Subject == "Second Saved Message"));
	}

	[TestMethod]
	public void HasAuthorizeAttribute() => AssertRequiresAuthorization(typeof(SaveboxModel));
}
