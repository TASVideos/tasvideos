using System;
using System.Collections.Generic;
using System.Linq;
using TASVideos.Data.Entity;

// ReSharper disable StyleCop.SA1202
// ReSharper disable StaticMemberInitializerReferesToMemberBelow
namespace TASVideos.Data.SeedData
{
	public static class RoleSeedNames
	{
		public const string DefaultUser = "Default User";
		public const string LimitedUser = "Limited User";
		public const string ExperiencedForumUser = "Experienced Forum User";

		public const string Admin = "Site Admin";
		public const string AdminAssistant = "Admin Assistant";
		public const string Editor = "Editor";
		public const string VestedEditor = "Vested Editor";
		public const string Judge = "Judge";
		public const string SeniorJudge = "Senior Judge";
		public const string Encoder = "Encoder";
		public const string Publisher = "Publisher";
		public const string SeniorPublisher = "Senior Publisher";
		public const string ForumModerator = "Forum Moderator";
		public const string ForumAdmin = "Forum Admin";
		public const string Ambassador = "Ambassador";
		public const string SiteDeveloper = "Site Developer";
		public const string EmulatorCoder = "Emulator Coder";
	}

	public class RoleSeedData
	{
		private static readonly PermissionTo[] DefaultUserPermissions =
		{
			PermissionTo.RateMovies,
			PermissionTo.EditHomePage,
			PermissionTo.SubmitMovies,
			PermissionTo.CreateForumPosts
		};

		private static readonly PermissionTo[] EditorPermissions =
		{
			PermissionTo.EditWikiPages,
			PermissionTo.EditGameResources
		};

