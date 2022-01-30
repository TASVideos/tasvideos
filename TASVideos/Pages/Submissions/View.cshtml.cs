using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.MovieParsers.Result;
using TASVideos.Pages.Submissions.Models;

namespace TASVideos.Pages.Submissions;

[AllowAnonymous]
public class ViewModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public ViewModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromRoute]
	public int Id { get; set; }

	public int PublicationId { get; set; }

	public bool IsPublished => PublicationId > 0;

	public bool CanEdit { get; set; }

	public SubmissionDisplayModel Submission { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var submission = await _db.Submissions
			.Where(s => s.Id == Id)
			.Select(s => new SubmissionDisplayModel // It is important to use a projection here to avoid querying the file data which is not needed and can be slow
			{
				StartType = (MovieStartType?)s.MovieStartType,
				SystemDisplayName = s.System!.DisplayName,
				SystemCode = s.System.Code,
				GameName = s.GameId != null ? s.Game!.DisplayName : s.GameName,
				GameVersion = s.GameVersion,
				RomName = s.RomName,
				Branch = s.Branch,
				Emulator = s.EmulatorVersion,
				FrameCount = s.Frames,
				FrameRate = s.SystemFrameRate!.FrameRate,
				RerecordCount = s.RerecordCount,
				Submitted = s.CreateTimestamp,
				Submitter = s.Submitter!.UserName,
				LastUpdateTimestamp = s.WikiContent!.LastUpdateTimestamp,
				LastUpdateUser = s.WikiContent.LastUpdateUserName,
				Status = s.Status,
				EncodeEmbedLink = s.EncodeEmbedLink,
				Judge = s.Judge != null ? s.Judge.UserName : "",
				Title = s.Title,
				ClassName = s.IntendedClass != null ? s.IntendedClass.Name : "",
				Publisher = s.Publisher != null ? s.Publisher.UserName : "",
				SystemId = s.SystemId,
				SystemFrameRateId = s.SystemFrameRateId,
				GameId = s.GameId,
				RomId = s.RomId,
				RejectionReasonDisplay = s.RejectionReasonId.HasValue
					? s.RejectionReason!.DisplayName
					: null,
				Authors = s.SubmissionAuthors
					.Where(sa => sa.SubmissionId == Id)
					.OrderBy(sa => sa.Ordinal)
					.Select(sa => sa.Author!.UserName)
					.ToList(),
				AdditionalAuthors = s.AdditionalAuthors,
				TopicId = s.TopicId
			})
			.SingleOrDefaultAsync();

		if (submission == null)
		{
			return NotFound();
		}

		Submission = submission;
		CanEdit = !string.IsNullOrWhiteSpace(User.Name())
			&& (User.Name() == Submission.Submitter
				|| Submission.Authors.Contains(User.Name()));

		if (Submission.Status == SubmissionStatus.Published)
		{
			PublicationId = (await _db.Publications.SingleOrDefaultAsync(p => p.SubmissionId == Id))?.Id ?? 0;
		}

		return Page();
	}

	public async Task<IActionResult> OnGetDownload()
	{
		var submissionFile = await _db.Submissions
			.Where(s => s.Id == Id)
			.Select(s => s.MovieFile)
			.SingleOrDefaultAsync();

		if (submissionFile == null)
		{
			return NotFound();
		}

		return File(submissionFile, MediaTypeNames.Application.Octet, $"submission{Id}.zip");
	}
}
