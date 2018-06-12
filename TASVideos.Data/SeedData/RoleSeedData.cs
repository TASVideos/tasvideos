using System;
using System.Collections.Generic;
using System.Linq;
using TASVideos.Data.Entity;

// ReSharper disable StyleCop.SA1202
// ReSharper disable StaticMemberInitializerReferesToMemberBelow
namespace TASVideos.Data.SeedData
{
	public static class SeedRoleNames
	{
		public const string EditHomePage = "Edit Home Page";
		public const string SubmitMovies = "Submit Movies";
		public const string ForumUser = "Forum User";
		public const string ExperiencedForumUser = "Experienced Forum User";

		public const string Admin = "Site Admin";
		public const string AdminAssistant = "Admin Assistant";
		public const string Editor = "Editor";
		public const string VestedEditor = "Vested Editor";
		public const string Judge = "Judge";
		public const string SeniorJudge = "SeniorJudge";
		public const string Encoder = "Encoder";
		public const string Publisher = "Publisher";
		public const string SeniorPublisher = "Senior Publisher";
		public const string ForumModerator = "Forum Moderator";
	}

	public class RoleSeedData
	{
		private static readonly PermissionTo[] EditorPermissions =
		{
			PermissionTo.EditWikiPages,
			PermissionTo.EditGameResources
		};

		private static readonly PermissionTo[] VestedEditor = EditorPermissions.Concat(new[]
		{
			PermissionTo.EditSystemPages,
			PermissionTo.EditUsers,
			PermissionTo.AssignRoles,
			PermissionTo.MoveWikiPages,
			PermissionTo.EditPublicationMetaData
		}).ToArray();

		private static readonly PermissionTo[] JudgePermissions =
		{
			PermissionTo.EditSubmissions,
			PermissionTo.JudgeSubmissions,
			PermissionTo.ReplaceSubmissionMovieFile,
			PermissionTo.CatalogMovies,
			PermissionTo.EditPublicationMetaData
		};

		private static readonly PermissionTo[] SeniorJudgePermissions = JudgePermissions.Concat(new[]
		{
			PermissionTo.AssignRoles,
			PermissionTo.OverrideSubmissionStatus
		}).ToArray();

		private static readonly PermissionTo[] PublisherPermissions =
		{
			PermissionTo.PublishMovies,
			PermissionTo.CatalogMovies,
			PermissionTo.EditSubmissions,
			PermissionTo.EditPublicationMetaData
		};

		private static readonly PermissionTo[] SeniorPublisherPermissions =
		{
			PermissionTo.AssignRoles
		};

		private static readonly PermissionTo[] AdminAssistantPermissions = VestedEditor.Concat(new[]
		{
			PermissionTo.CatalogMovies,
			PermissionTo.EditRoles
		}).ToArray();

		private static readonly PermissionTo[] ModeratorPermissions =
		{
			PermissionTo.EditForumPosts,
			PermissionTo.LockTopics
		};

		public static readonly Role EditHomePage = new Role
		{
			IsDefault = true,
			Name = SeedRoleNames.EditHomePage,
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
			IsDefault = true,
			Name = SeedRoleNames.SubmitMovies,
			Description = "Contains the SubmitMovies permission that allows users to submit movies. All users have this role by default.",
			RolePermission = new[]
			{
				new RolePermission
				{
					Role = SubmitMovies,
					PermissionId = PermissionTo.SubmitMovies
				}
			}
		};

		public static readonly Role ForumUser = new Role
		{
			IsDefault = true,
			Name = SeedRoleNames.ForumUser,
			Description = "Contains the CreateForumPosts permission that allows users to creat forum posts. All users have this role by default.",
			RolePermission = new[]
			{
				new RolePermission
				{
					Role = ForumUser,
					PermissionId = PermissionTo.CreateForumPosts
				}
			}
		};

