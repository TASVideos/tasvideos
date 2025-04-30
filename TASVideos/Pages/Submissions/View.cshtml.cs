using TASVideos.Core.Services.Wiki;
using TASVideos.MovieParsers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.Pages.Submissions;

[AllowAnonymous]
public class ViewModel(ApplicationDbContext db, IWikiPages wikiPages, IFileService fileService, IMovieParser parser)
	: SubmitPageModelBase(parser, fileService)
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
			.ToSubmissionDisplayModel()
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

		CanEdit = CanEditSubmission(Submission.Submitter, Submission.Authors);
		if (Submission.Status == SubmissionStatus.Published)
		{
			PublicationId = await db.Publications.Where(p => p.SubmissionId == Id).Select(p => p.Id).SingleOrDefaultAsync();
		}

		return Page();
	}

	public async Task<IActionResult> OnGetDownload() => ZipFile(await fileService.GetSubmissionFile(Id));

	public class SubmissionDisplay : ISubmissionDisplay
	{
		public bool IsCataloged => SystemId.HasValue
			&& SystemFrameRateId.HasValue
			&& GameId > 0
			&& GameVersionId > 0
			&& GameGoalId > 0;

		public MovieStartType? StartType { get; init; }
		public string? ClassName { get; init; }
		public string? SystemDisplayName { get; init; }
		public string? GameName { get; init; }
		public string? SubmittedGameName { get; init; }
		public string? GameVersion { get; init; }
		public string? SubmittedGameVersion { get; init; }
		public string? SubmittedRomName { get; init; }
		public string? SubmittedBranch { get; init; }
		public string? Goal { get; init; }
		public string? Emulator { get; init; }

		[Url]
		public string? EncodeEmbedLink { get; init; }
		public int FrameCount { get; init; }
		public long? CycleCount { get; init; }
		public double FrameRate { get; init; }
		public int RerecordCount { get; init; }
		public List<string> Authors { get; init; } = [];
		public string? Submitter { get; init; }
		public DateTime Date { get; init; }
		public DateTime LastUpdateTimestamp { get; set; }
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
		public string? SyncedBy { get; init; }
		public DateTime? SyncedOn { get; init; }
		public string? AdditionalSyncNotes { get; init; }
		public string? HashType { get; init; }
		public string? Hash { get; init; }
	}
}
