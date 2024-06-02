using System.ComponentModel;
using TASVideos.Core.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Users;

[RequirePermission(PermissionTo.EditUsers)]
public class EditModel(
	IRoleService roleService,
	ApplicationDbContext db,
	ExternalMediaPublisher publisher,
	IUserMaintenanceLogger userMaintenanceLogger,
	UserManager userManager)
	: BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public UserEditModel UserToEdit { get; set; } = new();

	[DisplayName("Available Roles")]
	public List<SelectListItem> AvailableRoles { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		if (User.GetUserId() == Id)
		{
			return RedirectToPage("/Profile/Settings");
		}

		var userToEdit = await db.Users
			.Where(u => u.Id == Id)
			.Select(u => new UserEditModel
			{
				UserName = u.UserName,
				TimezoneId = u.TimeZoneId,
				From = u.From,
				SelectedRoles = u.UserRoles.Select(ur => ur.RoleId).ToList(),
				CreateTimestamp = u.CreateTimestamp,
				LastLoggedInTimeStamp = u.LastLoggedInTimeStamp,
				Email = u.Email,
				EmailConfirmed = u.EmailConfirmed,
				IsLockedOut = u.LockoutEnabled && u.LockoutEnd.HasValue,
				Signature = u.Signature,
				Avatar = u.Avatar,
				MoodAvatarUrlBase = u.MoodAvatarUrlBase,
				UseRatings = u.UseRatings,
				BannedUntil = u.BannedUntil,
				ModeratorComments = u.ModeratorComments
			})
			.SingleOrDefaultAsync();

		if (userToEdit is null)
		{
			return NotFound();
		}

		UserToEdit = userToEdit;
		var roles = await roleService.GetAllRolesUserCanAssign(User.GetUserId(), UserToEdit.SelectedRoles);
		AvailableRoles = roles.ToDropDown().ToList();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		var roles = await roleService.GetAllRolesUserCanAssign(User.GetUserId(), UserToEdit.SelectedRoles);
		if (!ModelState.IsValid)
		{
			AvailableRoles = roles.ToDropDown().ToList();
			return Page();
		}

		var user = await db.Users.Include(u => u.UserRoles).SingleOrDefaultAsync(u => u.Id == Id);
		if (user is null)
		{
			return NotFound();
		}

		// Double check user can assign all new the roles they are requesting to assign
		var rolesThatUserCanAssign = roles.Where(r => !r.Disabled).Select(r => r.Id).ToList();
		if (UserToEdit.SelectedRoles.Except(rolesThatUserCanAssign).Any())
		{
			return AccessDenied();
		}

		var userNameChange = UserToEdit.UserName != user.UserName
			? user.UserName
			: null;

		if (UserToEdit.UserName is not null)
		{
			user.UserName = UserToEdit.UserName;
		}

		user.TimeZoneId = UserToEdit.TimezoneId;
		user.From = UserToEdit.From;
		user.Signature = UserToEdit.Signature;
		user.Avatar = UserToEdit.Avatar;
		user.MoodAvatarUrlBase = UserToEdit.MoodAvatarUrlBase;
		user.UseRatings = UserToEdit.UseRatings;
		user.BannedUntil = UserToEdit.BannedUntil;
		user.ModeratorComments = UserToEdit.ModeratorComments;

		var currentRoles = await db.UserRoles
			.Where(ur => ur.User == user && rolesThatUserCanAssign.Contains(ur.RoleId))
			.ToListAsync();

		db.UserRoles.RemoveRange(currentRoles);

		var result = await db.TrySaveChanges();
		if (result != SaveResult.Success)
		{
			ErrorStatusMessage($"Unable to update user data for {user.UserName}");
			return BasePageRedirect("List");
		}

		db.UserRoles.AddRange(UserToEdit.SelectedRoles
			.Select(r => new UserRole
			{
				User = user,
				RoleId = r
			}));

		var saveResult2 = await db.TrySaveChanges();
		if (saveResult2 != SaveResult.Success)
		{
			ErrorStatusMessage($"Unable to update user data for {user.UserName}");
			return BasePageRedirect("List");
		}

		if (userNameChange is not null)
		{
			await publisher.SendUserManagement(
				$"Username {userNameChange} changed to [{user.UserName}]({{0}}) by {User.Name()}", user.UserName);
			string message = $"Username {userNameChange} changed to {user.UserName} by {User.Name()}";
			await userMaintenanceLogger.Log(user.Id, message, User.GetUserId());
			await userManager.UserNameChanged(user, userNameChange);
		}

		// Announce Role change
		var allRoles = await db.Roles.ToListAsync();
		var currentRoleIds = currentRoles.Select(r => r.RoleId).ToList();
		var newRoleIds = UserToEdit.SelectedRoles.ToList();
		var addedRoles = allRoles
			.Where(r => newRoleIds.Except(currentRoleIds).Contains(r.Id))
			.Select(r => r.Name)
			.ToList();
		var removedRoles = allRoles
			.Where(r => currentRoleIds.Except(newRoleIds).Contains(r.Id))
			.Select(r => r.Name)
			.ToList();

		var anyAddedRoles = addedRoles.Any();
		var anyRemovedRoles = removedRoles.Any();
		if (anyAddedRoles || anyRemovedRoles)
		{
			var message = "";
			if (anyAddedRoles)
			{
				message += "Added roles: " + string.Join(", ", addedRoles);
			}

			if (anyAddedRoles && anyRemovedRoles)
			{
				message += " | ";
			}

			if (anyRemovedRoles)
			{
				message += "Removed roles: " + string.Join(", ", removedRoles);
			}

			await publisher.SendUserManagement($"User [{user.UserName}]({{0}}) edited by {User.Name()}", user.UserName);
			await userMaintenanceLogger.Log(user.Id, message, User.GetUserId());
		}

		SuccessStatusMessage($"User {user.UserName} updated");

		// If username is changed, we want to ignore the returnUrl that will be the old name
		return userNameChange is not null
			? RedirectToPage("/Users/Profile", new { Name = user.UserName })
			: BasePageRedirect("List");
	}

	public async Task<IActionResult> OnGetUnlock()
	{
		var user = await db.Users.SingleOrDefaultAsync(u => u.Id == Id);
		if (user is null)
		{
			return NotFound();
		}

		user.LockoutEnd = null;
		SetMessage(await db.TrySaveChanges(), $"User {user.UserName} unlocked", $"Unable to unlock user {user.UserName}");

		return BaseReturnUrlRedirect();
	}

	public class UserEditModel
	{
		[DisplayName("User Name")]
		[StringLength(50)]
		public string? UserName { get; init; }

		[DisplayName("Time Zone")]
		public string TimezoneId { get; init; } = TimeZoneInfo.Utc.Id;

		[Display(Name = "Location")]
		public string? From { get; init; }

		[DisplayName("Selected Roles")]
		public List<int> SelectedRoles { get; init; } = [];

		[DisplayName("Account Created On")]
		public DateTime CreateTimestamp { get; init; }

		[DisplayName("User Last Logged In")]
		[DisplayFormat(NullDisplayText = "Never")]
		public DateTime? LastLoggedInTimeStamp { get; init; }

		[EmailAddress]
		public string? Email { get; init; }
		public bool EmailConfirmed { get; init; }

		[Display(Name = "Locked Status")]
		public bool IsLockedOut { get; init; }
		public string? Signature { get; init; }
		public string? Avatar { get; init; }

		[Display(Name = "Mood Avatar")]
		public string? MoodAvatarUrlBase { get; init; }
		public string? OriginalUserName => UserName;

		[Display(Name = "Use Ratings", Description = "If unchecked, the user's publication ratings will not be used when calculating average rating")]
		public bool UseRatings { get; init; }

		public DateTime? BannedUntil { get; init; }

		[Display(Name = "Moderator Comments")]
		public string? ModeratorComments { get; init; }
	}
}
