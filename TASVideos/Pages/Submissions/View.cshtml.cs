using System.Net.Mime;
using TASVideos.Core.Services.Wiki;
using TASVideos.MovieParsers.Result;

namespace TASVideos.Pages.Submissions;

[AllowAnonymous]
public class ViewModel(ApplicationDbContext db, IWikiPages wikiPages) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	public int PublicationId { get; set; }

	public bool IsPublished => PublicationId > 0;

	public bool CanEdit { get; set; }

	public SubmissionDisplay Submission { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var submission = await db.Submissions
			.Where(s => s.Id == Id)
			.Select(s => new SubmissionDisplay // It is important to use a projection here to avoid querying the file data which is not needed and can be slow
			{
				StartType = (MovieStartType?)s.MovieStartType,
				SystemDisplayName = s.System!.DisplayName,
				GameName = s.GameId != null ? s.Game!.DisplayName : null,
				SubmittedGameName = s.GameName,
				GameVersion = s.GameVersionId != null ? s.GameVersion!.Name : s.SubmittedGameVersion,
				RomName = s.RomName,
				Branch = s.Branch,
				Goal = s.GameGoal != null
					? s.GameGoal!.DisplayName
					: null,
				Emulator = s.EmulatorVersion,
				FrameCount = s.Frames,
				FrameRate = s.SystemFrameRate!.FrameRate,
				RerecordCount = s.RerecordCount,
				Submitted = s.CreateTimestamp,
				Submitter = s.Submitter!.UserName,
				Status = s.Status,
				EncodeEmbedLink = s.EncodeEmbedLink,
				Judge = s.Judge != null ? s.Judge.UserName : "",
				Title = s.Title,
				ClassName = s.IntendedClass != null ? s.IntendedClass.Name : "",
				Publisher = s.Publisher != null ? s.Publisher.UserName : "",
				SystemId = s.SystemId,
				SystemFrameRateId = s.SystemFrameRateId,
				GameId = s.GameId,
				GameVersionId = s.GameVersionId,
				RejectionReasonDisplay = s.RejectionReasonId.HasValue
					? s.RejectionReason!.DisplayName
					: null,
				Authors = s.SubmissionAuthors
					.Where(sa => sa.SubmissionId == Id)
					.OrderBy(sa => sa.Ordinal)
					.Select(sa => sa.Author!.UserName)
					.ToList(),
				AdditionalAuthors = s.AdditionalAuthors,
				TopicId = s.TopicId,
				Warnings = s.Warnings,
				CycleCount = s.CycleCount,
				Annotations = s.Annotations,
				GameGoalId = s.GameGoalId
			})
			.SingleOrDefaultAsync();

		if (submission is null)
		{
			return NotFound();
		}

		Submission = submission;
		var submissionPage = await wikiPages.SubmissionPage(Id);
		if (submissionPage is not null)
		{
			Submission.LastUpdateTimestamp = submissionPage.CreateTimestamp;
			Submission.LastUpdateUser = submissionPage.AuthorName;
		}

		CanEdit = !string.IsNullOrWhiteSpace(User.Name())
			&& (User.Name() == Submission.Submitter
				|| Submission.Authors.Contains(User.Name()));

		if (Submission.Status == SubmissionStatus.Published)
		{
			PublicationId = (await db.Publications.SingleOrDefaultAsync(p => p.SubmissionId == Id))?.Id ?? 0;
		}

		return Page();
	}

	public async Task<IActionResult> OnGetDownload()
	{
		var submissionFile = await db.Submissions
			.Where(s => s.Id == Id)
			.Select(s => s.MovieFile)
			.SingleOrDefaultAsync();

		if (submissionFile is null)
		{
			return NotFound();
		}

		return File(submissionFile, MediaTypeNames.Application.Octet, $"submission{Id}.zip");
	}

	public class SubmissionDisplay : ISubmissionDisplay
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
		public string? Submitter { get; init; }

		[Display(Name = "Submit Date")]
		public DateTime Submitted { get; init; }

		[Display(Name = "Last Edited")]
		public DateTime LastUpdateTimestamp { get; set; }

		[Display(Name = "Last Edited By")]
		public string? LastUpdateUser { get; set; }
		public SubmissionStatus Status { get; init; }
		public string? Judge { get; init; }
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
}
