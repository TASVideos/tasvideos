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
	public MovieStartType? StartType { get; set; }

	[Display(Name = "For Publication Class")]
	public string? ClassName { get; set; }

	[Display(Name = "System")]
	public string? SystemDisplayName { get; set; }

	public string? SystemCode { get; set; }

	[Display(Name = "Game Name")]
	public string? GameName { get; set; }

	[Display(Name = "Submitted Game Name")]
	public string? SubmittedGameName { get; set; }

	[Display(Name = "Game Version")]
	public string? GameVersion { get; set; }

	[Display(Name = "ROM Filename")]
	public string? RomName { get; set; }

	[Display(Name = "Goal")]
	public string? Branch { get; set; }

	public string? Goal { get; set; }

	[Display(Name = "Emulator")]
	public string? Emulator { get; set; }

	[Url]
	[Display(Name = "Encode Embed Link")]
	public string? EncodeEmbedLink { get; set; }

	[Display(Name = "Frame Count")]
	public int FrameCount { get; set; }

	[Display(Name = "Cycle Count")]
	public long? CycleCount { get; set; }

	[Display(Name = "Frame Rate")]
	public double FrameRate { get; set; }

	[Display(Name = "Rerecord Count")]
	public int RerecordCount { get; set; }

	[Display(Name = "Author")]
	public List<string> Authors { get; set; } = [];

	[Display(Name = "Submitter")]
	public string? Submitter { get; set; }

	[Display(Name = "Submit Date")]
	public DateTime Submitted { get; set; }

	[Display(Name = "Last Edited")]
	public DateTime LastUpdateTimestamp { get; set; }

	[Display(Name = "Last Edited By")]
	public string? LastUpdateUser { get; set; }

	[Display(Name = "Status")]
	public SubmissionStatus Status { get; set; }

	[Display(Name = "Judge")]
	public string? Judge { get; set; }

	[Display(Name = "Publisher")]
	public string? Publisher { get; set; }

	public string? Annotations { get; set; }

	public string? RejectionReasonDisplay { get; set; }

	public string Title { get; set; } = "";

	public string? AdditionalAuthors { get; set; }

	public bool WarnStartType => StartType.HasValue && StartType != MovieStartType.PowerOn;

	public int? TopicId { get; set; }
	public int? GameId { get; set; }
	public string? Warnings { get; set; }

	internal int? SystemId { get; set; }
	internal int? SystemFrameRateId { get; set; }
	public int? GameVersionId { get; set; }
	internal int? GameGoalId { get; set; }
}
