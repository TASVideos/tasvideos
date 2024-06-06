namespace TASVideos.Pages.Users;

[RequirePermission(matchAny: false, PermissionTo.SeeEmails, PermissionTo.EditUsers)]
public class NukeModel(ApplicationDbContext db, IUserMaintenanceLogger userMaintenanceLogger) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	public string OriginalUserName { get; set; } = "";
	public string AnonymousUserName => $"Anonymous{Id}";
	public string AnonymousEmail => $"{AnonymousUserName}@example.com";

	public async Task<IActionResult> OnGet()
	{
		if (User.GetUserId() == Id)
		{
			return RedirectToPage("/Profile/Settings");
		}

		var userName = await db.Users
			.Where(u => u.Id == Id)
			.Select(u => u.UserName)
			.SingleOrDefaultAsync();

		if (userName is null)
		{
			return NotFound();
		}

		OriginalUserName = userName;
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		var user = await db.Users.FindAsync(Id);
		if (user is null)
		{
			return NotFound();
		}

		OriginalUserName = user.UserName;

		user.PasswordHash = "";
		user.UserName = AnonymousUserName;
		user.NormalizedUserName = user.UserName.ToUpperInvariant();
		user.Email = AnonymousEmail;
		user.NormalizedEmail = user.Email.ToUpperInvariant();
		user.From = null;
		user.TimeZoneId = TimeZoneInfo.Utc.Id;
		user.LastLoggedInTimeStamp = null;
		user.CreateTimestamp = DateTime.UnixEpoch;
		user.Signature = null;
		user.Avatar = null;
		user.MoodAvatarUrlBase = null;
		user.PreferredPronouns = PreferredPronounTypes.Unspecified;

		var roles = await db.UserRoles
			.Where(ur => ur.UserId == user.Id)
			.ToListAsync();
		db.UserRoles.RemoveRange(roles);

		var votes = await db.ForumPollOptionVotes
			.Where(v => v.UserId == user.Id)
			.ToListAsync();

		foreach (var vote in votes)
		{
			vote.IpAddress = null;
		}

		var posts = await db.ForumPosts
			.Where(p => p.PosterId == user.Id)
			.ToListAsync();

		foreach (var post in posts)
		{
			post.IpAddress = null;
		}

		var logs = await db.UserMaintenanceLogs
			.Where(l => l.UserId == user.Id)
			.ToListAsync();

		db.UserMaintenanceLogs.RemoveRange(logs);
		await db.SaveChangesAsync();

		// The simple solution to having the correct data for pubs and subs is to save changes first
		// This is a repeatable process, so we aren't worried about partial successes
		// And this is very rare, so we aren't as worried about speed
		var pubs = await db.Publications
			.IncludeTitleTables()
			.ForAuthor(user.Id)
			.ToListAsync();

		foreach (var pub in pubs)
		{
			pub.GenerateTitle();
		}

		var subs = await db.Submissions
			.IncludeTitleTables()
			.ForAuthor(user.Id)
			.ToListAsync();
		foreach (var sub in subs)
		{
			sub.GenerateTitle();
		}

		await userMaintenanceLogger.Log(user.Id, "User was anonymized", User.GetUserId());

		// If username is changed, we want to ignore the returnUrl that will be the old name
		return BasePageRedirect("Edit", new { Id });
	}
}
