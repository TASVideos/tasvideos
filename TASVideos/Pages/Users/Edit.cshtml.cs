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
using TASVideos.Models;
using TASVideos.Pages.Users.Models;

namespace TASVideos.Pages.Users
{
	[RequirePermission(PermissionTo.EditUsers)]
	public class EditModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public EditModel(ApplicationDbContext db)
		{
			_db = db;
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

			var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == Id);
			if (user == null)
			{
				return NotFound();
			}

			if (UserToEdit.UserName != user.UserName)
			{
				user.UserName = UserToEdit.UserName;
			}

			if (UserToEdit.TimezoneId != user.TimeZoneId)
			{
				user.TimeZoneId = UserToEdit.TimezoneId;
			}

			user.From = UserToEdit.From;
			
			_db.UserRoles.RemoveRange(_db.UserRoles.Where(ur => ur.User == user));
			await _db.SaveChangesAsync();

			_db.UserRoles.AddRange(UserToEdit.SelectedRoles
				.Select(r => new UserRole
				{
					User = user,
					RoleId = r
				}));

			await _db.SaveChangesAsync();

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
	}
}
