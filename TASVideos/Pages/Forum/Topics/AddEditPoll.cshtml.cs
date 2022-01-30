using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Topics.Models;

namespace TASVideos.Pages.Forum.Topics;

[RequirePermission(PermissionTo.CreateForumPolls)]
public class AddEditPollModel : BaseForumModel
{
	private readonly ApplicationDbContext _db;
	private readonly IForumService _forumService;

	public AddEditPollModel(ApplicationDbContext db, IForumService forumService)
	{
		_db = db;
		_forumService = forumService;
	}

	[FromRoute]
	public int TopicId { get; set; }

	public string TopicTitle { get; set; } = "";

	public int? PollId { get; set; }

	public bool AnyVotes { get; set; }

	[BindProperty]
	public PollCreateModel Poll { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);

		var topic = await _db.ForumTopics
			.Include(t => t.Poll)
			.ThenInclude(p => p!.PollOptions)
			.ThenInclude(o => o.Votes)
			.ExcludeRestricted(seeRestricted)
			.Where(t => t.Id == TopicId)
			.SingleOrDefaultAsync();

		if (topic == null)
		{
			return NotFound();
		}

		TopicTitle = topic.Title;

		if (topic.Poll is not null)
		{
			AnyVotes = topic.Poll.PollOptions.SelectMany(o => o.Votes).Any();

			Poll = new PollCreateModel
			{
				MultiSelect = topic.Poll.MultiSelect,
				Question = topic.Poll.Question,
				DaysOpen = topic.Poll.CloseDate.HasValue
					? (int)(topic.Poll.CloseDate.Value - DateTime.UtcNow).TotalDays
					: null,
				PollOptions = topic.Poll.PollOptions
					.OrderBy(o => o.Ordinal)
					.Select(o => o.Text)
					.ToList()
			};

			PollId = topic.PollId;
		}

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		if (!Poll.OptionsAreValid)
		{
			ModelState.AddModelError($"{nameof(Poll.PollOptions)}", "Invalid poll options");
			return Page();
		}

		var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);

		var topic = await _db.ForumTopics
			.Include(t => t.Poll)
			.ThenInclude(p => p!.PollOptions)
			.ThenInclude(o => o.Votes)
			.ExcludeRestricted(seeRestricted)
			.Where(t => t.Id == TopicId)
			.SingleOrDefaultAsync();

		if (topic == null)
		{
			return NotFound();
		}

		if (topic.Poll is not null)
		{
			AnyVotes = topic.Poll.PollOptions.SelectMany(o => o.Votes).Any();

			topic.Poll.CloseDate = Poll.DaysOpen.HasValue
				? DateTime.UtcNow.AddDays(Poll.DaysOpen.Value)
				: null;

			if (!AnyVotes)
			{
				topic.Poll.MultiSelect = Poll.MultiSelect;
				topic.Poll.Question = Poll.Question ?? "";
				topic.Poll.PollOptions.Clear();
				topic.Poll.PollOptions.AddRange(Poll.PollOptions
					.Select((po, i) => new ForumPollOption
					{
						Text = po,
						Ordinal = i
					}));
			}

			await ConcurrentSave(_db, "Poll edited", "Unable to clear existing poll");
		}
		else
		{
			await _forumService.CreatePoll(
				topic,
				new PollCreateDto(Poll.Question, Poll.DaysOpen, Poll.MultiSelect, Poll.PollOptions));
		}

		return RedirectToPage("Index", new { Id = TopicId });
	}
}
