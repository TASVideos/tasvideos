﻿using TASVideos.Core.Services.Wiki;
using TASVideos.MovieParsers.Result;

namespace TASVideos.Pages.Submissions;

[AllowAnonymous]
public class ViewModel(ApplicationDbContext db, IWikiPages wikiPages, IFileService fileService) : BasePageModel
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
				GameVersion = s.GameVersionId != null ? s.GameVersion!.Name : "",
				SubmittedGameVersion = s.SubmittedGameVersion,
				RomName = s.RomName,
				Branch = s.Branch,
				Goal = s.GameGoal != null
					? s.GameGoal!.DisplayName
					: null,
				Emulator = s.EmulatorVersion,
				FrameCount = s.Frames,
				FrameRate = s.SystemFrameRate!.FrameRate,
				RerecordCount = s.RerecordCount,
				Date = s.CreateTimestamp,
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
				GameGoalId = s.GameGoalId,
				SyncedOn = s.SyncedOn,
				SyncedBy = s.SyncedBy
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
		public string? RomName { get; init; }
		public string? Branch { get; init; }
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
		public string? SyncedBy { get; set; }
		public DateTime? SyncedOn { get; set; }
	}
}
