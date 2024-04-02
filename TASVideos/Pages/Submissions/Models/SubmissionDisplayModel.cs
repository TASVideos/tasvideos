using TASVideos.MovieParsers.Result;

namespace TASVideos.Pages.Submissions.Models;

public class SubmissionDisplayModel : ISubmissionDisplay
{
	public bool IsCataloged => SystemId.HasValue
		&& SystemFrameRateId.HasValue
		&& GameId > 0
		&& GameVersionId > 0
		&& GameGoalId > 0;

	[Display(Name = "Start Type")]
	public MovieStartType? StartType { get; init; }

	[Display(Name = "For Publication Class")]
	public string? ClassName { get; init; }

	[Display(Name = "System")]
	public string? SystemDisplayName { get; init; }

	[Display(Name = "Game Name")]
	public string? GameName { get; init; }

	[Display(Name = "Submitted Game Name")]
	public string? SubmittedGameName { get; init; }

	[Display(Name = "Game Version")]
	public string? GameVersion { get; init; }

	[Display(Name = "ROM Filename")]
	public string? RomName { get; init; }

	[Display(Name = "Goal")]
	public string? Branch { get; init; }
	public string? Goal { get; init; }

	[Display(Name = "Emulator")]
	public string? Emulator { get; init; }

	[Url]
	[Display(Name = "Encode Embed Link")]
	public string? EncodeEmbedLink { get; init; }

	[Display(Name = "Frame Count")]
	public int FrameCount { get; init; }

	[Display(Name = "Cycle Count")]
	public long? CycleCount { get; init; }

	[Display(Name = "Frame Rate")]
	public double FrameRate { get; init; }

	[Display(Name = "Rerecord Count")]
	public int RerecordCount { get; init; }

	[Display(Name = "Author")]
	public List<string> Authors { get; init; } = [];

	[Display(Name = "Submitter")]
	public string? Submitter { get; init; }

	[Display(Name = "Submit Date")]
	public DateTime Submitted { get; init; }

	[Display(Name = "Last Edited")]
	public DateTime LastUpdateTimestamp { get; set; }

	[Display(Name = "Last Edited By")]
	public string? LastUpdateUser { get; set; }

	[Display(Name = "Status")]
	public SubmissionStatus Status { get; init; }

	[Display(Name = "Judge")]
	public string? Judge { get; init; }

	[Display(Name = "Publisher")]
	public string? Publisher { get; init; }
	public string? Annotations { get; init; }
	public string? RejectionReasonDisplay { get; init; }
	public string Title { get; init; } = "";
	public string? AdditionalAuthors { get; init; }
	public bool WarnStartType => StartType.HasValue && StartType != MovieStartType.PowerOn;
	public int? TopicId { get; init; }
	public int? GameId { get; init; }
	public string? Warnings { get; init; }
	internal int? SystemId { get; init; }
	internal int? SystemFrameRateId { get; init; }
	public int? GameVersionId { get; init; }
	internal int? GameGoalId { get; init; }
}
