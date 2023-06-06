using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Users;

[RequirePermission(matchAny: false, PermissionTo.SeeEmails, PermissionTo.EditUsers)]
public class NukeModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly IUserMaintenanceLogger _userMaintenanceLogger;

	public NukeModel(ApplicationDbContext db, IUserMaintenanceLogger userMaintenanceLogger)
	{
		_db = db;
		_userMaintenanceLogger = userMaintenanceLogger;
	}

	[FromRoute]
	public int Id { get; set; }

	// TODO: move me
	public class UserModel
	{
		public int Id { get; init; }

		[Display(Name = "Original UserName")]
		public string UserName { get; init; } = "";

		[Display(Name = "New Username")]
		public string AnonymousUserName => $"Anonymous{Id}";

		[Display(Name = "New Email")]
		public string AnonymousEmail => $"{AnonymousUserName}@example.com";
	}

	public UserModel Profile { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		if (User.GetUserId() == Id)
		{
			return RedirectToPage("/Profile/Settings");
		}

		var profile = await _db.Users
			.Where(u => u.Id == Id)
			.Select(u => new UserModel
			{
				Id = u.Id,
				UserName = u.UserName
			})
			.SingleOrDefaultAsync();

		if (profile is null)
		{
			return NotFound();
		}

		Profile = profile;
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == Id);
		if (user is null)
		{
			return NotFound();
		}

		var anonModel = new UserModel { Id = user.Id };

		user.PasswordHash = "";
		user.UserName = anonModel.AnonymousUserName;
		user.NormalizedUserName = user.UserName.ToUpperInvariant();
		user.Email = anonModel.AnonymousEmail;
		user.NormalizedEmail = user.Email.ToUpperInvariant();
		user.From = null;
		user.TimeZoneId = TimeZoneInfo.Utc.Id;
		user.LastLoggedInTimeStamp = null;
		user.CreateTimestamp = DateTime.UnixEpoch;
		user.Signature = null;
		user.Avatar = null;
		user.MoodAvatarUrlBase = null;
		user.PreferredPronouns = PreferredPronounTypes.Unspecified;

		var roles = await _db.UserRoles
			.Where(ur => ur.UserId == user.Id)
			.ToListAsync();
		_db.UserRoles.RemoveRange(roles);

		var votes = await _db.ForumPollOptionVotes
			.Where(v => v.UserId == user.Id)
			.ToListAsync();

		foreach (var vote in votes)
		{
			vote.IpAddress = null;
		}

		var posts = await _db.ForumPosts
			.Where(p => p.PosterId == user.Id)
			.ToListAsync();

		foreach (var post in posts)
		{
			post.IpAddress = null;
		}

		var logs = await _db.UserMaintenanceLogs
			.Where(l => l.UserId == user.Id)
			.ToListAsync();

		_db.UserMaintenanceLogs.RemoveRange(logs);
		await _db.SaveChangesAsync();

		// The simple solution to having the correct data for pubs and subs is to save changes first
		// This is a repeatable process so we aren't worried about partial successes
		// And this is very rare so we aren't as worried about about speed
		var pubs = await _db.Publications
			.IncludeTitleTables()
			.ForAuthor(user.Id)
			.ToListAsync();

		foreach (var pub in pubs)
		{
			pub.GenerateTitle();
		}

		var subs = await _db.Submissions
			.IncludeTitleTables()
			.ForAuthor(user.Id)
			.ToListAsync();
		foreach (var sub in subs)
		{
			sub.GenerateTitle();
		}

		await _userMaintenanceLogger.Log(user.Id, "User was anonymized", User.GetUserId());

		// If username is changed, we want to ignore the returnUrl that will be the old name
		return BasePageRedirect("Edit", new { Id });
	}
}
