using TASVideos.Core.Services.Email;

namespace TASVideos.Core.Services;

public interface IPrivateMessageService
{
	/// <summary>
	/// Returns the <see cref="PrivateMessage"/>
	/// record with the given <see cref="id"/> if the user has access to the message
	/// A user has access if they are the sender or the receiver of the message
	/// </summary>
	Task<Message?> GetMessage(int userId, int id);
	Task<PageOf<InboxEntry>> GetInbox(int userId, PagingModel paging);
	Task<PageOf<SentboxEntry>> GetSentInbox(int userId, PagingModel paging);
	Task<ICollection<SaveboxEntry>> GetSavebox(int userId);

	/// <summary>
	/// Returns the number of unread <see cref="PrivateMessage"/>
	/// for the given <see cref="User" />
	/// </summary>
	ValueTask<int> GetUnreadMessageCount(int userId);
	Task SendMessage(int fromUserId, string toUserName, string subject, string text);
	Task SendMessageToRole(int fromUserId, string roleName, string subject, string text);
	Task<SaveResult> SaveMessage(int userId, int privateMessageId);
	Task<SaveResult> DeleteMessage(int userId, int privateMessageId);

	/// <summary>
	/// Returns a list of roles that are allowed for bulk sending
	/// </summary>
	Task<string[]> AllowedRoles();
}

internal class PrivateMessageService(ApplicationDbContext db, ICacheService cache, IEmailService emailService) : IPrivateMessageService
{
	internal const string UnreadMessageCount = "UnreadMessageCountCache-";

	// TODO: this does not belong in code, move to a system wiki page, or database table
	private static readonly string[] AllowedBulkRoles = ["site admin", "moderator"];
	public async Task<Message?> GetMessage(int userId, int id)
	{
		var pm = await db.PrivateMessages
			.Include(p => p.FromUser)
			.Include(p => p.ToUser)
			.Where(p => (!p.DeletedForFromUser && p.FromUserId == userId)
				|| (!p.DeletedForToUser && p.ToUserId == userId))
			.SingleOrDefaultAsync(p => p.Id == id);

		if (pm is null)
		{
			return null;
		}

		// If it is the recipient and the message are not deleted
		if (!pm.ReadOn.HasValue && pm.ToUserId == userId)
		{
			pm.ReadOn = DateTime.UtcNow;
			await db.SaveChangesAsync();
			cache.Remove(UnreadMessageCount + userId); // Message count possibly no longer valid
		}

		return new Message(
			pm.Subject,
			pm.CreateTimestamp,
			pm.Text,
			pm.FromUserId,
			pm.FromUser!.UserName,
			pm.ToUserId,
			pm.ToUser!.UserName,
			pm.ToUserId == userId,
			pm.EnableBbCode,
			pm.EnableHtml);
	}

	public async Task<PageOf<InboxEntry>> GetInbox(int userId, PagingModel paging)
	{
		return await db.PrivateMessages
			.SentToUser(userId)
			.ThatAreNotToUserDeleted()
			.ThatAreNotToUserSaved()
			.OrderBy(m => m.ReadOn.HasValue)
			.ThenByDescending(m => m.CreateTimestamp)
			.Select(pm => new InboxEntry(
				pm.Id,
				pm.Subject,
				pm.FromUser!.UserName,
				pm.CreateTimestamp,
				pm.ReadOn.HasValue))
			.PageOf(paging);
	}

	public async Task<PageOf<SentboxEntry>> GetSentInbox(int userId, PagingModel paging)
	{
		return await db.PrivateMessages
			.ThatAreNotToUserDeleted()
			.FromUser(userId)
			.OrderBy(m => m.ReadOn.HasValue)
			.ThenByDescending(m => m.CreateTimestamp)
			.Select(pm => new SentboxEntry(
				pm.Id,
				pm.Subject,
				pm.ToUser!.UserName,
				pm.CreateTimestamp,
				pm.ReadOn.HasValue))
			.PageOf(paging);
	}

