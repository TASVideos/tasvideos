using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TASVideos.Data.Attributes;

namespace TASVideos.Data.Entity
{
	/// <summary>
	/// Represents the most granular level permissions possible in the site. All site code is based on these permissions
	/// The <see cref="Role" /> table represents a group of permissions that can be assigned to a <seealso cref="User"/>
	/// </summary>
	public enum PermissionTo
	{
		#region User 1-99

		[Display(Name = "Create Forum Posts")]
		[Group("User")]
		[Description("The ability to post on the forums. By default, all registered users have this permission, unless revoked.")]
		CreateForumPosts = 1,

		[Display(Name = "Edit Home Page")]
		[Group("User")]
		[Description("The ability to edit a personal homepage. By default, all registered users have this permission, unless revoked.")]
		EditHomePage = 2,

		[Display(Name = "Submit Movies")]
		[Group("User")]
		[Description("The ability to submit a movie to be considered for publication. By default, all registered users have this permission, unless revoked.")]
		SubmitMovies = 3,

		[Display(Name = "Create Forum Topics")]
		[Group("User")]
		[Description("The ability to create new topics on the forums. Experienced users have the ability to do this, unless revoked")]
		CreateForumTopics = 10,

		[Display(Name = "Vote In Polls")]
		[Group("User")]
		[Description("The ability to participate in forum polls. Experienced users have the ability ot do this, unless revoked.")]
		VoteInPolls = 11,

		#endregion

		#region Wiki 100

		[Display(Name = "Edit Wiki Pages")]
		[Group("Wiki")]
		[Description("The ability to edit basic wiki pages. This is the most basic editor privilege but some pages may be restrited to other privileges.")]
		EditWikiPages = 100,

		[Display(Name = "Edit Game Resources")]
		[Group("Wiki")]
		[Description("The ability to edit Game Resource wiki pages. These are basic game information and are considered separate from general wiki pages.")]
		EditGameResources = 101,

		[Display(Name = "Edit System Pages")]
		[Group("Wiki")]
		[Description("The ability to edit System Wiki pages. These pages are more fundamental to the site behavior whan basic wiki pages.")]
		EditSystemPages = 102,

		[Display(Name = "Edit Roles")]
		[Group("Wiki")]
		[Description("The ability to create and edit roles and add/remove privileges to those roles.")]
		EditRoles = 103,

		[Display(Name = "Movie Wiki Pages")]
		[Group("Wiki")]
		[Description("The ability to movie a wiki page to another location")]
		MoveWikiPages = 104,

		[Display(Name = "Delete Wiki Pages")]
		[Group("Wiki")]
		[Description("The ability to delete a wiki page")]
		DeleteWikiPages = 105,

		[Display(Name = "See Admin Pages")]
		[Group("Wiki Administration")]
		[Description("The ability to see certain restricted pages that pertain to administration activities.")]
		SeeAdminPages = 190,

		#endregion

		#region Queue Maintenance 200

		[Display(Name = "Judge Movies")]
		[Group("Queue Maintenance")]
		[Description("The ability to judge submissions in the submission queue")]
		JudgeSubmissions = 200,

		[Display(Name = "Publish Movies")]
		[Group("Queue Maintenance")]
		[Description("The ability to publish movies")]
		PublishMovies = 201,

		[Display(Name = "Edit Submissions")]
		[Group("Queue Maintenance")]
		[Description("The ability to edit submission text and basic metadata. Changing statuses, editing movie files, etc. are further restricted by other permissions")]
		EditSubmissions = 202,

		[Display(Name = "Replace Submission Movie File")]
		[Group("Queue Maintenance")]
		[Description("The ability to replace the movie file of an existing un-published submission")]
		ReplaceSubmissionMovieFile = 203,

		[Display(Name = "Override Submissison Status")]
		[Group("Queue Maintenance")]
		[Description("The ability to set a submission status regardless of condition (exception: published submissions)")]
		OverrideSubmissionStatus = 204,

		#endregion

		#region Publication Maintenance 300

		[Display(Name = "Set Tier")]
		[Group("Publication Maintenance")]
		[Description("The ability to set a published movie's movie tier")]
		SetTier = 300,

		[Display(Name = "Catalog Movies")]
		[Group("Publication Maintenance")]
		[Description("The ability to edit the game, system, and rom information for movies")]
		CatalogMovies = 301,

		[Display(Name = "Edit Publication Metadata")]
		[Group("Publication Maintenance")]
		[Description("The ability to edit publication information such as branch, screenshots, torrents, tags, etc")]
		EditPublicationMetaData = 302,

		[Display(Name = "Display Private Ratings")]
		[Group("Publication Maintenance")]
		[Description("The ability to see publication ratings even from users with non-public ratings")]
		SeePrivateRatings = 303,

		[Display(Name = "Edit recommendation flag")]
		[Group("Publication Maintenance")]
		[Description("The ability to edit a movie's recomended flag, which flags the movie as recommended to new comers")]
		EditRecommendation = 304,

		#endregion

		#region Forum Moderation 400

		[Display(Name = "Edit Forum Posts")]
		[Group("Forum Moderation")]
		[Description("The ability to edit post created by another user.")]
		EditForumPosts = 400,

		[Display(Name = "Lock Topics")]
		[Group("Forum Administration")]
		[Description("The ability to lock and unlock forum topics.")]
		LockTopics = 401,

		[Display(Name = "Post In Locked Topics")]
		[Group("Forum Administration")]
		[Description("The ability to create a post in a topic that is currently locked.")]
		PostInLockedTopics = 402,

		[Display(Name = "See Restricted Froums")]
		[Group("Forum Administration")]
		[Description("The ability to see forums that are restricted from general access.")]
		SeeRestrictedForums = 490,

		[Display(Name = "See Poll Results")]
		[Group("Forum Administration")]
		[Description("The ability to see which users voted on which poll option.")]
		SeePollResults = 491,

		#endregion

		#region User Administration 500

		[Display(Name = "Delete Roles")]
		[Group("User Administration")]
		[Description("The ability to delete an existing role.")]
		DeleteRoles = 500,

		[Display(Name = "View Users")]
		[Group("User Administration")]
		[Description("The ability to see other user's profile data in read-only form.")]
		ViewUsers = 501,

		[Display(Name = "Edit Users")]
		[Group("User Administration")]
		[Description("The ability to edit basic information about another user.")]
		EditUsers = 502,

		[Display(Name = "Edit Users UserName")]
		[Group("User Administration")]
		[Description("The ability to change another user's UserName. Users with this permission should also have the EditUsers permission.")]
		EditUsersUserName = 503,

		[Display(Name = "Assign Roles")]
		[Group("User Administration")]
		[Description("The ability to assign Roles to any User with some restrictions. A role can only be assigned if all permissions within it are marked as assignable by a role the user has.")]
		AssignRoles = 504,

		#endregion

		#region Admin 9001

		[Display(Name = "See Diagnostics")]
		[Group("Admin")]
		[Description("The ability to see high level application and server information such as diagnostics stats and other sensitive information")]
		SeeDiagnostics = 9001

		#endregion
	}
}
