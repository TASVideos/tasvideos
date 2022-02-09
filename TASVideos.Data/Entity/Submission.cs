using TASVideos.Common;
using TASVideos.Data.Entity.Forum;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Data.Entity;

public interface ISubmissionFilter
{
	IEnumerable<SubmissionStatus> StatusFilter { get; }
	IEnumerable<int> Years { get; }
	IEnumerable<string> Systems { get; }
	string? User { get; }
	IEnumerable<int> GameIds { get; }
}

public class Submission : BaseEntity, ITimeable
{
	public int Id { get; set; }

	public int? WikiContentId { get; set; }
	public virtual WikiPage? WikiContent { get; set; }

	public int? TopicId { get; set; }
	public virtual ForumTopic? Topic { get; set; }

	// TODO: don't make this nullable! Need to fix the importer for this to work
	public int? SubmitterId { get; set; }
	public virtual User? Submitter { get; set; }

	public virtual ICollection<SubmissionAuthor> SubmissionAuthors { get; set; } = new HashSet<SubmissionAuthor>();

	public int? IntendedClassId { get; set; }
	public virtual PublicationClass? IntendedClass { get; set; }

	public int? JudgeId { get; set; }
	public virtual User? Judge { get; set; }

	public int? PublisherId { get; set; }
	public virtual User? Publisher { get; set; }

	public SubmissionStatus Status { get; set; } = SubmissionStatus.New;
	public virtual ICollection<SubmissionStatusHistory> History { get; set; } = new HashSet<SubmissionStatusHistory>();

	[Required]
	public byte[] MovieFile { get; set; } = Array.Empty<byte>();

	public string? MovieExtension { get; set; }

	public int? GameId { get; set; }
	public virtual Game.Game? Game { get; set; }

	public int? RomId { get; set; }
	public virtual GameRom? Rom { get; set; }

	// Metadata parsed from movie file
	public int? SystemId { get; set; }
	public virtual GameSystem? System { get; set; }

	public int? SystemFrameRateId { get; set; }
	public virtual GameSystemFrameRate? SystemFrameRate { get; set; }

	public virtual Publication? Publication { get; set; }

	public int Frames { get; set; }
	public int RerecordCount { get; set; }

	// Metadata, user entered
	[StringLength(100)]
	public string? EncodeEmbedLink { get; set; }

	[StringLength(100)]
	public string? GameVersion { get; set; }

	[StringLength(100)]
	public string? GameName { get; set; }

	[StringLength(50)]
	public string? Branch { get; set; }

	[StringLength(250)]
	public string? RomName { get; set; }

	[StringLength(50)]
	public string? EmulatorVersion { get; set; }

	public int? MovieStartType { get; set; }

	public int? RejectionReasonId { get; set; }
	public virtual SubmissionRejectionReason? RejectionReason { get; set; }

	/// <summary>
	/// Gets or sets Any author's that are not a user. If they are a user, they should linked, and not listed here.
	/// </summary>
	[StringLength(200)]
	public string? AdditionalAuthors { get; set; }

	/// <summary>
	/// Gets or sets a de-normalized column consisting of the submission title for display when linked or in the queue
	/// ex: N64 The Legend of Zelda: Majora's Mask "low%" in 1:59:01.
	/// </summary>
	[Required]
	public string Title { get; set; } = "";

	double ITimeable.FrameRate => SystemFrameRate?.FrameRate ?? 0;

	public void GenerateTitle()
	{
		if (System == null)
		{
			throw new ArgumentNullException($"{nameof(System)} can not be null.");
		}

		if (SystemFrameRate == null)
		{
			throw new ArgumentNullException($"{nameof(SystemFrameRate)} can not be null.");
		}

		var authorList = SubmissionAuthors
			.OrderBy(sa => sa.Ordinal)
			.Select(sa => sa.Author?.UserName)
			.Where(sa => !string.IsNullOrWhiteSpace(sa));

		if (!string.IsNullOrWhiteSpace(AdditionalAuthors))
		{
			authorList = authorList.Concat(AdditionalAuthors.SplitWithEmpty(","));
		}

		Title =
		$"#{Id}: {string.Join(", ", authorList).LastCommaToAmpersand()}'s {System.Code} {GameName}"
			+ (!string.IsNullOrWhiteSpace(Branch) ? $" \"{Branch}\"" : "")
			+ $" in {this.Time().ToStringWithOptionalDaysAndHours()}";
	}

	// Temporary for import debugging
	public decimal LegacyTime { get; set; }
	public decimal ImportedTime { get; set; }

	[StringLength(4096)]
	public string? LegacyAlerts { get; set; }
}

public static class SubmissionExtensions
{
	public static bool CanPublish(this Submission submission)
	{
		return submission.SystemId > 0
			&& submission.SystemFrameRateId > 0
			&& submission.GameId > 0
			&& submission.RomId > 0
			&& submission.IntendedClassId > 0
			&& submission.Status == SubmissionStatus.PublicationUnderway;
	}

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

		if (criteria.StatusFilter.Any())
		{
			query = query.Where(s => criteria.StatusFilter.Contains(s.Status));
		}

		if (criteria.Systems.Any())
		{
			query = query.Where(s => s.System != null && criteria.Systems.Contains(s.System.Code));
		}

		if (criteria.GameIds.Any())
		{
			query = query.Where(s => criteria.GameIds.Contains(s.GameId ?? 0));
		}

		return query;
	}

	public static IQueryable<Submission> ThatAreActive(this IQueryable<Submission> query)
	{
		return query.Where(s => s.Status != SubmissionStatus.Published
			&& s.Status != SubmissionStatus.Cancelled
			&& s.Status != SubmissionStatus.Rejected);
	}

	public static IQueryable<Submission> ThatAreRejected(this IQueryable<Submission> query)
	{
		return query.Where(s => s.Status == SubmissionStatus.Rejected);
	}

	public static IQueryable<Submission> ThatHaveBeenJudgedBy(this IQueryable<Submission> query, string userName)
	{
		return query.Where(s => s.JudgeId.HasValue && s.Judge!.UserName == userName);
	}
}
