using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TASVideos.Attributes;

namespace TASVideos.Data.Entity
{
	/// <summary>
	/// Represents the most granular level permissions possible in the site. All site code is based on these permissions
	/// The <see cref="Role" /> table represents a group of permissions that can be assigned to a <seealso cref="User"/>.
	/// </summary>
	public enum PermissionTo
	{
		#region User 1-99

		[Group("User")]
		[Description("The ability to post on the forums. By default, all registered users have this permission, unless revoked.")]
		CreateForumPosts = 1,

		[Group("User")]
		[Description("The ability to edit a personal homepage. By default, all registered users have this permission, unless revoked.")]
		EditHomePage = 2,

		[Group("User")]
		[Description("The ability to submit a movie to be considered for publication. By default, all registered users have this permission, unless revoked.")]
		SubmitMovies = 3,

		[Group("User")]
		[Description("The ability to rate publications. By default, all registered users have this permission, unless revoked.")]
		RateMovies = 4,

		[Group("User")]
		[Description("The ability to create new topics on the forums. Experienced users have the ability to do this, unless revoked")]
		CreateForumTopics = 10,

		[Group("User")]
		[Description("The ability to participate in forum polls. Experienced users have the ability ot do this, unless revoked.")]
		VoteInPolls = 11,

		[Group("User")]
		[Description("The ability to use the mood avatar feature. Allows a user to specificy a mood avatar when creating a post in the forum.")]
		UseMoodAvatars = 12,

		[Group("User")]
		[Description("The ability to upload movie and related files for personal storage.")]
		UploadUserFiles,

		[Group("User")]
		[Description("The ability to send private messages.")]
		SendPrivateMessages = 20,

		#endregion

		#region Wiki 100

		[Group("Wiki")]
		[Description("The ability to edit basic wiki pages. This is the most basic editor privilege but some pages may be restrited to other privileges.")]
		EditWikiPages = 100,

		[Group("Wiki")]
		[Description("The ability to edit Game Resource wiki pages. These are basic game information and are considered separate from general wiki pages.")]
		EditGameResources = 101,

		[Group("Wiki")]
		[Description("The ability to edit System Wiki pages. These pages are more fundamental to the site behavior whan basic wiki pages.")]
		EditSystemPages = 102,

		[Group("Wiki")]
		[Description("The ability to create and edit roles and add/remove privileges to those roles.")]
		EditRoles = 103,

		[Group("Wiki")]
		[Description("The ability to movie a wiki page to another location.")]
		MoveWikiPages = 104,

		[Group("Wiki")]
		[Description("The ability to delete a wiki pag.e")]
		DeleteWikiPages = 105,

		[Group("Wiki")]
		[Description("The ability to see wiki pages/revisions that were deleted.")]
		SeeDeletedWikiPages = 106,

		[Group("Wiki")]
		[Description("The ability to add/edit/remove ram addresses.")]
		EditRamAddresses = 107,

		[Group("Wiki Administration")]
		[Description("The ability to see certain restricted pages that pertain to administration activities.")]
		SeeAdminPages = 190,

		#endregion

		#region Queue Maintenance 200

		[Group("Queue Maintenance")]
		[Description("The ability to judge submissions in the submission queue")]
		JudgeSubmissions = 200,

		[Group("Queue Maintenance")]
		[Description("The ability to publish movies")]
		PublishMovies = 201,

		[Group("Queue Maintenance")]
		[Description("The ability to edit submission text and basic metadata. Changing statuses, editing movie files, etc. are further restricted by other permissions")]
		EditSubmissions = 202,

		[Group("Queue Maintenance")]
		[Description("The ability to replace the movie file of an existing un-published submission")]
		ReplaceSubmissionMovieFile = 203,

		[Group("Queue Maintenance")]
		[Description("The ability to set a submission status regardless of condition (exception: published submissions)")]
		OverrideSubmissionStatus = 204,

		[Group("Queue Maintenance")]
		[Description("The ability to deprecate an existing movie parser. When deprecated, a movie will no longer be eligible for submission")]
		DeprecateMovieParsers = 205,

		#endregion

		#region Publication Maintenance 300

		[Group("Publication Maintenance")]
		[Description("The ability to set a published movie's class")]
		SetPublicationClass = 300,

		[Group("Publication Maintenance")]
		[Description("The ability to edit the game, system, and rom information for movies")]
		CatalogMovies = 301,

		[Group("Publication Maintenance")]
		[Description("The ability to edit publication information such as branch, tags, flags, etc")]
		EditPublicationMetaData = 302,