	public async Task<ICollection<SaveboxEntry>> GetSavebox(int userId)
	{
		return await db.PrivateMessages
			.ThatAreSavedByUser(userId)
			.OrderBy(m => m.ToUser!.Id == userId)
			.ThenByDescending(m => m.CreateTimestamp)
			.Select(pm => new SaveboxEntry(
				pm.Id,
				pm.Subject,
				pm.FromUser!.UserName,
				pm.ToUser!.UserName,
				pm.CreateTimestamp))
			.ToListAsync();
	}

	public async ValueTask<int> GetUnreadMessageCount(int userId)
	{
		var cacheKey = UnreadMessageCount + userId;
		if (cache.TryGetValue(cacheKey, out int unreadMessageCount))
		{
			return unreadMessageCount;
		}

		unreadMessageCount = await db.PrivateMessages
			.ThatAreNotToUserDeleted()
			.SentToUser(userId)
			.CountAsync(pm => pm.ReadOn == null);

		cache.Set(cacheKey, unreadMessageCount, Durations.OneMinute);
		return unreadMessageCount;
	}

	public async Task SendMessage(int fromUserId, string toUserName, string subject, string text)
	{
		var toUser = await db.Users
			.ForUser(toUserName)
			.Select(u => new
			{
				u.Id,
				u.UserName,
				u.Email,
				u.EmailOnPrivateMessage
			})
			.SingleOrDefaultAsync();

		if (toUser is null)
		{
			return;
		}

		var message = new PrivateMessage
		{
			FromUserId = fromUserId,
			ToUserId = toUser.Id,
			Subject = subject,
			Text = text,
			EnableBbCode = true
		};

		db.PrivateMessages.Add(message);
		await db.SaveChangesAsync();

		if (toUser.EmailOnPrivateMessage)
		{
			await emailService.NewPrivateMessage(toUser.Email, toUser.UserName);
		}
	}

	public async Task SendMessageToRole(int fromUserId, string roleName, string subject, string text)
	{
		roleName = roleName.ToLower().Trim();

		var allowed = await AllowedRoles();
		if (!allowed.Contains(roleName))
		{
			return;
		}

		var role = await db.Roles
			.Where(r => r.Name == roleName)
			.SingleOrDefaultAsync();

		if (role is null)
		{
			return;
		}

		var users = await db.Users
			.Where(u => u.UserRoles.Any(ur => ur.RoleId == role.Id))
			.ToListAsync();

		string bulkSubject = $"Sent to role {role}: {subject}";

		foreach (var user in users)
		{
			await SendMessage(fromUserId, user.UserName, bulkSubject, text);
		}
	}

	public async Task<SaveResult> SaveMessage(int userId, int privateMessageId)
	{
		var message = await db.PrivateMessages.FindAsync(privateMessageId);
		if (message is null)
		{
			return SaveResult.UpdateFailure;
		}

		if (message.FromUserId == userId)
		{
			message.SavedForFromUser = true;
		}
		else if (message.ToUserId == userId)
		{
			message.SavedForToUser = true;
		}
		else
		{
			return SaveResult.UpdateFailure;
		}

		return await db.TrySaveChanges();
	}

	public async Task<SaveResult> DeleteMessage(int userId, int privateMessageId)
	{
		var message = await db.PrivateMessages.FindAsync(privateMessageId);
		if (message is null)
		{
			return SaveResult.UpdateFailure;
		}

		if (message.FromUserId == userId)
		{
			if (message.ReadOn.HasValue)
			{
				message.DeletedForFromUser = true;
			}
			else
			{
				db.PrivateMessages.Remove(message);
			}
		}
		else if (message.ToUserId == userId)
		{
			message.DeletedForToUser = true;
		}
		else
		{
			return SaveResult.UpdateFailure;
		}

		return await db.TrySaveChanges();
	}

	public async Task<string[]> AllowedRoles() => await Task.FromResult(AllowedBulkRoles);
}

public record InboxEntry(int Id, string? Subject, string From, DateTime Date, bool IsRead);
public record SentboxEntry(int Id, string? Subject, string To, DateTime SendDate, bool IsRead);
public record SaveboxEntry(int Id, string? Subject, string From, string To, DateTime SendDate);

public record Message(
	string? Subject,
	DateTime SentOn,
	string Text,
	int FromUserId,
	string FromUserName,
	int ToUserId,
	string ToUserName,
	bool CanReply,
	bool EnableBbCode,
	bool EnableHtml);
