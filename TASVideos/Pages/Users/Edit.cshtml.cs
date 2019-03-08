using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Users.Models;
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Users
{
	[RequirePermission(PermissionTo.EditUsers)]
	public class EditModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ExternalMediaPublisher _publisher;

		public EditModel(
			ApplicationDbContext db,
			ExternalMediaPublisher publisher)
		{
			_db = db;
			_publisher = publisher;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public UserEditModel UserToEdit { get; set; }

		[DisplayName("Available Roles")]
		public IEnumerable<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>();

		public async Task<IActionResult> OnGet()
		{
			using (await _db.Database.BeginTransactionAsync())
			{
				UserToEdit = await _db.Users
					.Where(u => u.Id == Id)
					.ProjectTo<UserEditModel>()
					.SingleOrDefaultAsync();

				if (UserToEdit == null)
				{
					return NotFound();
				}

				AvailableRoles = await GetAllRolesUserCanAssign(User.GetUserId(), UserToEdit.SelectedRoles);
			}

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
			
			var currentRoles = await _db.UserRoles
				.Where(ur => ur.User == user)
				.ToListAsync();

			_db.UserRoles.RemoveRange(currentRoles);

			try
			{
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				return BadRequest("Unable to modify user. User data may have been modified since the page was opened.");
			}

			_db.UserRoles.AddRange(UserToEdit.SelectedRoles
				.Select(r => new UserRole
				{
					User = user,
					RoleId = r
				}));

			try
			{
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				return BadRequest("Unable to modify user. User data may have been modified since the page was opened.");
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
				var message = $"user {user.UserName} roles modified by {User.Identity.Name},";
				if (addedRoles.Any())
				{
					message += " added: " + string.Join(",", addedRoles);
				}

				if (removedRoles.Any())
				{
					message += " removed: " + string.Join(",", removedRoles);
				}
				
				_publisher.SendUserManagement(message, "", $"{BaseUrl}/Users/Profile/{user.UserName}");
			}

			return RedirectToPage("List");
		}

		public async Task<IActionResult> OnGetUnlock(string returnUrl)
		{
			var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == Id);
			if (user == null)
			{
				return NotFound();
			}

			user.LockoutEnd = null;
			await _db.SaveChangesAsync();
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
				.SelectMany(ur => ur.Role.RolePermission)
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
				.SelectMany(ur => ur.Role.RolePermission)
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
