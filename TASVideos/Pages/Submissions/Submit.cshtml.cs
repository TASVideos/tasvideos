using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.MovieParsers;
using TASVideos.Pages.Submissions.Models;
using TASVideos.Services;
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Submissions
{
	[RequirePermission(PermissionTo.SubmitMovies)]
	public class SubmitModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IWikiPages _wikiPages;
		private readonly ExternalMediaPublisher _publisher;
		private readonly MovieParser _parser;
		private readonly UserManager _userManager;

		public SubmitModel(
			ApplicationDbContext db,
			ExternalMediaPublisher publisher,
			IWikiPages wikiPages,
			MovieParser parser,
			UserManager userManager)
		{
			_db = db;
			_publisher = publisher;
			_wikiPages = wikiPages;
			_parser = parser;
			_userManager = userManager;
		}

		[BindProperty]
		public SubmissionCreateModel Create { get; set; }

		public void OnGet()
		{
			Create = new SubmissionCreateModel
			{
				Authors = new List<string> { User.Identity.Name }
			};
		}

		public async Task<IActionResult> OnPost()
		{
			await ValidateModel();

			if (!ModelState.IsValid)
			{
				return Page();
			}

			// TODO: set up auto-mapper, the v8 upgrade didn't like a default mapping
			var submission = new Submission
			{
				GameVersion = Create.GameVersion,
				GameName = Create.GameName,
				Branch = Create.Branch,
				RomName = Create.RomName,
				EmulatorVersion = Create.Emulator,
				EncodeEmbedLink = Create.EncodeEmbedLink
			};

			// Parse movie file
			// TODO: check warnings
			var parseResult = _parser.Parse(Create.MovieFile.OpenReadStream());

			if (!parseResult.Success)
			{
				foreach (var error in parseResult.Errors)
				{
					ModelState.AddModelError("", error);
				}
			}

			submission.MovieStartType = (int)parseResult.StartType;
			submission.Frames = parseResult.Frames;
			submission.RerecordCount = parseResult.RerecordCount;
			submission.MovieExtension = parseResult.FileExtension;
			submission.System = await _db.GameSystems.SingleOrDefaultAsync(g => g.Code == parseResult.SystemCode);

			if (submission.System == null)
			{
				ModelState.AddModelError("", $"Unknown system type of {parseResult.SystemCode}");
				return Page();
			}

			submission.Submitter = await _userManager.GetUserAsync(User);
			submission.SystemFrameRate = await _db.GameSystemFrameRates
				.SingleOrDefaultAsync(f => f.GameSystemId == submission.System.Id
					&& f.RegionCode == parseResult.Region.ToString());

			using (var memoryStream = new MemoryStream())
			{
				await Create.MovieFile.CopyToAsync(memoryStream);
				submission.MovieFile = memoryStream.ToArray();
			}

			_db.Submissions.Add(submission);
			await _db.SaveChangesAsync();

			// Create a wiki page corresponding to this submission
			var revision = new WikiPage
			{
				PageName = LinkConstants.SubmissionWikiPage + submission.Id,
				RevisionMessage = $"Auto-generated from Submission #{submission.Id}",
				Markup = Create.Markup,
				MinorEdit = false
			};
			await _wikiPages.Add(revision);
			submission.WikiContent = revision;

			// Add authors
			var users = await _db.Users
				.Where(u => Create.Authors.Contains(u.UserName))
				.ToListAsync();

			var submissionAuthors = users.Select(u => new SubmissionAuthor
			{
				SubmissionId = submission.Id,
				UserId = u.Id
			});

			_db.SubmissionAuthors.AddRange(submissionAuthors);

			submission.GenerateTitle();

			var poll = new ForumPoll
			{
				CreateUserName = SiteGlobalConstants.TASVideoAgent,
				LastUpdateUserName = SiteGlobalConstants.TASVideoAgent,
				Question = SiteGlobalConstants.PollQuestion,
				PollOptions = new[]
				{
					new ForumPollOption { Text = SiteGlobalConstants.PollOptionNo, Ordinal = 0 },
					new ForumPollOption { Text = SiteGlobalConstants.PollOptionYes, Ordinal = 1 },
					new ForumPollOption { Text = SiteGlobalConstants.PollOptionsMeh, Ordinal = 2 }
				}
			};

			_db.ForumPolls.Add(poll);

			await _db.SaveChangesAsync();

			// Create Topic in workbench
			var topic = new ForumTopic
			{
				CreateUserName = SiteGlobalConstants.TASVideoAgent,
				LastUpdateUserName = SiteGlobalConstants.TASVideoAgent,
				ForumId = ForumConstants.WorkBenchForumId,
				Title = submission.Title,
				PosterId = SiteGlobalConstants.TASVideoAgentId,
				PageName = LinkConstants.SubmissionWikiPage + submission.Id,
				PollId = poll.Id,
			};

			// Create first post
			var post = new ForumPost
			{
				CreateUserName = SiteGlobalConstants.TASVideoAgent,
				LastUpdateUserName = SiteGlobalConstants.TASVideoAgent,
				Topic = topic,
				PosterId = SiteGlobalConstants.TASVideoAgentId,
				Subject = submission.Title,
				Text = SiteGlobalConstants.NewSubmissionPost + $"<a href=\"/{submission.Id}S\">{submission.Title}</a>",
				EnableHtml = true,
				EnableBbCode = false
			};

			_db.ForumTopics.Add(topic);
			_db.ForumPosts.Add(post);
			await _db.SaveChangesAsync();

			poll.TopicId = topic.Id;
			await _db.SaveChangesAsync();

			_publisher.AnnounceSubmission(submission.Title, $"{BaseUrl}/{submission.Id}S");

			return Redirect($"/{submission.Id}S");
		}

		public IActionResult OnGetPrefillText()
		{
			var page = _wikiPages.Page("System/SubmissionDefaultMessage");
			return new JsonResult(new { text = page.Markup });
		}

		private async Task ValidateModel()
		{
			Create.Authors = Create.Authors
				.Where(a => !string.IsNullOrWhiteSpace(a))
				.ToList();

			if (!Create.Authors.Any())
			{
				ModelState.AddModelError(
					nameof(SubmissionCreateModel.Authors),
					"A submission must have at least one author"); // TODO: need to use the AtLeastOne attribute error message since it will be localized
			}

			if (!Create.MovieFile.FileName.EndsWith(".zip")
			|| Create.MovieFile.ContentType != "application/x-zip-compressed")
			{
				ModelState.AddModelError(nameof(SubmissionCreateModel.MovieFile), "Not a valid .zip file");
			}

			if (Create.MovieFile.Length > 150 * 1024)
			{
				ModelState.AddModelError(nameof(SubmissionCreateModel.MovieFile), ".zip is too big, are you sure this is a valid movie file?");
			}

			foreach (var author in Create.Authors)
			{
				if (!await _db.Users.Exists(author))
				{
					ModelState.AddModelError(nameof(SubmissionCreateModel.Authors), $"Could not find user: {author}");
				}
			}
		}
	}
}