		private static readonly PermissionTo[] VestedEditorPermissions = EditorPermissions.Concat(new[]
		{
			PermissionTo.SeeDeletedWikiPages,
			PermissionTo.DeleteWikiPages,
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
			PermissionTo.EditPublicationMetaData,
			PermissionTo.SeeRestrictedForums
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
			PermissionTo.EditPublicationMetaData,
			PermissionTo.SeeRestrictedForums
		};

		private static readonly PermissionTo[] SeniorPublisherPermissions = PublisherPermissions.Concat(new[]
		{
			PermissionTo.AssignRoles
		}).ToArray();

		private static readonly PermissionTo[] AdminAssistantPermissions = VestedEditorPermissions.Concat(new[]
		{
			PermissionTo.CatalogMovies,
			PermissionTo.EditRoles,
			PermissionTo.SeeRestrictedForums
		}).ToArray();

		private static readonly PermissionTo[] ForumModeratorPermissions =
		{
			PermissionTo.EditForumPosts,
			PermissionTo.LockTopics,
			PermissionTo.MoveTopics
		};

		private static readonly PermissionTo[] ForumAdminPermissions = ForumModeratorPermissions.Concat(new[]
		{
			PermissionTo.SeeRestrictedForums,
			PermissionTo.EditForums,
			PermissionTo.EditCategories,
			PermissionTo.DeleteForumPosts,
			PermissionTo.CreateForumPolls,
			PermissionTo.ResetPollResults
		}).ToArray();

		private static readonly PermissionTo[] AmbassadorPermissions =
		{
			PermissionTo.SeeRestrictedForums,
			PermissionTo.EditWikiPages,
			PermissionTo.EditSystemPages
		};

		private static readonly PermissionTo[] SiteDeveloperPermissions =
		{
			PermissionTo.SeeDiagnostics
		};

		private static readonly List<RoleLink> SiteDeveloperLinks = new List<RoleLink>
		{
			new RoleLink { Link = "SiteCodingStandards" }
		};

		private static readonly List<RoleLink> AdminRoleLinks = new List<RoleLink>
		{
			new RoleLink { Link = "AdminGuidelines" }
		};

		public static readonly Role DefaultUser = new Role
		{
			IsDefault = true,
			Name = RoleSeedNames.DefaultUser,
			Description = "Contains basic permissions that all new users receive upon registration. These permissions include posting in forums, submitting and rating movies, and editing a personal homepage.",
			RolePermission = DefaultUserPermissions.Select(p => new RolePermission
			{
				Role = DefaultUser,
				PermissionId = p
			}).ToArray()
		};

		public static readonly Role LimitedUser = new Role
		{
			IsDefault = false,
			Name = RoleSeedNames.LimitedUser,
			Description = "Contains all the basic permissions of a default user except the ability to submit movies. This permission is given when a user has abused the submission system but has otherwise not violated site rules.",
			RolePermission = DefaultUserPermissions
				.Where(p => p != PermissionTo.SubmitMovies)
				.Select(p => new RolePermission
				{
					Role = LimitedUser,
					PermissionId = p
				}).ToArray()
		};

		public static readonly Role ExperiencedForumUser = new Role
		{
			IsDefault = false,
			Name = RoleSeedNames.ExperiencedForumUser,
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

		public static readonly Role Admin = new Role
		{
			Name = RoleSeedNames.Admin,
			Description = "This is a site administrator that is responsible for maintaining TASVideos",
			RolePermission = Enum.GetValues(typeof(PermissionTo))
				.Cast<PermissionTo>()
				.Select(p => new RolePermission
				{
					Role = Admin,
					PermissionId = p,
					CanAssign = true
				})
				.ToArray(),
			RoleLinks = AdminRoleLinks
		};

		public static readonly Role AdminAssistant = new Role
		{
			Name = RoleSeedNames.AdminAssistant,
			Description = "This is a wiki editor that maintain many aspects of the wiki site, including user and role maintenance, and they can assign editors as well as vested editors",
			RolePermission = AdminAssistantPermissions.Select(p => new RolePermission
			{
				Role = AdminAssistant, // Meh, for lack of a better way
				PermissionId = p,
				CanAssign = VestedEditorPermissions.Contains(p)
			}).ToArray(),
			RoleLinks = AdminRoleLinks.Concat(new[]
			{
				new RoleLink { Link = "EditorGuidelines" },
				new RoleLink { Link = "TextFormattingRules" }
			}).ToList(),
		};

		public static readonly Role Editor = new Role
		{
			Name = RoleSeedNames.Editor,
			Description = "This is a wiki editor that can edit basic wiki pages",
			RolePermission = EditorPermissions.Select(p => new RolePermission
			{
				Role = Editor,
				PermissionId = p
			}).ToArray(),
			RoleLinks = new[]
			{
				new RoleLink { Link = "EditorGuidelines" },
				new RoleLink { Link = "TextFormattingRules" }
			}
		};

		public static readonly Role VestedEditor = new Role
		{
			Name = RoleSeedNames.VestedEditor,
			Description = "This is a wiki editor that can edit any wiki page, including system pages",
			RolePermission = VestedEditorPermissions.Select(p => new RolePermission
			{
				Role = VestedEditor,
				PermissionId = p,
				CanAssign = EditorPermissions.Contains(p)
			}).ToArray(),
			RoleLinks = new[]
			{
				new RoleLink { Link = "EditorGuidelines" },
				new RoleLink { Link = "TextFormattingRules" }
			}
		};

		public static readonly Role Judge = new Role
		{
			Name = RoleSeedNames.Judge,
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
		};

		public static readonly Role SeniorJudge = new Role
		{
			Name = RoleSeedNames.SeniorJudge,
			Description = "The senior judge, in addition to judging movies, can assign judges and settle disputes among judges.",
			RolePermission = SeniorJudgePermissions.Select(p => new RolePermission
			{
				Role = SeniorJudge,
				PermissionId = p,
				CanAssign = EditorPermissions.Contains(p)
			}).ToArray(),
			RoleLinks = AdminRoleLinks.Concat(new[]
			{
				new RoleLink { Link = "JudgeGuidelines" }
			}).ToList(),
			
		};

		public static readonly Role Publisher = new Role
		{
			Name = RoleSeedNames.Publisher,
			Description = "Publishers take accepted submissions and turn them into publications.",
			RolePermission = PublisherPermissions.Select(p => new RolePermission
			{
				Role = Publisher,
				PermissionId = p
			}).ToArray(),
			RoleLinks = new[] { new RoleLink { Link = "PublisherGuidelines" } }
		};

		public static readonly Role SeniorPublisher = new Role
		{
			Name = RoleSeedNames.SeniorPublisher,
			Description = "Senior Publishers, in addition to publishing movies, can assign publishers, set encoding standards, and settle disputes among publishers.",
			RolePermission = SeniorPublisherPermissions.Select(p => new RolePermission
			{
				Role = SeniorPublisher,
				PermissionId = p,
				CanAssign = EditorPermissions.Contains(p)
			}).ToArray(),
			RoleLinks = AdminRoleLinks.Concat(new[] { new RoleLink { Link = "PublisherGuidelines" } }).ToList()
		};

		public static readonly Role ForumModerator = new Role
		{
			Name = RoleSeedNames.ForumModerator,
			Description = "Forum Moderators monitor forum content, modify posts and lock topics if needed. Moderators can also take limited action against users on a need basis",
			RolePermission = ForumModeratorPermissions.Select(p => new RolePermission
			{
				Role = ForumModerator,
				PermissionId = p,
				CanAssign = false
			}).ToArray(),
			RoleLinks = new List<RoleLink>()
		};

		public static readonly Role ForumAdmin = new Role
		{
			Name = RoleSeedNames.ForumAdmin,
			Description = "Form Administrators manage forums and permissions. Administrators can also assign Moderators.",
			RolePermission = ForumAdminPermissions.Select(p => new RolePermission
			{
				Role = ForumAdmin,
				PermissionId = p,
				CanAssign = ForumModeratorPermissions.Contains(p)
			}).ToArray(),
			RoleLinks = AdminRoleLinks
		};

		public static readonly Role Ambassador = new Role
		{
			Name = RoleSeedNames.Ambassador,
			Description = "Ambassadors take the burden on themselves to arrange various site related events and also visit game conferences and advertise the site. They handle various public relations issues and can edit most pages on the site.",
			RolePermission = AmbassadorPermissions.Select(p => new RolePermission
			{
				Role = Ambassador,
				PermissionId = p,
				CanAssign = false
			}).ToArray()
		};

		public static readonly Role SiteDeveloper = new Role
		{
			Name = RoleSeedNames.SiteDeveloper,
			Description = "Site developers have access to the site code and can make changes.",
			RolePermission = SiteDeveloperPermissions.Select(p => new RolePermission
			{
				Role = SiteDeveloper,
				PermissionId = p,
				CanAssign = false
			}).ToArray(),
			RoleLinks = SiteDeveloperLinks
		};

		public static readonly Role EmulatorCoder = new Role
		{
			Name = RoleSeedNames.EmulatorCoder,
			Description = "Emulator coders are people who have contributed code to site approved rerecording emulators.  This role is \"ceremonial\" and does not have any additional permissions."
		};

		public static IEnumerable<Role> AllRoles =>
			new[]
			{
				DefaultUser,
				LimitedUser,
				ExperiencedForumUser,
				Admin,
				AdminAssistant,
				Editor,
				VestedEditor,
				Judge,
				SeniorJudge,
				Publisher,
				SeniorPublisher,
				ForumModerator,
				ForumAdmin,
				Ambassador,
				SiteDeveloper,
				EmulatorCoder
			};
	}
}
