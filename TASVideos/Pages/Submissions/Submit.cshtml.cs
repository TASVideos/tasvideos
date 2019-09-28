using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Extensions;
using TASVideos.MovieParsers;
using TASVideos.Pages.Submissions.Models;
using TASVideos.Services;
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.Submissions
{
	[RequirePermission(PermissionTo.SubmitMovies)]
	public class SubmitModel : SubmissionBasePageModel
	{
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
			: base(db)
		{
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

			// TODO: check warnings
			var parseResult = _parser.Parse(Create.MovieFile.OpenReadStream());
			await MapParsedResult(parseResult, submission);
			
			if (!ModelState.IsValid)
			{
				return Page();
			}

			submission.MovieFile = await FormFileToBytes(Create.MovieFile);

			submission.Submitter = await _userManager.GetUserAsync(User);

			Db.Submissions.Add(submission);
			await Db.SaveChangesAsync();

			await CreateSubmissionWikiPage(submission);

			Db.SubmissionAuthors.AddRange(await Db.Users
				.Where(u => Create.Authors.Contains(u.UserName))
				.Select(u => new SubmissionAuthor
				{
					SubmissionId = submission.Id,
					UserId = u.Id
				})
				.ToListAsync());

			submission.GenerateTitle();

			await CreateSubmissionTopic(submission.Id, submission.Title);
			_publisher.AnnounceSubmission(submission.Title, $"{BaseUrl}/{submission.Id}S");

			return Redirect($"/{submission.Id}S");
		}

		public async Task<IActionResult> OnGetPrefillText()
		{
			var page = await _wikiPages.Page("System/SubmissionDefaultMessage");
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
					$"{nameof(Create)}.{nameof(SubmissionCreateModel.Authors)}",
					"A submission must have at least one author"); // TODO: need to use the AtLeastOne attribute error message since it will be localized
			}

			if (!Create.MovieFile.IsZip())
			{
				ModelState.AddModelError($"{nameof(Create)}.{nameof(SubmissionCreateModel.MovieFile)}", "Not a valid .zip file");
			}

			if (Create.MovieFile.Length > 150 * 1024)
			{
				ModelState.AddModelError($"{nameof(Create)}.{nameof(SubmissionCreateModel.MovieFile)}", ".zip is too big, are you sure this is a valid movie file?");
			}

			foreach (var author in Create.Authors)
			{
				if (!await Db.Users.Exists(author))
				{
					ModelState.AddModelError($"{nameof(Create)}.{nameof(SubmissionCreateModel.Authors)}", $"Could not find user: {author}");
				}
			}
		}

		private async Task CreateSubmissionWikiPage(Submission submission)
		{
			var revision = new WikiPage
			{
				PageName = LinkConstants.SubmissionWikiPage + submission.Id,
				RevisionMessage = $"Auto-generated from Submission #{submission.Id}",
				Markup = Create.Markup,
				MinorEdit = false
			};
			await _wikiPages.Add(revision);
			submission.WikiContent = revision;
		}

		private async Task CreateSubmissionTopic(int id, string title)
		{
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

			// Create Topic in workbench
			var topic = new ForumTopic
			{
				CreateUserName = SiteGlobalConstants.TASVideoAgent,
				LastUpdateUserName = SiteGlobalConstants.TASVideoAgent,
				ForumId = ForumConstants.WorkBenchForumId,
				Title = title,
				PosterId = SiteGlobalConstants.TASVideoAgentId,
				PageName = LinkConstants.SubmissionWikiPage + id,
				Poll = poll
			};

			// Create first post
			var post = new ForumPost
			{
				CreateUserName = SiteGlobalConstants.TASVideoAgent,
				LastUpdateUserName = SiteGlobalConstants.TASVideoAgent,
				Topic = topic,
				PosterId = SiteGlobalConstants.TASVideoAgentId,
				Subject = title,
				Text = SiteGlobalConstants.NewSubmissionPost + $"<a href=\"/{id}S\">{title}</a>",
				EnableHtml = true,
				EnableBbCode = false,
				PosterMood = ForumPostMood.Normal
			};

			Db.ForumPolls.Add(poll);
			Db.ForumTopics.Add(topic);
			Db.ForumPosts.Add(post);
			await Db.SaveChangesAsync();

			poll.TopicId = topic.Id;
			await Db.SaveChangesAsync();
		}
	}
}
