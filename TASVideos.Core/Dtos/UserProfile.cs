using System.ComponentModel.DataAnnotations;

// TODO: Create a view model to separate presentation concerns (Display attributes)
namespace TASVideos.Core.Services;

/// <summary>
/// Represents a user with publicly available information
/// Intended for the User/Profile page.
/// </summary>
public class UserProfile
{
	public int Id { get; set; }
	public string UserName { get; set; } = "";
	public int PlayerPoints { get; set; }
	public string PlayerRank { get; set; } = "";
	public DateTime JoinedOn { get; set; }
	public DateTime? LastLoggedIn { get; set; }
	public int PostCount { get; set; }
	public string? Avatar { get; set; }
	public string? Location { get; set; }
	public string? Signature { get; set; }
	public bool PublicRatings { get; set; }
	public string? TimeZone { get; set; }
	public PreferredPronounTypes PreferredPronouns { get; set; }

	// Private info
	public string? Email { get; set; }
	public bool EmailConfirmed { get; set; }
	public bool LockedOutStatus { get; set; }
	public DateTime? BannedUntil { get; set; }
	public string? ModeratorComments { get; set; }
	public int PublicationActiveCount { get; set; }
	public int PublicationObsoleteCount { get; set; }
	public bool HasHomePage { get; set; }
	public bool AnyPublications => PublicationActiveCount + PublicationObsoleteCount > 0;
	public IEnumerable<string> PublishedSystems { get; set; } = [];
	public WikiEdit WikiEdits { get; set; } = new();
	public PublishingSummary Publishing { get; set; } = new();
	public JudgingSummary Judgments { get; set; } = new();
	public IEnumerable<RoleDto> Roles { get; set; } = [];
	public IEnumerable<AwardAssignmentSummary> Awards { get; set; } = [];
	public IEnumerable<SubmissionEntry> Submissions { get; set; } = [];
	public RatingSummary Ratings { get; set; } = new();
	public UserFileSummary UserFiles { get; set; } = new();
	public int SubmissionCount => Submissions.Sum(s => s.Count);
	public bool IsBanned => BannedUntil.HasValue && BannedUntil >= DateTime.UtcNow;
	public bool BanIsIndefinite => BannedUntil >= DateTime.UtcNow.AddYears(2);

	public class SubmissionEntry
	{
		public SubmissionStatus Status { get; set; }
		public int Count { get; set; }
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
		public int Total { get; set; }
		public IEnumerable<string> Systems { get; set; } = [];
	}

	// TODO: more data points
	public class PublishingSummary
	{
		public int TotalPublished { get; set; }
	}

	// TODO: more data points
	public class JudgingSummary
	{
		public int TotalJudgments { get; set; }
	}
}
