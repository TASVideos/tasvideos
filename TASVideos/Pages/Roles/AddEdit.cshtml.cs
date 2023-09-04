﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Roles.Models;

namespace TASVideos.Pages.Roles;

[RequirePermission(PermissionTo.EditRoles)]
public class AddEditModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly IRoleService _roleService;
	private readonly ExternalMediaPublisher _publisher;

	public AddEditModel(
		ApplicationDbContext db,
		IRoleService roleService,
		ExternalMediaPublisher publisher)
	{
		_db = db;
		_roleService = roleService;
		_publisher = publisher;
	}

	[FromRoute]
	public int? Id { get; set; }

	[FromQuery]
	public int? CopyFrom { get; set; }

	public bool IsInUse { get; set; }

	[BindProperty]
	public RoleEditModel Role { get; set; } = new();

	[Display(Name = "Available Permissions")]
	public IEnumerable<SelectListItem> AvailablePermissions => PermissionsSelectList;

	[Display(Name = "Available Assignable Permissions")]
	public IEnumerable<SelectListItem> AvailableAssignablePermissions { get; set; } = new List<SelectListItem>();

	private static IEnumerable<SelectListItem> PermissionsSelectList =>
		Enum.GetValues(typeof(PermissionTo))
			.Cast<PermissionTo>()
			.Select(p => new SelectListItem
			{
				Value = ((int)p).ToString(),
				Text = p.EnumDisplayName()
			})
			.OrderBy(s => s.Text)
			.ToList();

	public async Task<IActionResult> OnGet()
	{
		if (Id.HasValue)
		{
			var role = await _db.Roles
				.Where(r => r.Id == Id.Value)
				.ToRoleEditModel()
				.SingleOrDefaultAsync();

			if (role is null)
			{
				return NotFound();
			}

			Role = role;
			IsInUse = !await _roleService.IsInUse(Id.Value);
			SetAvailableAssignablePermissions();
		}
		else
		{
			if (CopyFrom.HasValue)
			{
				var role = await _db.Roles
					.Where(r => r.Id == CopyFrom.Value)
					.ToRoleEditModel()
					.SingleOrDefaultAsync();
				if (role is not null)
				{
					role.Name += " (Copied From)";
					Role = role;
				}
			}

			IsInUse = false;
		}

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			SetAvailableAssignablePermissions();
			return Page();
		}

		Role.Links = Role.Links.Where(l => !string.IsNullOrWhiteSpace(l));
		if (!ModelState.IsValid)
		{
			AvailableAssignablePermissions = Role.SelectedPermissions
			.Select(sp => new SelectListItem
			{
				Text = ((PermissionTo)sp).ToString(),
				Value = sp.ToString()
			});
			return Page();
		}

		await AddUpdateRole(Role);
		await ConcurrentSave(_db, $"Role {Id} updated", $"Unable to update Role {Id}");

		return BasePageRedirect("List");
	}

	public async Task<IActionResult> OnPostDelete()
	{
		if (!Id.HasValue)
		{
			return NotFound();
		}

		if (!User.Has(PermissionTo.DeleteRoles))
		{
			return AccessDenied();
		}

		if (await _roleService.IsInUse(Id.Value))
		{
			ErrorStatusMessage($"Role {Id} cannot be deleted because it is in use by at least 1 user");
			return BasePageRedirect("List");
		}

		_db.Roles.Attach(new Role { Id = Id.Value }).State = EntityState.Deleted;

		var result = await ConcurrentSave(_db, $"Role {Id} deleted", $"Unable to delete Role {Id}");
		if (result)
		{
			await _publisher.SendUserManagement(
				$"Role {Id} deleted by {User.Name()}",
				"",
				"",
				"");
		}

		return BasePageRedirect("List");
	}

	public async Task<IActionResult> OnGetRolesThatCanBeAssignedBy(int[] ids)
	{
		var result = await _roleService.GetRolesThatCanBeAssignedBy(ids.Select(p => (PermissionTo)p));
		return new JsonResult(result);
	}

	private void SetAvailableAssignablePermissions()
	{
		AvailableAssignablePermissions = Role.SelectedPermissions
			.Select(sp => new SelectListItem
			{
				Text = ((PermissionTo)sp).ToString(),
				Value = sp.ToString()
			}).OrderBy(sp => sp.Text);
	}

	private async Task AddUpdateRole(RoleEditModel model)
	{
		Role role;
		if (Id.HasValue)
		{
			role = await _db.Roles.SingleAsync(r => r.Id == Id);
			_db.RolePermission.RemoveRange(_db.RolePermission.Where(rp => rp.RoleId == Id));
			_db.RoleLinks.RemoveRange(_db.RoleLinks.Where(rp => rp.Role!.Id == Id));
			await _db.SaveChangesAsync();

			await _publisher.SendUserManagement(
				$"Role {model.Name} updated by {User.Name()}",
				$"Role [{model.Name}]({{0}}) updated by {User.Name()}",
				"",
				$"Roles/{model.Name}");
		}
		else
		{
			role = new Role();
			_db.Roles.Attach(role);
			await _publisher.SendUserManagement(
				$"New Role {model.Name} added by {User.Name()}",
				$"New Role [{model.Name}]({{0}}) added by {User.Name()}",
				"",
				$"Roles/{model.Name}");
		}

		role.Name = model.Name;
		role.IsDefault = model.IsDefault;
		role.Description = model.Description;
		role.AutoAssignPostCount = model.AutoAssignPostCount;
		role.AutoAssignPublications = model.AutoAssignPublications;

		await _db.RolePermission.AddRangeAsync(model.SelectedPermissions
			.Select(p => new RolePermission
			{
				RoleId = role.Id,
				PermissionId = (PermissionTo)p,
				CanAssign = model.SelectedAssignablePermissions.Contains(p)
			}));

		await _db.RoleLinks.AddRangeAsync(model.Links.Select(rl => new RoleLink
		{
			Link = rl,
			Role = role
		}));
	}
}