		[Group("Publication Maintenance")]
		[Description("The ability to see publication ratings even from users with non-public ratings")]
		SeePrivateRatings = 303,

		[Group("Publication Maintenance")]
		[Description("The ability to edit a movie's recomended flag, which flags the movie as recommended to new comers")]
		EditRecommendation = 304,

		[Group("Publication Maintenance")]
		[Description("The ability to add/remove publication files such as screenshots")]
		EditPublicationFiles = 305,

		[Group("Publication Maintenance")]
		[Description("The ability to add/remove additional movie files to an existing publication")]
		CreateAdditionalMovieFiles = 306,

		[Group("Publication Maintenance")]
		[Description("The ability to add, edit, and remove the tags used for publications")]
		TagMaintenance = 390,

		[Group("Publication Maintenance")]
		[Description("The ability to add, edit, and remove the flags used for publications")]
		FlagMaintenance = 391,

		[Group("Publication Maintenance")]
		[Description("The ability to add, edit, and remove the publication classes")]
		ClassMaintenance = 392,

		#endregion

		#region Forum Moderation 400

		[Group("Forum Moderation")]
		[Description("The ability to edit post created by another user.")]
		EditForumPosts = 400,

		[Group("Forum Moderation")]
		[Description("The ability to delete post created by another user.")]
		DeleteForumPosts = 401,

		[Group("Forum Administration")]
		[Description("The ability to lock and unlock forum topics.")]
		LockTopics = 402,

		[Group("Forum Administration")]
		[Description("The ability to create a post in a topic that is currently locked.")]
		PostInLockedTopics = 403,

		[Group("Forum Administration")]
		[Description("The ability to move a topic from one forum to another.")]
		MoveTopics = 404,

		[Group("Forum Administration")]
		[Description("The ability to split a subset of posts from a topic and create a new topic")]
		SplitTopics = 405,

		[Group("Forum Administration")]
		[Description("The ability to create or edit a forum.")]
		EditForums = 406,

		[Group("Forum Administration")]
		[Description("The ability to edit forum categories")]
		EditCategories = 407,

		[Group("Forum Administration")]
		[Description("The ability to participate in forum polls. Experienced users have the ability ot do this, unless revoked.")]
		CreateForumPolls = 408,

		[Group("Forum Administration")]
		[Description("Ability to set whether or not a topic is an announcement or a sticky topic.")]
		SetTopicType = 409,

		[Group("Forum Administration")]
		[Description("Ability to set merge a topic into an existing topic.")]
		MergeTopics = 410,

		[Group("Forum Administration")]
		[Description("Ability to edit username patterns that are disallowed for registration.")]
		EditDisallows = 411,

		[Group("Forum Administration")]
		[Description("The ability to see forums that are restricted from general access.")]
		SeeRestrictedForums = 490,

		[Group("Forum Administration")]
		[Description("The ability to see which users voted on which poll option.")]
		SeePollResults = 491,

		[Group("Forum Administration")]
		[Description("The ability to reset a poll result to empty.")]
		ResetPollResults = 492,

		#endregion

		#region User Administration 500

		[Group("User Administration")]
		[Description("The ability to delete an existing role.")]
		DeleteRoles = 500,

		[Group("User Administration")]
		[Description("The ability to see private information about a user, such as email address")]
		ViewPrivateUserData = 501,

		[Group("User Administration")]
		[Description("The ability to edit basic information about another user.")]
		EditUsers = 502,

		[Group("User Administration")]
		[Description("The ability to change another user's UserName. Users with this permission should also have the EditUsers permission.")]
		EditUsersUserName = 503,

		[Group("User Administration")]
		[Description("The ability to assign Roles to any User with some restrictions. A role can only be assigned if all permissions within it are marked as assignable by a role the user has.")]
		AssignRoles = 504,

		[Group("User Administration")]
		[Description("The ability to edit/delete user files of another user.")]
		EditUserFiles = 505,

		[Group("User Administration")]
		[Description("The to ban/unban ip addresses that a user can register or log in from.")]
		BanIpAddresses = 506,

		#endregion

		#region Admin 9001

		[Group("Admin")]
		[Description("The ability to see high level application and server information such as diagnostics stats and other sensitive information")]
		SeeDiagnostics = 9001

		#endregion
	}

	public static class PermissionUtil
	{
		public static IEnumerable<PermissionTo> AllPermissions() => Enum
			.GetValues(typeof(PermissionTo))
			.Cast<PermissionTo>()
			.ToList();
	}
}
