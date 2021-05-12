using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.RazorPages.Pages.Users.Models;

namespace TASVideos.RazorPages.Pages.Users
{
	[RequirePermission(PermissionTo.EditUsers)]
	public class EditModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;
		private readonly ExternalMediaPublisher _publisher;

		public EditModel(
			ApplicationDbContext db,
			IMapper mapper,
			ExternalMediaPublisher publisher)
		{
			_db = db;
			_mapper = mapper;
			_publisher = publisher;
		}

		[FromRoute]
		public int Id { get; set; }

		[FromQuery]
		public string? ReturnUrl { get; set; }

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
				return ReturnToPreviousPage();
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
				return ReturnToPreviousPage();
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
			if (addedRoles.Any() || removedRoles.Any())
			{
				var message = $"user {user.UserName} roles modified by {User.Name()},";
				if (addedRoles.Any())
				{
					message += " added: " + string.Join(",", addedRoles);
				}

				if (removedRoles.Any())
				{
					message += " removed: " + string.Join(",", removedRoles);
				}

				_publisher.SendUserManagement(message, "", $"Users/Profile/{user.UserName}", user.UserName!);
			}

			SuccessStatusMessage($"User {user.UserName} updated");
			return ReturnToPreviousPage();
		}

		private IActionResult ReturnToPreviousPage() => string.IsNullOrWhiteSpace(ReturnUrl)
			? RedirectToPage("List")
			: RedirectToLocal(ReturnUrl);

		public async Task<IActionResult> OnGetUnlock(string returnUrl)
		{
			var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == Id);
			if (user == null)
			{
				return NotFound();
			}

			user.LockoutEnd = null;
			await ConcurrentSave(_db, $"User {user.UserName} unlocked", $"Unable to unlock user {user.UserName}");

			return RedirectToLocal(returnUrl);
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
