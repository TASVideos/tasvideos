using TASVideos.Core.Services.Email;

namespace TASVideos.Core.Services;

public interface IPrivateMessageService
{
	Task SendMessage(int fromUserId, string toUserName, string subject, string text);
	Task SendMessageToRole(int fromUserId, string roleName, string subject, string text);
	
	/// <summary>
	/// Returns a list of roles that are allowed for bulk sending
	/// </summary>
	Task<string[]> AllowedRoles();
}

internal class PrivateMessageService : IPrivateMessageService
{
	// TODO: this does not belong in code, move to a system wiki page, or database table
	private static readonly string[] AllowedBulkRoles = { "site admin", "moderator" };

	private readonly ApplicationDbContext _db;
	private readonly IEmailService _emailService;

	public PrivateMessageService(ApplicationDbContext db, IEmailService emailService)
	{
		_db = db;
		_emailService = emailService;
	}

	public async Task SendMessage(int fromUserId, string toUserName, string subject, string text)
	{
		var toUser = await _db.Users
			.Where(u => u.UserName == toUserName)
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

		_db.PrivateMessages.Add(message);
		await _db.SaveChangesAsync();

		if (toUser.EmailOnPrivateMessage)
		{
			await _emailService.NewPrivateMessage(toUser.Email, toUser.UserName);
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

		var role = await _db.Roles
			.Where(r => r.Name.ToLower() == roleName)
			.SingleOrDefaultAsync();

		if (role is null)
		{
			return;
		}

		var users = await _db.Users
			.Where(u => u.UserRoles.Any(ur => ur.RoleId == role.Id))
			.ToListAsync();

		string bulkSubject = $"Sent to role {role}: {subject}";

		foreach (var user in users)
		{
			await SendMessage(fromUserId, user.UserName, bulkSubject, text);
		}
	}

	public async Task<string[]> AllowedRoles()
	{
		return await Task.FromResult(AllowedBulkRoles);
	}
}
