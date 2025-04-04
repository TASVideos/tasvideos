using Microsoft.AspNetCore.Identity;

namespace TASVideos.Data.Entity;

public enum UserPreference
{
	Auto = 0, Always, Never
}

public class User : IdentityUser<int>, ITrackable
{
	public new string UserName
	{
		get => base.UserName!;
		set => base.UserName = value;
	}

	public new string NormalizedUserName
	{
		get => base.NormalizedUserName!;
		set => base.NormalizedUserName = value;
	}

	public new string Email
	{
		get => base.Email!;
		set => base.Email = value;
	}

	public new string NormalizedEmail
	{
		get => base.NormalizedEmail!;
		set => base.NormalizedEmail = value;
	}

	public DateTime? LastLoggedInTimeStamp { get; set; }

	public string TimeZoneId { get; set; } = TimeZoneInfo.Utc.Id;

	public DateTime CreateTimestamp { get; set; } = DateTime.UtcNow;
	public DateTime LastUpdateTimestamp { get; set; } = DateTime.UtcNow;

	public string? Avatar { get; set; }

	public string? From { get; set; }

	public string? Signature { get; set; }

	public bool PublicRatings { get; set; } = true;

	public string? MoodAvatarUrlBase { get; set; }

	public DateTime? BannedUntil { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to use
	/// the user's ratings when calculating a publication's average rating.
	/// </summary>
	public bool UseRatings { get; set; } = true;

	public string? ModeratorComments { get; set; }

	public PreferredPronounTypes PreferredPronouns { get; set; } = PreferredPronounTypes.Unspecified;

	public bool EmailOnPrivateMessage { get; set; }

	public UserPreference? AutoWatchTopic { get; set; }

	public UserDateFormat DateFormat { get; set; } = UserDateFormat.Auto;
	public UserTimeFormat TimeFormat { get; set; } = UserTimeFormat.Auto;
	public UserDecimalFormat DecimalFormat { get; set; } = UserDecimalFormat.Auto;

	public ICollection<UserRole> UserRoles { get; init; } = [];
	public ICollection<SubmissionAuthor> Submissions { get; init; } = [];
	public ICollection<PublicationAuthor> Publications { get; init; } = [];

	public ICollection<ForumTopic> Topics { get; init; } = [];
	public ICollection<ForumPost> Posts { get; init; } = [];
	public ICollection<ForumTopicWatch> ForumTopicWatches { get; init; } = [];

	public ICollection<UserAward> UserAwards { get; init; } = [];

	[ForeignKey(nameof(PrivateMessage.FromUserId))]
	public ICollection<PrivateMessage> SentPrivateMessages { get; init; } = [];

	[ForeignKey(nameof(PrivateMessage.ToUserId))]
	public ICollection<PrivateMessage> ReceivedPrivateMessages { get; init; } = [];

	public ICollection<PublicationRating> PublicationRatings { get; init; } = [];

	public ICollection<UserFile> UserFiles { get; init; } = [];
	public ICollection<UserFileComment> UserFileComments { get; init; } = [];

	public ICollection<PublicationMaintenanceLog> PublicationMaintenanceLogs { get; init; } = [];

	public ICollection<UserMaintenanceLog> UserMaintenanceLogs { get; init; } = [];
	public ICollection<UserMaintenanceLog> EditMaintenanceLogs { get; init; } = [];

	public ICollection<WikiPage> WikiRevisions { get; init; } = [];
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
	MDY
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
		=> await query.AnyAsync(q => q.UserName == userName);

	public static IQueryable<User> ThatHaveSubmissions(this IQueryable<User> query)
		=> query.Where(u => u.Submissions.Any());

	public static IQueryable<User> ThatPartiallyMatch(this IQueryable<User> query, string? partial)
	{
		var upper = partial?.ToUpper() ?? "";
		return query.Where(u => u.NormalizedUserName.Contains(upper));
	}

	public static IQueryable<User> ThatArePublishedAuthors(this IQueryable<User> query)
		=> query.Where(u => u.Publications.Any());

	public static IQueryable<User> ThatHavePermission(this IQueryable<User> query, PermissionTo permission)
		=> query.Where(u => u.UserRoles
			.Any(r => r.Role!.RolePermission.Any(rp => rp.PermissionId == permission)));

	public static IQueryable<User> ThatHaveRole(this IQueryable<User> query, string role)
		=> query.Where(u => u.UserRoles.Any(ur => ur.Role!.Name == role));

	public static IQueryable<User> ForUsers(this IQueryable<User> query, IEnumerable<string> users)
		=> query.Where(u => users.Contains(u.UserName));

	public static IQueryable<User> ForUser(this IQueryable<User> query, string? userName)
		=> query.Where(u => u.UserName == userName);

	public static IQueryable<User> ThatHaveCustomLocale(this IQueryable<User> query)
		=> query.Where(u => u.DateFormat != UserDateFormat.Auto || u.TimeFormat != UserTimeFormat.Auto || u.DecimalFormat != UserDecimalFormat.Auto);

	public static IQueryable<User> ThatAreBanned(this IQueryable<User> query)
		=> query.Where(u => u.BannedUntil.HasValue && u.BannedUntil > DateTime.UtcNow); // > and < in these methods, but what about ==?

	public static IQueryable<User> ThatAreNotBanned(this IQueryable<User> query)
		=> query.Where(u => !u.BannedUntil.HasValue || u.BannedUntil < DateTime.UtcNow);

	public static bool IsBanned(this User user) => user.BannedUntil.HasValue && user.BannedUntil > DateTime.UtcNow;

	public static IQueryable<SubmissionAuthor> ToSubmissionAuthors(this IQueryable<User> query, int submissionId, IList<string> authors)
		=> query
			.ForUsers(authors)
			.Select(u => new SubmissionAuthor
			{
				SubmissionId = submissionId,
				UserId = u.Id,
				Author = u,
				Ordinal = authors.IndexOf(u.UserName)
			});
}
