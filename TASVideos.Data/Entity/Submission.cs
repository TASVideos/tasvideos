using TASVideos.Data.AutoHistory;

namespace TASVideos.Data.Entity;

public enum ShowVerified { All, Verified, NotVerified }
public interface ISubmissionFilter
{
	ICollection<SubmissionStatus> Statuses { get; }
	ICollection<int> Years { get; }
	ICollection<string> Systems { get; }
	string? User { get; }
	ICollection<int> GameIds { get; }
	int? StartType { get; }
	bool? ShowVerified { get; }
}

[IncludeInAutoHistory]
public class Submission : BaseEntity, ITimeable
{
	public int Id { get; set; }

	public int? TopicId { get; set; }
	public ForumTopic? Topic { get; set; }

	public int SubmitterId { get; set; }
	public User? Submitter { get; set; }

	public ICollection<SubmissionAuthor> SubmissionAuthors { get; init; } = [];

	public int? IntendedClassId { get; set; }
	public PublicationClass? IntendedClass { get; set; }

	public int? JudgeId { get; set; }
	public User? Judge { get; set; }

	public int? PublisherId { get; set; }
	public User? Publisher { get; set; }

	public SubmissionStatus Status { get; set; } = SubmissionStatus.New;
	public ICollection<SubmissionStatusHistory> History { get; init; } = [];

	[ExcludeFromAutoHistory]
	public byte[] MovieFile { get; set; } = [];

	public string? MovieExtension { get; set; }

	public int? GameId { get; set; }
	public Game.Game? Game { get; set; }

	public int? GameVersionId { get; set; }
	public GameVersion? GameVersion { get; set; }

	// Metadata parsed from movie file
	public int? SystemId { get; set; }
	public GameSystem? System { get; set; }

	public int? SystemFrameRateId { get; set; }
	public GameSystemFrameRate? SystemFrameRate { get; set; }

	public Publication? Publication { get; set; }

	public int Frames { get; set; }
	public int RerecordCount { get; set; }

	// Metadata, user entered
	public string? EncodeEmbedLink { get; set; }

	public string? SubmittedGameVersion { get; set; }

	public string? GameName { get; set; }

	public string? Branch { get; set; }

	public string? RomName { get; set; }

	public string? EmulatorVersion { get; set; }

	public int? MovieStartType { get; set; }

	public int? RejectionReasonId { get; set; }
	public SubmissionRejectionReason? RejectionReason { get; set; }

	/// <summary>
	/// Gets or sets Any author's that are not a user. If they are a user, they should be linked, and not listed here.
	/// </summary>
	public string? AdditionalAuthors { get; set; }

	/// <summary>
	/// Gets or sets a de-normalized column consisting of the submission title for display when linked or in the queue
	/// ex: N64 The Legend of Zelda: Majora's Mask "low%" in 1:59:01.
	/// </summary>
	public string Title { get; set; } = "";

	public string? Annotations { get; set; }

	double ITimeable.FrameRate => SystemFrameRate?.FrameRate ?? 0;

	public int? GameGoalId { get; set; }
	public GameGoal? GameGoal { get; set; }

	public string? HashType { get; set; }
	public string? Hash { get; set; }

	public void GenerateTitle()
	{
		var authorList = SubmissionAuthors
			.OrderBy(sa => sa.Ordinal)
			.Select(sa => sa.Author?.UserName)
			.Where(sa => !string.IsNullOrWhiteSpace(sa));

		if (!string.IsNullOrWhiteSpace(AdditionalAuthors))
		{
			authorList = authorList.Concat(AdditionalAuthors.SplitWithEmpty(","));
		}

		var gameName = GameName;
		if (Game is not null && Game.Id > 0)
		{
			gameName = Game.DisplayName;
		}

		if (GameVersion is not null && !string.IsNullOrWhiteSpace(GameVersion.TitleOverride))
		{
			gameName = GameVersion.TitleOverride;
		}

		string? goal = GameGoal?.DisplayName;
		goal = goal switch
		{
			null => Branch,
			"baseline" => null,
			_ => goal
		};

		Title =
		$"#{Id}: {string.Join(", ", authorList).LastCommaToAmpersand()}'s {System?.Code ?? "Unknown"} {gameName}"
			+ (!string.IsNullOrWhiteSpace(goal) ? $" \"{goal}\"" : "")
			+ $" in {this.Time().ToStringWithOptionalDaysAndHours()}";
	}

