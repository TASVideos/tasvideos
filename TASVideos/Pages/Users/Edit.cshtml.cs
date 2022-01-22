using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Users.Models;

namespace TASVideos.Pages.Users
{
	[RequirePermission(PermissionTo.EditUsers)]
	public class EditModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;
		private readonly ExternalMediaPublisher _publisher;
		private readonly IUserMaintenanceLogger _userMaintenanceLogger;

		public EditModel(
			ApplicationDbContext db,
			IMapper mapper,
			ExternalMediaPublisher publisher,
			IUserMaintenanceLogger userMaintenanceLogger)
		{
			_db = db;
			_mapper = mapper;
			_publisher = publisher;
			_userMaintenanceLogger = userMaintenanceLogger;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public UserEditModel UserToEdit { get; set; } = new ();

		[DisplayName("Available Roles")]
		public IEnumerable<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();

		public async Task<IActionResult> OnGet()
		{
			if (User.GetUserId() == Id)
			{
				return RedirectToPage("/Profile/Settings");
			}

			UserToEdit = await _mapper.ProjectTo<UserEditModel>(
					_db.Users.Where(u => u.Id == Id))
				.SingleOrDefaultAsync();

			if (UserToEdit == null)
			{
				return NotFound();
			}

			AvailableRoles = await GetAllRolesUserCanAssign(User.GetUserId(), UserToEdit.SelectedRoles);
			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				AvailableRoles = await GetAllRolesUserCanAssign(User.GetUserId(), UserToEdit.SelectedRoles);
				return Page();
			}

			var user = await _db.Users.Include(u => u.UserRoles).SingleOrDefaultAsync(u => u.Id == Id);
			if (user == null)
			{
				return NotFound();
			}

			// Double check user can assign all new the roles they are requesting to assign
			var rolesThatUserCanAssign = await GetAllRoleIdsUserCanAssign(User.GetUserId(), user.UserRoles.Select(ur => ur.RoleId));
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
					"",
					$"Users/Profile/{user.UserName}");
				await _userMaintenanceLogger.Log(user.Id, message, User.GetUserId());
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
					message,
					$"Users/Profile/{user.UserName}");
				await _userMaintenanceLogger.Log(user.Id, message, User.GetUserId());
			}

			SuccessStatusMessage($"User {user.UserName} updated");
			return BasePageRedirect("List");
		}

		public async Task<IActionResult> OnGetUnlock()
		{
			var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == Id);
			if (user == null)
			{
				return NotFound();
			}

			user.LockoutEnd = null;
			await ConcurrentSave(_db, $"User {user.UserName} unlocked", $"Unable to unlock user {user.UserName}");

			return BaseReturnUrlRedirect();
		}

		private async Task<IEnumerable<SelectListItem>> GetAllRolesUserCanAssign(int userId, IEnumerable<int> assignedRoles)
		{
			if (assignedRoles == null)
			{
				throw new ArgumentException($"{nameof(assignedRoles)} can not be null");
			}

			var assignedRoleList = assignedRoles.ToList();
			var assignablePermissions = await _db.Users
				.Where(u => u.Id == userId)
				.SelectMany(u => u.UserRoles)
				.SelectMany(ur => ur.Role!.RolePermission)
				.Where(rp => rp.CanAssign)
				.Select(rp => rp.PermissionId)
				.ToListAsync();

			return await _db.Roles
				.Where(r => r.RolePermission.All(rp => assignablePermissions.Contains(rp.PermissionId))
					|| assignedRoleList.Contains(r.Id))
				.Select(r => new SelectListItem
				{
					Value = r.Id.ToString(),
					Text = r.Name,
					Disabled = !r.RolePermission.All(rp => assignablePermissions.Contains(rp.PermissionId))
						&& assignedRoleList.Any() // EF Core 2.1 issue, needs this or a user with no assigned roles blows up
						&& assignedRoleList.Contains(r.Id)
				})
				.ToListAsync();
		}

		// TODO: reduce copy-pasta
		private async Task<IEnumerable<int>> GetAllRoleIdsUserCanAssign(int userId, IEnumerable<int> assignedRoles)
		{
			if (assignedRoles == null)
			{
				throw new ArgumentException($"{nameof(assignedRoles)} can not be null");
			}

			var assignedRoleList = assignedRoles.ToList();
			var assignablePermissions = await _db.Users
				.Where(u => u.Id == userId)
				.SelectMany(u => u.UserRoles)
				.SelectMany(ur => ur.Role!.RolePermission)
				.Where(rp => rp.CanAssign)
				.Select(rp => rp.PermissionId)
				.ToListAsync();

			return await _db.Roles
				.Where(r => r.RolePermission.All(rp => assignablePermissions.Contains(rp.PermissionId))
					|| assignedRoleList.Contains(r.Id))
				.Select(r => r.Id)
				.ToListAsync();
		}
	}
}
