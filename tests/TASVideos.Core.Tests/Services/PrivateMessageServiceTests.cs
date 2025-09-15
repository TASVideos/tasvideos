using TASVideos.Core.Services.Email;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class PrivateMessageServiceTests : TestDbBase
{
	private readonly TestCache _cache;
	private readonly IEmailService _emailService;
	private readonly PrivateMessageService _privateMessageService;

	public PrivateMessageServiceTests()
	{
		_cache = new TestCache();
		_emailService = Substitute.For<IEmailService>();
		_privateMessageService = new PrivateMessageService(_db, _cache, _emailService);
	}

	#region GetMessage()

	[TestMethod]
	public async Task GetMessage_IdDoesNotExist_ReturnsNull()
	{
		const int userId = 1;
		_db.AddUser(userId, "Test");
		var actual = await _privateMessageService.GetMessage(userId, -1);
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task GetMessage_IdExists_ButDoesNotBelongToUser_ReturnsNull()
	{
		const int sentFromId = 1;
		_db.AddUser(sentFromId, "FromUser");
		const int sentToId = 2;
		_db.AddUser(sentToId, "ToUser");
		const int messageId = 1;
		const int anotherUser = 3;
		_db.AddUser(anotherUser, "AnotherUser");
		_db.PrivateMessages.Add(new PrivateMessage
		{
			Id = messageId,
			FromUserId = sentFromId,
			ToUserId = sentToId
		});
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.GetMessage(anotherUser, messageId);
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task GetMessage_SentFromUser_ReturnsMessage()
	{
		const int sentFromId = 1;
		_db.AddUser(sentFromId, "FromUser");
		const int sentToId = 2;
		_db.AddUser(sentToId, "ToUser");

		const int messageId = 1;
		const string subject = "Subject";
		const string text = "Text";
		var sentTimestamp = DateTime.UtcNow.AddDays(-1);

		_db.PrivateMessages.Add(new PrivateMessage
		{
			Id = messageId,
			CreateTimestamp = sentTimestamp,
			FromUserId = sentFromId,
			ToUserId = sentToId,
			Subject = subject,
			Text = text
		});
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.GetMessage(sentFromId, messageId);
		Assert.IsNotNull(actual);
		Assert.AreEqual(sentFromId, actual.FromUserId);
		Assert.AreEqual(sentToId, actual.ToUserId);
		Assert.AreEqual(subject, actual.Subject);
		Assert.AreEqual(text, actual.Text);
		Assert.IsFalse(actual.CanReply, "Should not be able to reply to a message their own message");
		Assert.AreEqual(sentTimestamp, actual.SentOn);
	}

	[TestMethod]
	public async Task GetMessage_FromUserButDeletedForFromUser_ReturnsNull()
	{
		const int sentFromId = 1;
		_db.AddUser(sentFromId, "FromUser");
		const int sentToId = 2;
		_db.AddUser(sentToId, "ToUser");
		const int messageId = 1;

		_db.PrivateMessages.Add(new PrivateMessage
		{
			Id = messageId,
			FromUserId = sentFromId,
			ToUserId = sentToId,
			DeletedForFromUser = true
		});
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.GetMessage(sentFromId, messageId);
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task GetMessage_ToUserButDeletedForFromUser_ReturnsMessage()
	{
		const int sentFromId = 1;
		_db.AddUser(sentFromId, "FromUser");
		const int sentToId = 2;
		_db.AddUser(sentToId, "ToUser");
		const int messageId = 1;

		_db.PrivateMessages.Add(new PrivateMessage
		{
			Id = messageId,
			FromUserId = sentFromId,
			ToUserId = sentToId,
			DeletedForFromUser = true
		});
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.GetMessage(sentToId, messageId);
		Assert.IsNotNull(actual);
		Assert.AreEqual(sentToId, actual.ToUserId);
		Assert.AreEqual(sentFromId, actual.FromUserId);
	}

	[TestMethod]
	public async Task GetMessage_FromUserButDeletedForToUser_ReturnsMessage()
	{
		const int sentFromId = 1;
		_db.AddUser(sentFromId, "FromUser");
		const int sentToId = 2;
		_db.AddUser(sentToId, "ToUser");
		const int messageId = 1;

		_db.PrivateMessages.Add(new PrivateMessage
		{
			Id = messageId,
			FromUserId = sentFromId,
			ToUserId = sentToId,
			DeletedForToUser = true
		});
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.GetMessage(messageId, sentFromId);
		Assert.IsNotNull(actual);
		Assert.AreEqual(sentToId, actual.ToUserId);
		Assert.AreEqual(sentFromId, actual.FromUserId);
	}

	[TestMethod]
	public async Task GetMessage_ToUserButDeletedForToUser_ReturnsNull()
	{
		const int sentFromId = 1;
		_db.AddUser(sentFromId, "FromUser");
		const int sentToId = 2;
		_db.AddUser(sentToId, "ToUser");
		const int messageId = 1;

		_db.PrivateMessages.Add(new PrivateMessage
		{
			Id = messageId,
			FromUserId = sentFromId,
			ToUserId = sentToId,
			DeletedForToUser = true
		});
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.GetMessage(messageId, sentToId);
		Assert.IsNull(actual);
	}

	[TestMethod]
	public async Task GetMessage_ToUser_ReturnsMessage_AndMarksMessageAsRead()
	{
		const int sentFromId = 1;
		_db.AddUser(sentFromId, "FromUser");
		const int sentToId = 2;
		_db.AddUser(sentToId, "ToUser");

		const int messageId = 1;
		const string subject = "Subject";
		const string text = "Text";
		var sentTimestamp = DateTime.UtcNow.AddDays(-1);

		_db.PrivateMessages.Add(new PrivateMessage
		{
			Id = messageId,
			CreateTimestamp = sentTimestamp,
			FromUserId = sentFromId,
			ToUserId = sentToId,
			Subject = subject,
			Text = text,
			ReadOn = null
		});
		await _db.SaveChangesAsync();

		string cacheKey = PrivateMessageService.UnreadMessageCount + sentToId;
		_cache.Set(cacheKey, 1);

		var actual = await _privateMessageService.GetMessage(sentToId, messageId);

		Assert.IsNotNull(actual);
		Assert.AreEqual(0, _cache.Count(), "Unread message count cache must be cleared");
		var dbMessage = _db.PrivateMessages.SingleOrDefault(pm => pm.Id == messageId);
		Assert.IsNotNull(dbMessage);
		Assert.IsNotNull(dbMessage.ReadOn, "Message must be marked as read");
		Assert.AreEqual(sentFromId, actual.FromUserId);
		Assert.AreEqual(sentToId, actual.ToUserId);
		Assert.AreEqual(subject, actual.Subject);
		Assert.AreEqual(text, actual.Text);
		Assert.IsTrue(actual.CanReply, "Should not be able to reply to a message they received");
		Assert.AreEqual(sentTimestamp, actual.SentOn);
	}

	#endregion

	#region GetInbox()

	[TestMethod]
	public async Task Inbox_DoesNotIncludeMessagesTheUserSent()
	{
		const int inboxUser = 1;
		_db.AddUser(inboxUser, "UserWithInbox");
		const int otherUser = 2;
		_db.AddUser(otherUser, "OtherUser");
		const string expectedSubject = "Messages returned should be this subject";
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = inboxUser, ToUserId = otherUser });
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = otherUser, ToUserId = inboxUser, Subject = expectedSubject });
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.GetInbox(inboxUser, new PagingModel());
		Assert.IsNotNull(actual);
		Assert.AreEqual(1, actual.RowCount);
		Assert.IsTrue(actual.All(m => m.Subject == expectedSubject));
	}

	[TestMethod]
	public async Task Inbox_DoesNotIncludeDeletedMessages()
	{
		const int inboxUser = 1;
		_db.AddUser(inboxUser, "UserWithInbox");
		const int otherUser = 2;
		_db.AddUser(otherUser, "OtherUser");
		const string expectedSubject = "Messages returned should be this subject";
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = otherUser, ToUserId = inboxUser, Subject = expectedSubject });
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = otherUser, ToUserId = inboxUser, DeletedForFromUser = true, Subject = expectedSubject });
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = otherUser, ToUserId = inboxUser, DeletedForToUser = true });
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.GetInbox(inboxUser, new PagingModel());
		Assert.IsNotNull(actual);
		Assert.AreEqual(2, actual.RowCount);
		Assert.IsTrue(actual.All(m => m.Subject == expectedSubject));
	}

	[TestMethod]
	public async Task Inbox_DoesNotIncludeSavedMessages()
	{
		const int inboxUser = 1;
		_db.AddUser(inboxUser, "UserWithInbox");
		const int otherUser = 2;
		_db.AddUser(otherUser, "OtherUser");
		const string expectedSubject = "Messages returned should be this subject";
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = otherUser, ToUserId = inboxUser, Subject = expectedSubject });
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = otherUser, ToUserId = inboxUser, SavedForFromUser = true, Subject = expectedSubject });
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = otherUser, ToUserId = inboxUser, SavedForToUser = true });
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.GetInbox(inboxUser, new PagingModel());
		Assert.IsNotNull(actual);
		Assert.AreEqual(2, actual.RowCount);
		Assert.IsTrue(actual.All(m => m.Subject == expectedSubject));
	}

	#endregion

	#region GetSentBox()

	[TestMethod]
	public async Task Sentbox_DoesNotIncludeMessagesTheUserReceived()
	{
		const int sentBoxUser = 1;
		_db.AddUser(sentBoxUser, "UserWithSentBox");
		const int otherUser = 2;
		_db.AddUser(otherUser, "OtherUser");
		const string expectedSubject = "Messages returned should be this subject";
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = sentBoxUser, ToUserId = otherUser, Subject = expectedSubject });
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = otherUser, ToUserId = sentBoxUser });
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.GetSentInbox(sentBoxUser, new PagingModel());
		Assert.IsNotNull(actual);
		Assert.AreEqual(1, actual.RowCount);
		Assert.IsTrue(actual.All(m => m.Subject == expectedSubject));
	}

	[TestMethod]
	public async Task Sentbox_DoesNotIncludeDeletedMessages()
	{
		const int sentBoxUser = 1;
		_db.AddUser(sentBoxUser, "UserWithSentBox");
		const int otherUser = 2;
		_db.AddUser(otherUser, "OtherUser");
		const string expectedSubject = "Messages returned should be this subject";
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = otherUser, ToUserId = sentBoxUser });
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = sentBoxUser, ToUserId = sentBoxUser, Subject = expectedSubject });
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = sentBoxUser, ToUserId = otherUser, DeletedForFromUser = true, Subject = expectedSubject });
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = sentBoxUser, ToUserId = otherUser, DeletedForToUser = true });
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.GetSentInbox(sentBoxUser, new PagingModel());
		Assert.IsNotNull(actual);
		Assert.AreEqual(2, actual.RowCount);
		Assert.IsTrue(actual.All(m => m.Subject == expectedSubject));
	}

	#endregion

	#region GetSaveBox()

	[TestMethod]
	public async Task Savebox_IncludesSentAndReceived()
	{
		const int saveBoxUser = 1;
		_db.AddUser(saveBoxUser, "UserWithSentBox");
		const int otherUser = 2;
		_db.AddUser(otherUser, "OtherUser");
		const string expectedSubject = "Messages returned should be this subject";
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = otherUser, ToUserId = saveBoxUser });
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = otherUser, ToUserId = otherUser, SavedForFromUser = true, DeletedForToUser = true });
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = otherUser, ToUserId = saveBoxUser, SavedForFromUser = true });
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = otherUser, ToUserId = saveBoxUser, SavedForToUser = true, Subject = expectedSubject });
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = saveBoxUser, ToUserId = otherUser, SavedForFromUser = true, Subject = expectedSubject });
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = saveBoxUser, ToUserId = otherUser, SavedForToUser = true });
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.GetSavebox(saveBoxUser);
		Assert.IsNotNull(actual);
		Assert.AreEqual(2, actual.Count);
		Assert.IsTrue(actual.All(m => m.Subject == expectedSubject));
	}

	#endregion

	#region GetUnreadMessageCount()

	[TestMethod]
	public async Task UnreadMessageCount_Cached_ReturnsFromCache()
	{
		const int sentToId = 2;
		_db.AddUser(sentToId, "ToUser");
		const int messageCount = 5;
		_cache.Set(PrivateMessageService.UnreadMessageCount + sentToId, messageCount);

		var actual = await _privateMessageService.GetUnreadMessageCount(sentToId);
		Assert.AreEqual(messageCount, actual);
	}

	[TestMethod]
	public async Task UnreadMessageCount_NotCached_CachesAndReturns()
	{
		const int currentUserId = 1;
		_db.AddUser(currentUserId, "CurrentUser");
		const int otherUserId = 2;
		_db.AddUser(otherUserId, "OtherUser");
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = currentUserId, ToUserId = otherUserId });
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = otherUserId, ToUserId = currentUserId });
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = otherUserId, ToUserId = otherUserId });
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.GetUnreadMessageCount(currentUserId);
		Assert.AreEqual(1, actual);
	}

	[TestMethod]
	public async Task UnreadMessageCount_DoesNotIncludeSentOrReadOrDeletedMessages()
	{
		const int currentUserId = 1;
		_db.AddUser(currentUserId, "CurrentUser");
		const int otherUserId = 2;
		_db.AddUser(otherUserId, "OtherUser");

		// Not sent to user
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = currentUserId, ToUserId = otherUserId });
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = otherUserId, ToUserId = otherUserId });

		// Sent to User
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = otherUserId, ToUserId = currentUserId }); // Expected
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = otherUserId, ToUserId = currentUserId, DeletedForFromUser = true }); // Expected
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = otherUserId, ToUserId = currentUserId, DeletedForToUser = true }); // Not Expected, because it is deleted
		_db.PrivateMessages.Add(new PrivateMessage { FromUserId = otherUserId, ToUserId = currentUserId, ReadOn = DateTime.UtcNow }); // Not Expected, because it is read
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.GetUnreadMessageCount(currentUserId);
		Assert.AreEqual(2, actual);
		Assert.IsTrue(_cache.ContainsKey(PrivateMessageService.UnreadMessageCount + currentUserId));
	}

	#endregion

	#region SendMessage()

	[TestMethod]
	public async Task Send_ToUserNotFound_DoesNotSend()
	{
		const string toUser = "DoesNotExist";
		await _privateMessageService.SendMessage(1, toUser, "", "");
		Assert.AreEqual(0, _db.PrivateMessages.Count());
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
			NormalizedEmail = toUserName,
			EmailOnPrivateMessage = toUserEmailOnPm,
			Email = toUserEmail,
			NormalizedUserName = toUserEmail
		});
		_db.AddUser(fromUserId, "_");
		await _db.SaveChangesAsync();

		await _privateMessageService.SendMessage(fromUserId, toUserName, subject, text);

		Assert.AreEqual(1, _db.PrivateMessages.Count());
		var pm = _db.PrivateMessages.Single();
		Assert.AreEqual(fromUserId, pm.FromUserId);
		Assert.AreEqual(toUserId, pm.ToUserId);
		Assert.AreEqual(subject, pm.Subject);
		Assert.AreEqual(text, pm.Text);
		await _emailService.Received(1).NewPrivateMessage(toUserEmail, toUserName);
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
			NormalizedUserName = toUserName,
			EmailOnPrivateMessage = toUserEmailOnPm,
			Email = toUserEmail,
			NormalizedEmail = toUserEmail
		});
		_db.AddUser(fromUserId, "_");
		await _db.SaveChangesAsync();

		await _privateMessageService.SendMessage(fromUserId, toUserName, subject, text);

		Assert.AreEqual(1, _db.PrivateMessages.Count());
		var pm = _db.PrivateMessages.Single();
		Assert.AreEqual(fromUserId, pm.FromUserId);
		Assert.AreEqual(toUserId, pm.ToUserId);
		Assert.AreEqual(subject, pm.Subject);
		Assert.AreEqual(text, pm.Text);
		await _emailService.DidNotReceive().NewPrivateMessage(toUserEmail, toUserName);
	}

	#endregion

	#region SendMessageToRole()

	[TestMethod]
	public async Task SendMessageToRole_RoleNotAllowed_NothingSent()
	{
		const string role = "not an allowed role";

		await _privateMessageService.SendMessageToRole(1, role, "", "");
		Assert.AreEqual(0, _db.PrivateMessages.Count());
	}

	[TestMethod]
	public async Task SendMessageToRole_RoleDoesExist_NothingSent()
	{
		const string role = "moderator";

		await _privateMessageService.SendMessageToRole(1, role, "", "");
		Assert.AreEqual(0, _db.PrivateMessages.Count());
	}

	[TestMethod]
	public async Task SendMessageToRole_Successful_SendsMessageToAllUsersWithRole()
	{
		const int moderatorRoleId = 1;
		const string moderatorRole = "moderator";
		const string subject = "Sending to all moderators";
		const string text = "text that all moderators will see";
		_db.Roles.Add(new Role { Id = moderatorRoleId, Name = moderatorRole });
		const int moderator1Id = 1;
		const int moderator2Id = 2;
		const int sentFromUserId = 3;
		const int unusedUserId = 4;
		_db.AddUser(moderator1Id);
		_db.AddUser(moderator2Id);
		_db.AddUser(sentFromUserId);
		_db.AddUser(unusedUserId);
		_db.UserRoles.Add(new UserRole { RoleId = moderatorRoleId, UserId = moderator1Id });
		_db.UserRoles.Add(new UserRole { RoleId = moderatorRoleId, UserId = moderator2Id });
		await _db.SaveChangesAsync();

		await _privateMessageService.SendMessageToRole(sentFromUserId, moderatorRole, subject, text);

		Assert.AreEqual(1, _db.PrivateMessages.Count(pm => pm.ToUserId == moderator1Id));
		Assert.AreEqual(1, _db.PrivateMessages.Count(pm => pm.ToUserId == moderator2Id));
		Assert.AreEqual(0, _db.PrivateMessages.Count(pm => pm.ToUserId == sentFromUserId));
		Assert.AreEqual(0, _db.PrivateMessages.Count(pm => pm.ToUserId == unusedUserId));
		Assert.AreEqual(2, _db.PrivateMessages.Count(pm => pm.FromUserId == sentFromUserId));
		Assert.IsTrue(_db.PrivateMessages.All(pm => pm.Subject != null && pm.Subject.Contains(subject)));
		Assert.IsTrue(_db.PrivateMessages.All(pm => pm.Text.Contains(text)));
	}

	#endregion

	#region SaveMessageForUser()

	[TestMethod]
	public async Task SaveMessageForUser_MessageDoesNotExist_ReturnsFailure()
	{
		var user = _db.AddUser("Exists");

		var actual = await _privateMessageService.SaveMessage(user.Entity.Id, -1);
		Assert.AreEqual(SaveResult.UpdateFailure, actual);
		Assert.IsFalse(_db.PrivateMessages.Any(pm => pm.SavedForFromUser || pm.SavedForToUser));
	}

	[TestMethod]
	public async Task SaveMessageForUser_MessageNotForUser_ReturnsFailure()
	{
		const int fromUserId = 1;
		const int toUserId = 2;
		const int unrelatedUserId = 3;
		_db.AddUser(fromUserId);
		_db.AddUser(toUserId);
		_db.AddUser(unrelatedUserId);
		const int messageId = 100;
		_db.PrivateMessages.Add(new PrivateMessage { Id = messageId, FromUserId = fromUserId, ToUserId = toUserId });
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.SaveMessage(unrelatedUserId, messageId);
		Assert.AreEqual(SaveResult.UpdateFailure, actual);
		Assert.IsFalse(_db.PrivateMessages.Any(pm => pm.SavedForFromUser || pm.SavedForToUser));
	}

	[TestMethod]
	public async Task SaveMessageForUser_MessageFromForUser_SavesForFromUser()
	{
		const int fromUserId = 1;
		const int toUserId = 2;
		const int unrelatedUserId = 3;
		_db.AddUser(fromUserId);
		_db.AddUser(toUserId);
		_db.AddUser(unrelatedUserId);
		const int messageId = 100;
		_db.PrivateMessages.Add(new PrivateMessage { Id = messageId, FromUserId = fromUserId, ToUserId = toUserId });
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.SaveMessage(fromUserId, messageId);
		Assert.AreEqual(SaveResult.Success, actual);
		Assert.IsTrue(_db.PrivateMessages.Any(pm => pm.FromUserId == fromUserId && pm.SavedForFromUser && !pm.SavedForToUser));
	}

	[TestMethod]
	public async Task SaveMessageForUser_MessageFromToUser_SavesForToUser()
	{
		const int fromUserId = 1;
		const int toUserId = 2;
		const int unrelatedUserId = 3;
		_db.AddUser(fromUserId);
		_db.AddUser(toUserId);
		_db.AddUser(unrelatedUserId);
		const int messageId = 100;
		_db.PrivateMessages.Add(new PrivateMessage { Id = messageId, FromUserId = fromUserId, ToUserId = toUserId });
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.SaveMessage(toUserId, messageId);
		Assert.AreEqual(SaveResult.Success, actual);
		Assert.IsTrue(_db.PrivateMessages.Any(pm => pm.ToUserId == toUserId && pm.SavedForToUser && !pm.SavedForFromUser));
	}

	#endregion

	#region DeleteMessage()

	[TestMethod]
	public async Task DeleteMessageForUser_MessageDoesNotExist_ReturnsFailure()
	{
		var user = _db.AddUser("Exists");

		var actual = await _privateMessageService.DeleteMessage(user.Entity.Id, -1);
		Assert.AreEqual(SaveResult.UpdateFailure, actual);
		Assert.IsFalse(_db.PrivateMessages.Any(pm => pm.DeletedForFromUser || pm.DeletedForToUser));
	}

	[TestMethod]
	public async Task DeleteMessageForUser_MessageNotForUser_ReturnsFailure()
	{
		const int fromUserId = 1;
		const int toUserId = 2;
		const int unrelatedUserId = 3;
		_db.AddUser(fromUserId);
		_db.AddUser(toUserId);
		_db.AddUser(unrelatedUserId);
		const int messageId = 100;
		_db.PrivateMessages.Add(new PrivateMessage { Id = messageId, FromUserId = fromUserId, ToUserId = toUserId });
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.DeleteMessage(unrelatedUserId, messageId);
		Assert.AreEqual(SaveResult.UpdateFailure, actual);
		Assert.IsFalse(_db.PrivateMessages.Any(pm => pm.DeletedForFromUser || pm.DeletedForToUser));
	}

	[TestMethod]
	public async Task DeleteMessageForUser_MessageFromFromUser_MessageUnread_HardDeletesMessage()
	{
		const int fromUserId = 1;
		const int toUserId = 2;
		const int unrelatedUserId = 3;
		_db.AddUser(fromUserId);
		_db.AddUser(toUserId);
		_db.AddUser(unrelatedUserId);
		const int messageId = 100;
		_db.PrivateMessages.Add(new PrivateMessage { Id = messageId, FromUserId = fromUserId, ToUserId = toUserId, ReadOn = null });
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.DeleteMessage(fromUserId, messageId);
		Assert.AreEqual(SaveResult.Success, actual);
		Assert.AreEqual(0, _db.PrivateMessages.Count());
	}

	[TestMethod]
	public async Task DeleteMessageForUser_MessageFromFromUser_MessageRead_DeletesForFromUser()
	{
		const int fromUserId = 1;
		const int toUserId = 2;
		const int unrelatedUserId = 3;
		_db.AddUser(fromUserId);
		_db.AddUser(toUserId);
		_db.AddUser(unrelatedUserId);
		const int messageId = 100;
		_db.PrivateMessages.Add(new PrivateMessage { Id = messageId, FromUserId = fromUserId, ToUserId = toUserId, ReadOn = DateTime.UtcNow.AddDays(-1) });
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.DeleteMessage(fromUserId, messageId);
		Assert.AreEqual(SaveResult.Success, actual);
		Assert.IsTrue(_db.PrivateMessages.Any(pm => pm.FromUserId == fromUserId && pm.DeletedForFromUser && !pm.DeletedForToUser));
	}

	[TestMethod]
	public async Task DeleteMessageForUser_MessageFromToUser_DeletesForToUser()
	{
		const int fromUserId = 1;
		const int toUserId = 2;
		const int unrelatedUserId = 3;
		_db.AddUser(fromUserId);
		_db.AddUser(toUserId);
		_db.AddUser(unrelatedUserId);
		const int messageId = 100;
		_db.PrivateMessages.Add(new PrivateMessage { Id = messageId, FromUserId = fromUserId, ToUserId = toUserId });
		await _db.SaveChangesAsync();

		var actual = await _privateMessageService.DeleteMessage(toUserId, messageId);
		Assert.AreEqual(SaveResult.Success, actual);
		Assert.IsTrue(_db.PrivateMessages.Any(pm => pm.ToUserId == toUserId && pm.DeletedForToUser && !pm.DeletedForFromUser));
	}

	#endregion
}
