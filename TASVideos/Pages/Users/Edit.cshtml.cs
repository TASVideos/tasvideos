﻿using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Users.Models;

namespace TASVideos.Pages.Users;

[RequirePermission(PermissionTo.EditUsers)]
public class EditModel : BasePageModel
{
	private readonly IRoleService _roleService;
	private readonly ApplicationDbContext _db;
	private readonly ExternalMediaPublisher _publisher;
	private readonly IUserMaintenanceLogger _userMaintenanceLogger;
	private readonly UserManager _userManager;

	public EditModel(
		IRoleService roleService,
		ApplicationDbContext db,
		ExternalMediaPublisher publisher,
		IUserMaintenanceLogger userMaintenanceLogger,
		UserManager userManager)
	{
		_roleService = roleService;
		_db = db;
		_publisher = publisher;
		_userMaintenanceLogger = userMaintenanceLogger;
		_userManager = userManager;
	}

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public UserEditModel UserToEdit { get; set; } = new();

	[DisplayName("Available Roles")]
	public IEnumerable<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();

	public async Task<IActionResult> OnGet()
	{
		if (User.GetUserId() == Id)
		{
			return RedirectToPage("/Profile/Settings");
		}

		var userToEdit = await _db.Users
			.Where(u => u.Id == Id)
			.ToUserEditModel()
			.SingleOrDefaultAsync();

		if (userToEdit is null)
		{
			return NotFound();
		}

		UserToEdit = userToEdit;
		var roles = await _roleService.GetAllRolesUserCanAssign(User.GetUserId(), UserToEdit.SelectedRoles);
		AvailableRoles = roles.ToDropDown();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		var roles = await _roleService.GetAllRolesUserCanAssign(User.GetUserId(), UserToEdit.SelectedRoles);
		if (!ModelState.IsValid)
		{
			AvailableRoles = roles.ToDropDown();
			return Page();
		}

		var user = await _db.Users.Include(u => u.UserRoles).SingleOrDefaultAsync(u => u.Id == Id);
		if (user is null)
		{
			return NotFound();
		}

		// Double check user can assign all new the roles they are requesting to assign
		var rolesThatUserCanAssign = roles.Select(r => r.Id);
		if (UserToEdit.SelectedRoles.Except(rolesThatUserCanAssign).Any())
		{
			return AccessDenied();
		}

		var userNameChange = UserToEdit.UserName != user.UserName
			? user.UserName
			: null;

		user.UserName = UserToEdit.UserName;
		user.TimeZoneId = UserToEdit.TimezoneId;
		user.From = UserToEdit.From;
		user.Signature = UserToEdit.Signature;
		user.Avatar = UserToEdit.Avatar;
		user.MoodAvatarUrlBase = UserToEdit.MoodAvatarUrlBase;
		user.UseRatings = UserToEdit.UseRatings;
		user.ModeratorComments = UserToEdit.ModeratorComments;

		var currentRoles = await _db.UserRoles
			.Where(ur => ur.User == user)
			.ToListAsync();

		_db.UserRoles.RemoveRange(currentRoles);

		var result = await ConcurrentSave(_db, "", $"Unable to update user data for {user.UserName}");
		if (!result)
		{
			return BasePageRedirect("List");
		}

		_db.UserRoles.AddRange(UserToEdit.SelectedRoles
			.Select(r => new UserRole
			{
				User = user,
				RoleId = r
			}));

		var saveResult2 = await ConcurrentSave(_db, "", $"Unable to update user data for {user.UserName}");
		if (!saveResult2)
		{
			return BasePageRedirect("List");
		}

		if (userNameChange != null)
		{
			string message = $"Username {userNameChange} changed to {user.UserName} by {User.Name()}";
			await _publisher.SendUserManagement(
				message,
				$"Username {userNameChange} changed to [{user.UserName}]({{0}}) by {User.Name()}",
				"",
				$"Users/Profile/{Uri.EscapeDataString(user.UserName!)}");
			await _userMaintenanceLogger.Log(user.Id, message, User.GetUserId());
			await _userManager.UserNameChanged(user, userNameChange);
		}

		// Announce Role change
		var allRoles = await _db.Roles.ToListAsync();
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

			await _publisher.SendUserManagement(
				$"User {user.UserName} edited by {User.Name()}",
				$"User [{user.UserName}]({{0}}) edited by {User.Name()}",
				message,
				$"Users/Profile/{Uri.EscapeDataString(user.UserName!)}");
			await _userMaintenanceLogger.Log(user.Id, message, User.GetUserId());
		}

		SuccessStatusMessage($"User {user.UserName} updated");

		// If username is changed, we want to ignore the returnUrl that will be the old name
		return userNameChange != null
			? RedirectToPage("/Users/Profile", new { Name = user.UserName })
			: BasePageRedirect("List");
	}

	public async Task<IActionResult> OnGetUnlock()
	{
		var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == Id);
		if (user is null)
		{
			return NotFound();
		}

		user.LockoutEnd = null;
		await ConcurrentSave(_db, $"User {user.UserName} unlocked", $"Unable to unlock user {user.UserName}");

		return BaseReturnUrlRedirect();
	}
}
