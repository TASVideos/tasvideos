namespace TASVideos.Pages.Roles;

[RequirePermission(PermissionTo.EditRoles)]
public class AddEditModel(ApplicationDbContext db, IRoleService roleService, IExternalMediaPublisher publisher) : BasePageModel
{
	[FromRoute]
	public int? Id { get; set; }

	[FromQuery]
	public int? CopyFrom { get; set; }

	public bool IsInUse { get; set; }

	[BindProperty]
	public RoleEdit Role { get; set; } = new();

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

		Role.RelatedLinks = Role.RelatedLinks.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
		if (!ModelState.IsValid)
		{
			AvailableAssignablePermissions = Role.SelectedPermissions
				.Cast<PermissionTo>()
				.ToDropDown()
				.ToList();

			return Page();
		}

		await AddUpdateRole(Role);
		SetMessage(await db.TrySaveChanges(), $"Role {Id} updated", $"Unable to update Role {Id}");

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
		var result = await db.TrySaveChanges();
		SetMessage(result, $"Role {Id} deleted", $"Unable to delete Role {Id}");
		if (result.IsSuccess())
		{
			await publisher.SendAdminMessage(PostGroups.UserManagement, $"Role {Id} deleted by {User.Name()}");
		}

		return BasePageRedirect("List");
	}

	public async Task<IActionResult> OnGetRolesThatCanBeAssignedBy(int[] ids)
	{
		var result = await roleService.GetRolesThatCanBeAssignedBy(ids.Select(p => (PermissionTo)p));
		return Json(result);
	}

	private void SetAvailableAssignablePermissions()
	{
		AvailableAssignablePermissions = [.. Role.SelectedPermissions
			.Cast<PermissionTo>()
			.ToDropDown()
			.OrderBy(sp => sp.Text)
		];
	}

	private async Task AddUpdateRole(RoleEdit model)
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

		role.RoleLinks.AddRange(model.RelatedLinks.Select(rl => new RoleLink
		{
			Link = rl,
			Role = role
		}));

		var action = edit ? "updated" : "added";
		await publisher.SendRoleManagement($"Role [{model.Name}]({{0}}) {action} by {User.Name()}", model.Name);
	}

	public class RoleEdit
	{
		[StringLength(50)]
		public string Name { get; set; } = "";
		public bool IsDefault { get; init; }

		[StringLength(300)]
		public string Description { get; init; } = "";

		[Range(1, 9999)]
		public int? AutoAssignPostCount { get; init; }
		public bool AutoAssignPublications { get; init; }

		[MinLength(1)]
		public List<int> SelectedPermissions { get; init; } = [];
		public List<int> SelectedAssignablePermissions { get; init; } = [];
		public List<string> RelatedLinks { get; set; } = [];
	}
}