		public static readonly Role ExperiencedForumUser = new Role
		{
			IsDefault = false,
			Name = SeedRoleNames.ExperiencedForumUser,
			Description = "Contains the CreateForumTopics and VoteInPolls permissions that allow users to create topics and participate in forum polls. This role is automatically applied to experienced users.",
			RolePermission = new[]
			{
				new RolePermission
				{
					Role = ExperiencedForumUser,
					PermissionId = PermissionTo.CreateForumTopics
				},
				new RolePermission
				{
					Role = ExperiencedForumUser,
					PermissionId = PermissionTo.VoteInPolls
				},
			}
		};

		public static readonly Role AdminRole = new Role
		{
			Name = SeedRoleNames.Admin,
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
				Name = SeedRoleNames.AdminAssistant,
				Description = "This is a wiki editor that maintain many aspects of the wiki site, including user and role maintenance, and they can assign editors as well as vested editors",
				RolePermission = AdminAssistantPermissions.Select(p => new RolePermission
				{
					RoleId = 13, // Meh, for lack of a better way
					PermissionId = p,
					CanAssign = VestedEditor.Contains(p)
				}).ToArray(),
				RoleLinks = new[]
				{
					new RoleLink { Link = "EditorGuidelines" },
					new RoleLink { Link = "TextFormattingRules" }
				}
			},
			new Role
			{
				Name = SeedRoleNames.Editor,
				Description = "This is a wiki editor that can edit basic wiki pages",
				RolePermission = EditorPermissions.Select(p => new RolePermission
				{
					RoleId = 12, // Meh, for lack of a better way
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
				Name = SeedRoleNames.VestedEditor,
				Description = "This is a wiki editor that can edit any wiki page, including system pages",
				RolePermission = VestedEditor.Select(p => new RolePermission
				{
					RoleId = 11, // Meh, for lack of a better way
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
				Name = SeedRoleNames.Judge,
				Description = "The judges decide which movies will be published and which movies are rejected. They can replace submission files.",
				RolePermission = JudgePermissions.Select(p => new RolePermission
				{
					RoleId = 10, // Meh, for lack of a better way
					PermissionId = p
				}).ToArray(),
				RoleLinks = new[]
				{
					new RoleLink { Link = "JudgeGuidelines" }
				}
			},
			new Role
			{
				Name = SeedRoleNames.SeniorJudge,
				Description = "The senior judge, in addition to judging movies, can assign judges and settle disputes among judges.",
				RolePermission = SeniorJudgePermissions.Select(p => new RolePermission
				{
					RoleId = 9, // Meh, for lack of a better way
					PermissionId = p,
					CanAssign = EditorPermissions.Contains(p)
				}).ToArray(),
				RoleLinks = new[]
				{
					new RoleLink { Link = "JudgeGuidelines" }
				}
			},
			new Role
			{
				Name = SeedRoleNames.Publisher,
				Description = "Publishers take accepted submissions and turn them into publications.",
				RolePermission = PublisherPermissions.Select(p => new RolePermission
				{
					RoleId = 8, // Meh, for lack of a better way
					PermissionId = p
				}).ToArray(),
				RoleLinks = new[] { new RoleLink { Link = "PublisherGuidelines" } }
			},
			new Role
			{
				Name = SeedRoleNames.SeniorPublisher,
				Description = "Senior Publishers, in addition to publishing movies, can assign publishers, set encoding standards, and settle disputes among publishers.",
				RolePermission = SeniorPublisherPermissions.Select(p => new RolePermission
				{
					RoleId = 7, // Meh, for lack of a better way
					PermissionId = p,
					CanAssign = EditorPermissions.Contains(p)
				}).ToArray(),
				RoleLinks = new[] { new RoleLink { Link = "PublisherGuidelines" } }
			},
			new Role
			{
				Name = SeedRoleNames.ForumModerator,
				Description = "Forum Moderators monitor forum content, modify posts and lock topics if needed. Moderators can also take limited action against users on a need basis",
				RolePermission = ModeratorPermissions.Select(p => new RolePermission
				{
					RoleId = 6, // Meh, for lack of a better
					PermissionId = p,
					CanAssign = false
				}).ToArray(),
				RoleLinks = new List<RoleLink>()
			}
		};
	}
}