	// Temporary for import debugging
	public decimal LegacyTime { get; set; }
	public decimal ImportedTime { get; set; }

	public string? Warnings { get; set; }

	public long? CycleCount { get; set; }

	public int? SyncedByUserId { get; set; }
	public User? SyncedByUser { get; set; }
	public DateTime? SyncedOn { get; set; }

	public string? AdditionalSyncNotes { get; set; }
}

public static class SubmissionExtensions
{
	public static bool CanPublish(this Submission submission) => submission is
	{
		SystemId: > 0,
		SystemFrameRateId: > 0,
		GameId: > 0,
		GameVersionId: > 0,
		IntendedClassId: > 0,
		Status: SubmissionStatus.PublicationUnderway,
		SyncedOn: not null
	};

	public static IQueryable<Submission> FilterBy(this IQueryable<Submission> query, ISubmissionFilter criteria)
	{
		if (!string.IsNullOrWhiteSpace(criteria.User))
		{
			query = query.Where(s => s.SubmissionAuthors.Any(sa => sa.Author!.UserName == criteria.User)
				|| s.Submitter != null && s.Submitter.UserName == criteria.User);
		}

		if (criteria.Years.Any())
		{
			query = query.Where(p => criteria.Years.Contains(p.CreateTimestamp.Year));
		}

		if (criteria.Statuses.Any())
		{
			query = query.Where(s => criteria.Statuses.Contains(s.Status));
		}

		if (criteria.Systems.Any())
		{
			query = query.Where(s => s.System != null && criteria.Systems.Contains(s.System.Code));
		}

		if (criteria.GameIds.Any())
		{
			query = query.Where(s => criteria.GameIds.Contains(s.GameId ?? 0));
		}

		if (criteria.StartType.HasValue)
		{
			query = query.Where(s => s.MovieStartType == criteria.StartType);
		}

		if (criteria.ShowVerified.HasValue)
		{
			query = criteria.ShowVerified.Value
				? query.ThatAreVerified()
				: query.ThatAreUnverified();
		}

		return query;
	}

	public static IQueryable<Submission> ThatAreActive(this IQueryable<Submission> query)
		=> query.Where(s => s.Status != SubmissionStatus.Published
			&& s.Status != SubmissionStatus.Playground
			&& s.Status != SubmissionStatus.Cancelled
			&& s.Status != SubmissionStatus.Rejected);

	public static IQueryable<Submission> ThatAreInActive(this IQueryable<Submission> query)
		=> query.Where(s => s.Status == SubmissionStatus.Published
			|| s.Status == SubmissionStatus.Playground
			|| s.Status == SubmissionStatus.Cancelled
			|| s.Status == SubmissionStatus.Rejected);

	public static IQueryable<Submission> ThatAreRejected(this IQueryable<Submission> query)
		=> query.Where(s => s.Status == SubmissionStatus.Rejected);

	public static IQueryable<Submission> ThatHaveBeenJudgedBy(this IQueryable<Submission> query, string userName)
		=> query.Where(s => s.JudgeId.HasValue && s.Judge!.UserName == userName);

	public static IQueryable<Submission> ForAuthor(this IQueryable<Submission> submissions, int userId)
		=> submissions.Where(p => p.SubmissionAuthors.Select(pa => pa.UserId).Contains(userId));

	/// <summary>
	/// Includes all the necessary sub-tables in order to generate a title
	/// </summary>
	public static IQueryable<Submission> IncludeTitleTables(this DbSet<Submission> query)
		=> query
			.Include(s => s.SubmissionAuthors)
			.ThenInclude(sa => sa.Author)
			.Include(s => s.System)
			.Include(s => s.SystemFrameRate)
			.Include(s => s.Game)
			.Include(s => s.GameVersion)
			.Include(s => s.GameGoal)
			.Include(gg => gg.GameGoal);

	public static IQueryable<Submission> ThatAreVerified(this IQueryable<Submission> query)
		=> query.Where(s => s.SyncedOn.HasValue);

	public static IQueryable<Submission> ThatAreUnverified(this IQueryable<Submission> query)
		=> query.Where(s => !s.SyncedOn.HasValue);
}
