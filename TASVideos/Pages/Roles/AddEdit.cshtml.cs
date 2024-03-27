using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Roles;

[RequirePermission(PermissionTo.EditRoles)]
public class AddEditModel(
	ApplicationDbContext db,
	IRoleService roleService,
	ExternalMediaPublisher publisher)
	: BasePageModel
{
	[FromRoute]
	public int? Id { get; set; }

	[FromQuery]
	public int? CopyFrom { get; set; }

	public bool IsInUse { get; set; }

	[BindProperty]
	public RoleEditModel Role { get; set; } = new();

	public List<SelectListItem> AvailableAssignablePermissions { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		if (Id.HasValue)
		{
			var role = await db.Roles
				.Where(r => r.Id == Id.Value)
				.ToRoleEditModel()
				.SingleOrDefaultAsync();

			if (role is null)
			{
				return NotFound();
			}

			Role = role;
			IsInUse = !await roleService.IsInUse(Id.Value);
			SetAvailableAssignablePermissions();
		}
		else
		{
			if (CopyFrom.HasValue)
			{
				var role = await db.Roles
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

		Role.Links = Role.Links.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
		if (!ModelState.IsValid)
		{
			AvailableAssignablePermissions = Role.SelectedPermissions
			.Select(sp => new SelectListItem
			{
				Text = ((PermissionTo)sp).ToString(),
				Value = sp.ToString()
			})
			.ToList();
			return Page();
		}

		await AddUpdateRole(Role);
		await ConcurrentSave(db, $"Role {Id} updated", $"Unable to update Role {Id}");

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

		if (await roleService.IsInUse(Id.Value))
		{
			ErrorStatusMessage($"Role {Id} cannot be deleted because it is in use by at least 1 user");
			return BasePageRedirect("List");
		}

		db.Roles.Attach(new Role { Id = Id.Value }).State = EntityState.Deleted;

		var result = await ConcurrentSave(db, $"Role {Id} deleted", $"Unable to delete Role {Id}");
		if (result)
		{
			await publisher.SendUserManagement(
				$"Role {Id} deleted by {User.Name()}", "", "", "");
		}

		return BasePageRedirect("List");
	}

	public async Task<IActionResult> OnGetRolesThatCanBeAssignedBy(int[] ids)
	{
		var result = await roleService.GetRolesThatCanBeAssignedBy(ids.Select(p => (PermissionTo)p));
		return new JsonResult(result);
	}

	private void SetAvailableAssignablePermissions()
	{
		AvailableAssignablePermissions = [.. Role.SelectedPermissions
			.Select(sp => new SelectListItem
			{
				Text = ((PermissionTo)sp).ToString(),
				Value = sp.ToString()
			}).OrderBy(sp => sp.Text)];
	}

	private async Task AddUpdateRole(RoleEditModel model)
	{
		var edit = false;
		Role role;
		if (Id.HasValue)
		{
			edit = true;
			role = await db.Roles
				.Include(r => r.RolePermission)
				.Include(r => r.RoleLinks)
				.SingleAsync(r => r.Id == Id);
			db.RolePermission.RemoveRange(db.RolePermission.Where(rp => rp.RoleId == Id));
			db.RoleLinks.RemoveRange(db.RoleLinks.Where(rp => rp.Role!.Id == Id));
			await db.SaveChangesAsync();
		}
		else
		{
			role = new Role();
			db.Roles.Attach(role);
		}

		role.Name = model.Name;
		role.IsDefault = model.IsDefault;
		role.Description = model.Description;
		role.AutoAssignPostCount = model.AutoAssignPostCount;
		role.AutoAssignPublications = model.AutoAssignPublications;

		role.RolePermission.AddRange(model.SelectedPermissions
			.Select(p => new RolePermission
			{
				RoleId = role.Id,
				PermissionId = (PermissionTo)p,
				CanAssign = model.SelectedAssignablePermissions.Contains(p)
			}));

		role.RoleLinks.AddRange(model.Links.Select(rl => new RoleLink
		{
			Link = rl,
			Role = role
		}));

		if (edit)
		{
			await publisher.SendUserManagement(
				$"Role {model.Name} updated by {User.Name()}",
				$"Role [{model.Name}]({{0}}) updated by {User.Name()}",
				"",
				$"Roles/{model.Name}");
		}
		else
		{
			await publisher.SendUserManagement(
				$"New Role {model.Name} added by {User.Name()}",
				$"New Role [{model.Name}]({{0}}) added by {User.Name()}",
				"",
				$"Roles/{model.Name}");
		}
	}

	public class RoleEditModel
	{
		[StringLength(50)]
		public string Name { get; set; } = "";

		[Display(Name = "Default", Description = "Default roles are given to all new users when they register")]
		public bool IsDefault { get; init; }

		[StringLength(300)]
		public string Description { get; init; } = "";

		[Range(1, 9999)]
		[Display(Name = "Auto-assign on Post Count", Description = "If set, the user will automatically be assigned this role when they reach this post count.")]
		public int? AutoAssignPostCount { get; init; }

		[Display(Name = "Auto-assign on Publication", Description = "If set, the user will automatically be assigned this role when they have a movie published.")]
		public bool AutoAssignPublications { get; init; }

		[MinLength(1)]
		public List<int> SelectedPermissions { get; init; } = [];

		public List<int> SelectedAssignablePermissions { get; init; } = [];

		[Display(Name = "Related Links")]
		public List<string> Links { get; set; } = [];
	}
}
