using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data.Entity.Awards;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Data.Entity;

public class User : IdentityUser<int>, ITrackable
{
	public DateTime? LastLoggedInTimeStamp { get; set; }

	[Required]
	[StringLength(250)]
	public string TimeZoneId { get; set; } = TimeZoneInfo.Utc.Id;

	public DateTime CreateTimestamp { get; set; } = DateTime.UtcNow;
	public string? CreateUserName { get; set; }
	public DateTime LastUpdateTimestamp { get; set; } = DateTime.UtcNow;
	public string? LastUpdateUserName { get; set; }

	[StringLength(250)]
	public string? Avatar { get; set; }

	[StringLength(100)]
	public string? From { get; set; }

	[StringLength(1000)]
	public string? Signature { get; set; }

	public bool PublicRatings { get; set; } = true;

	[StringLength(250)]
	public string? MoodAvatarUrlBase { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether or not to use
	/// the user's ratings when calculating a publication's average rating.
	/// </summary>
	public bool UseRatings { get; set; } = true;

	public PreferredPronounTypes PreferredPronouns { get; set; } = PreferredPronounTypes.Unspecified;

	// TODO: migration to remove this column
	[StringLength(32)]
	public string? LegacyPassword { get; set; }

	public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
	public virtual ICollection<SubmissionAuthor> Submissions { get; set; } = new HashSet<SubmissionAuthor>();
	public virtual ICollection<PublicationAuthor> Publications { get; set; } = new HashSet<PublicationAuthor>();

	public virtual ICollection<ForumTopic> Topics { get; set; } = new HashSet<ForumTopic>();
	public virtual ICollection<ForumPost> Posts { get; set; } = new HashSet<ForumPost>();
	public virtual ICollection<ForumTopicWatch> ForumTopicWatches { get; set; } = new HashSet<ForumTopicWatch>();

	public virtual ICollection<UserAward> UserAwards { get; set; } = new HashSet<UserAward>();

	[ForeignKey(nameof(PrivateMessage.FromUserId))]
	public virtual ICollection<PrivateMessage> SentPrivateMessages { get; set; } = new HashSet<PrivateMessage>();

	[ForeignKey(nameof(PrivateMessage.ToUserId))]
	public virtual ICollection<PrivateMessage> ReceivedPrivateMessages { get; set; } = new HashSet<PrivateMessage>();

	public virtual ICollection<PublicationRating> PublicationRatings { get; set; } = new HashSet<PublicationRating>();

	public virtual ICollection<UserFile> UserFiles { get; set; } = new HashSet<UserFile>();
	public virtual ICollection<UserFileComment> UserFileComments { get; set; } = new HashSet<UserFileComment>();

	public virtual ICollection<PublicationMaintenanceLog> PublicationMaintenanceLogs { get; set; } = new HashSet<PublicationMaintenanceLog>();

	public virtual ICollection<UserMaintenanceLog> UserMaintenanceLogs { get; set; } = new HashSet<UserMaintenanceLog>();
	public virtual ICollection<UserMaintenanceLog> EditMaintenanceLogs { get; set; } = new HashSet<UserMaintenanceLog>();

	public virtual ICollection<WikiPage> WikiRevisions { get; set; } = new HashSet<WikiPage>();
}

public enum PreferredPronounTypes
{
	Unspecified,

	[Display(Name = "He/Him")]
	HeHim,
	[Display(Name = "She/Her")]
	SheHer,

	[Display(Name = "They/Them")]
	TheyThem,

	[Display(Name = "He/They")]
	HeThey,

	[Display(Name = "She/They")]
	SheThey,

	[Display(Name = "It/Its")]
	ItIts,

	Any,

	Other
}

public static class UserExtensions
{
	public static async Task<bool> Exists(this IQueryable<User> query, string userName)
	{
		return await query.AnyAsync(q => q.UserName == userName);
	}

	public static IQueryable<User> ThatHaveSubmissions(this IQueryable<User> query)
	{
		return query.Where(u => u.Submissions.Any());
	}

	public static IQueryable<User> ThatPartiallyMatch(this IQueryable<User> query, string? partial)
	{
		var upper = partial?.ToUpper() ?? "";
		return query.Where(u => u.NormalizedUserName.Contains(upper));
	}

	public static IQueryable<User> ThatArePublishedAuthors(this IQueryable<User> query)
	{
		return query.Where(u => u.Publications.Any());
	}

	public static IQueryable<User> ThatHavePermission(this IQueryable<User> query, PermissionTo permission)
	{
		return query.Where(u => u.UserRoles
			.Any(r => r.Role!.RolePermission.Any(rp => rp.PermissionId == permission)));
	}

	public static IQueryable<User> ThatHaveRole(this IQueryable<User> query, string role)
	{
		return query.Where(u => u.UserRoles.Any(ur => ur.Role!.Name == role));
	}
}
