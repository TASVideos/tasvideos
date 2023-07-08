using TASVideos.Core.Services;
using TASVideos.Core.Services.Email;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class PrivateMessageServiceTests
{
	private readonly TestDbContext _db;
	private readonly Mock<IEmailService> _emailService;
	private readonly PrivateMessageService _privateMessageService;

	public PrivateMessageServiceTests()
	{
		_db = TestDbContext.Create();
		_emailService = new Mock<IEmailService>();
		_privateMessageService = new PrivateMessageService(_db, _emailService.Object);
	}

	[TestMethod]
	public async Task Send_ToUserNotFound_DoesNotSend()
	{
		const string toUser = "DoesNotExist";
		await _privateMessageService.SendMessage(1, toUser, "", "");
		Assert.AreEqual(_db.PrivateMessages.Count(), 0);
	}

	[TestMethod]
	public async Task Send_Success_EmailsAllowed()
	{
		const int fromUserId = 1;
		const int toUserId = 2;
		const string toUserName = "ToUser";
		const bool toUserEmailOnPm = true;
		const string toUserEmail = "a@example.com";
		const string subject = "Subject";
		const string text = "Text";
		_db.Users.Add(new User
		{
			Id = toUserId,
			UserName = toUserName,
			EmailOnPrivateMessage = toUserEmailOnPm,
			Email = toUserEmail
		});
		_db.Users.Add(new User { Id = fromUserId, UserName = "_" });
		await _db.SaveChangesAsync();

		await _privateMessageService.SendMessage(fromUserId, toUserName, subject, text);

		Assert.AreEqual(_db.PrivateMessages.Count(), 1);
		var pm = _db.PrivateMessages.Single();
		Assert.AreEqual(fromUserId, pm.FromUserId);
		Assert.AreEqual(toUserId, pm.ToUserId);
		Assert.AreEqual(subject, pm.Subject);
		Assert.AreEqual(text, pm.Text);
		_emailService.Verify(m => m.NewPrivateMessage(toUserEmail, toUserName), Times.Once);
	}

	[TestMethod]
	public async Task Send_Success_EmailsNotAllowed()
	{
		const int fromUserId = 1;
		const int toUserId = 2;
		const string toUserName = "ToUser";
		const bool toUserEmailOnPm = false;
		const string toUserEmail = "a@example.com";
		const string subject = "Subject";
		const string text = "Text";
		_db.Users.Add(new User
		{
			Id = toUserId,
			UserName = toUserName,
			EmailOnPrivateMessage = toUserEmailOnPm,
			Email = toUserEmail
		});
		_db.Users.Add(new User { Id = fromUserId, UserName = "_" });
		await _db.SaveChangesAsync();

		await _privateMessageService.SendMessage(fromUserId, toUserName, subject, text);

		Assert.AreEqual(_db.PrivateMessages.Count(), 1);
		var pm = _db.PrivateMessages.Single();
		Assert.AreEqual(fromUserId, pm.FromUserId);
		Assert.AreEqual(toUserId, pm.ToUserId);
		Assert.AreEqual(subject, pm.Subject);
		Assert.AreEqual(text, pm.Text);
		_emailService.Verify(m => m.NewPrivateMessage(toUserEmail, toUserName), Times.Never);
	}
}
