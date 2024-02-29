using Microsoft.AspNetCore.Identity;

namespace TASVideos.Data.Entity;

public enum UserPreference
{
	Auto = 0, Always, Never
}

public class User : IdentityUser<int>, ITrackable
{
	[StringLength(50)]
	public new string UserName
	{
		get => base.UserName!;
		set => base.UserName = value;
	}

	[StringLength(50)]
	public new string NormalizedUserName
	{
		get => base.NormalizedUserName!;
		set => base.NormalizedUserName = value;
	}

	[StringLength(100)]
	public new string Email
	{
		get => base.Email!;
		set => base.Email = value;
	}

	[StringLength(100)]
	public new string NormalizedEmail
	{
		get => base.NormalizedEmail!;
		set => base.NormalizedEmail = value;
	}

	public DateTime? LastLoggedInTimeStamp { get; set; }

	[StringLength(250)]
	public string TimeZoneId { get; set; } = TimeZoneInfo.Utc.Id;

	public DateTime CreateTimestamp { get; set; } = DateTime.UtcNow;
	public DateTime LastUpdateTimestamp { get; set; } = DateTime.UtcNow;

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
	/// Gets or sets a value indicating whether to use
	/// the user's ratings when calculating a publication's average rating.
	/// </summary>
	public bool UseRatings { get; set; } = true;

	[StringLength(1024)]
	public string? ModeratorComments { get; set; }

	public PreferredPronounTypes PreferredPronouns { get; set; } = PreferredPronounTypes.Unspecified;

	public bool EmailOnPrivateMessage { get; set; }

	public UserPreference? AutoWatchTopic { get; set; }

	public UserDateFormat DateFormat { get; set; } = UserDateFormat.Auto;
	public UserTimeFormat TimeFormat { get; set; } = UserTimeFormat.Auto;
	public UserDecimalFormat DecimalFormat { get; set; } = UserDecimalFormat.Auto;

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

public enum UserDateFormat
{
	[Display(Name = "Automatic")]
	Auto = 0,

	[Display(Name = "yyyy-MM-dd (2024-02-29)")]
	YMMDD,

	[Display(Name = "dd/MM/yyyy (29/02/2024)")]
	DDMMY,

	[Display(Name = "dd.MM.yyyy (29.02.2024)")]
	DDMMYDot,

	[Display(Name = "d/M/yyyy (29/2/2024)")]
	DMY,

	[Display(Name = "MM/dd/yyyy (02/29/2024)")]
	MMDDY,

	[Display(Name = "M/d/yyyy (2/29/2024)")]
	MDY,
}

public enum UserTimeFormat
{
	[Display(Name = "Automatic")]
	Auto = 0,

	[Display(Name = "24-hour clock (17:35)")]
	Clock24Hour,

	[Display(Name = "12-hour clock (5:35 PM)")]
	Clock12Hour
}

public enum UserDecimalFormat
{
	[Display(Name = "Automatic")]
	Auto = 0,

	[Display(Name = "Dot (1.23)")]
	Dot,

	[Display(Name = "Comma (1,23)")]
	Comma
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

	public static IQueryable<User> ForUsers(this IQueryable<User> query, IEnumerable<string> users)
	{
		return query.Where(u => users.Contains(u.UserName));
	}

	public static IQueryable<User> ThatHaveCustomLocale(this IQueryable<User> query)
	{
		return query.Where(u => u.DateFormat != UserDateFormat.Auto || u.TimeFormat != UserTimeFormat.Auto || u.DecimalFormat != UserDecimalFormat.Auto);
	}
}
