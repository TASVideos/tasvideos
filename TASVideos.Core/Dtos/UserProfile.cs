namespace TASVideos.Core.Services;

/// <summary>
/// Represents a user with publicly available information
/// Intended for the User/Profile page.
/// </summary>
public class UserProfile
{
	public int Id { get; init; }
	public string UserName { get; init; } = "";
	public int PlayerPoints { get; set; }
	public string PlayerRank { get; set; } = "";
	public DateTime JoinedOn { get; init; }
	public DateTime? LastLoggedIn { get; init; }
	public int PostCount { get; init; }
	public string? Avatar { get; init; }
	public string? Location { get; init; }
	public string? Signature { get; init; }
	public bool PublicRatings { get; init; }
	public string? TimeZone { get; init; }
	public PreferredPronounTypes PreferredPronouns { get; init; }

	// Private info
	public string? Email { get; init; }
	public bool EmailConfirmed { get; init; }
	public bool LockedOutStatus { get; init; }
	public DateTime? BannedUntil { get; init; }
	public string? ModeratorComments { get; init; }
	public int PublicationActiveCount { get; init; }
	public int PublicationObsoleteCount { get; init; }
	public bool HasHomePage { get; set; }
	public bool AnyPublications => PublicationActiveCount + PublicationObsoleteCount > 0;
	public IEnumerable<string> PublishedSystems { get; set; } = [];
	public WikiEdit WikiEdits { get; init; } = new();
	public PublishingSummary Publishing { get; set; } = new();
	public JudgingSummary Judgments { get; set; } = new();
	public IEnumerable<RoleDto> Roles { get; init; } = [];
	public IEnumerable<AwardAssignmentSummary> Awards { get; set; } = [];
	public IEnumerable<SubmissionEntry> Submissions { get; set; } = [];
	public RatingSummary Ratings { get; init; } = new();
	public UserFileSummary UserFiles { get; init; } = new();
	public int SubmissionCount => Submissions.Sum(s => s.Count);
	public bool IsBanned => BannedUntil.HasValue && BannedUntil >= DateTime.UtcNow;
	public bool BanIsIndefinite => BannedUntil >= DateTime.UtcNow.AddYears(2);

	public class SubmissionEntry
	{
		public SubmissionStatus Status { get; init; }
		public int Count { get; init; }
	}

	public class WikiEdit
	{
		public int TotalEdits { get; set; }
		public DateTime? FirstEdit { get; set; }
		public DateTime? LastEdit { get; set; }

		public DateTime FirstEditDateTime => FirstEdit ?? DateTime.UtcNow;
		public DateTime LastEditDateTime => LastEdit ?? DateTime.UtcNow;
	}

	public class RatingSummary
	{
		public int TotalMoviesRated { get; set; }
	}

	public class UserFileSummary
	{
		public int Total { get; init; }
		public IEnumerable<string> Systems { get; set; } = [];
	}

	// TODO: more data points
	public class PublishingSummary
	{
		public int TotalPublished { get; init; }
	}

	// TODO: more data points
	public class JudgingSummary
	{
		public int TotalJudgments { get; init; }
	}
}
