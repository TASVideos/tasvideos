using System;
using System.Linq;
using TASVideos.Data.Entity;

// ReSharper disable StyleCop.SA1202
namespace TASVideos.Data.SeedData
{
	public class RoleSeedData
	{
		private static readonly PermissionTo[] EditorPermissions =
		{
			PermissionTo.EditWikiPages,
			PermissionTo.EditGameResources
		};

		private static readonly PermissionTo[] SeniorEditorPermissions = EditorPermissions.Concat(new[]
		{
			PermissionTo.EditSystemPages,
			PermissionTo.EditUsers,
			PermissionTo.AssignRoles,
			PermissionTo.MoveWikiPages
		}).ToArray();

		private static readonly PermissionTo[] JudgePermissions =
		{
			PermissionTo.EditSubmissions,
			PermissionTo.JudgeSubmissions,
			PermissionTo.ReplaceSubmissionMovieFile
		};

		public static readonly Role EditHomePage = new Role
		{
			Name = "Edit Home Page",
			Description = "Contains the EditHomePage permission that allows users to edit their personal homepage. All users have this role by default.",
			RolePermission = new[]
			{
				new RolePermission
				{
					Role = EditHomePage,
					PermissionId = PermissionTo.EditHomePage
				}
			}
		};

		public static readonly Role SubmitMovies = new Role
		{
			Name = "Submit Movies",
			Description = "Contains the SubmitMovies permission that allows users to submit movies. All users have this role by default.",
			RolePermission = new[]
			{
				new RolePermission
				{
					Role = EditHomePage,
					PermissionId = PermissionTo.SubmitMovies
				}
			}
		};

		public static readonly Role AdminRole = new Role
		{
			Name = "Site Admin",
			Description = "This is a site administrator that is responsible for maintaining TASVideos",
			RolePermission = Enum.GetValues(typeof(PermissionTo))
				.Cast<PermissionTo>()
				.Select(p => new RolePermission
				{
					Role = AdminRole,
					PermissionId = p,
					CanAssign = true
				})
				.ToArray()
		};

		public static readonly Role[] Roles =
		{
			new Role
			{
				Name = "Editor",
				Description = "This is a wiki editor that can edit basic wiki pages",
				RolePermission = EditorPermissions.Select(p => new RolePermission
				{
					RoleId = 2,
					PermissionId = p
				}).ToArray(),
				RoleLinks = new[]
				{
					new RoleLink { Link = "EditorGuidelines" },
					new RoleLink { Link = "TextFormattingRules" }
				}
			},
			new Role
			{
				Name = "Senior Editor",
				Description = "This is a wiki editor that can edit any wiki page, including system pages",
				RolePermission = SeniorEditorPermissions.Select(p => new RolePermission
				{
					RoleId = 3, // Meh, for lack of a better way
					PermissionId = p,
					CanAssign = EditorPermissions.Contains(p)
				}).ToArray(),
				RoleLinks = new[]
				{
					new RoleLink { Link = "EditorGuidelines" },
					new RoleLink { Link = "TextFormattingRules" }
				}
			},
			new Role
			{
				Name = "Judge",
				Description = "The judges decide which movies will be published and which movies are rejected. They can replace submission files.",
				RolePermission = JudgePermissions.Select(p => new RolePermission
				{
					RoleId = 4, // Meh, for lack of a better way
					PermissionId = p
				}).ToArray(),
				RoleLinks = new[]
				{
					new RoleLink { Link = "JudgeGuidelines" }
				}
			}
		};
	}
}
