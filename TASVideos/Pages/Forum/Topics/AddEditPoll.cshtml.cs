using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum.Topics;

[RequirePermission(PermissionTo.CreateForumPolls)]
public class AddEditPollModel(ApplicationDbContext db, IForumService forumService) : BaseForumModel
{
	[FromRoute]
	public int TopicId { get; set; }

	public string TopicTitle { get; set; } = "";

	public int? PollId { get; set; }

	[BindProperty]
	public PollCreate Poll { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var topic = await db.ForumTopics
			.Include(t => t.Poll)
			.ThenInclude(p => p!.PollOptions)
			.ThenInclude(o => o.Votes)
			.ExcludeRestricted(UserCanSeeRestricted)
			.SingleOrDefaultAsync(t => t.Id == TopicId);

		if (topic is null)
		{
			return NotFound();
		}

		TopicTitle = topic.Title;

		if (topic.Poll is not null)
		{
			Poll = new PollCreate
			{
				MultiSelect = topic.Poll.MultiSelect,
				Question = topic.Poll.Question,
				DaysOpen = topic.Poll.CloseDate.HasValue
					? (int)(topic.Poll.CloseDate.Value - DateTime.UtcNow).TotalDays
					: null,
				PollOptions = topic.Poll.PollOptions
					.OrderBy(o => o.Ordinal)
					.Select(o => o.Text)
					.ToList(),
				HasVotes = topic.Poll.PollOptions.SelectMany(o => o.Votes).Any()
			};

			PollId = topic.PollId;
		}

		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (string.IsNullOrEmpty(Poll.Question))
		{
			ModelState.AddModelError($"{nameof(Poll)}.{nameof(Poll.Question)}", "The Question field is required.");
		}

		if (!Poll.OptionsAreValid)
		{
			ModelState.AddModelError($"{nameof(Poll)}.{nameof(Poll.PollOptions)}", "Enter at least 2 options. Each option must be a string with a maximum length of 250.");
			return Page();
		}

		if (!ModelState.IsValid)
		{
			return Page();
		}

		var topic = await db.ForumTopics
			.Include(t => t.Poll)
			.ThenInclude(p => p!.PollOptions)
			.ThenInclude(o => o.Votes)
			.ExcludeRestricted(UserCanSeeRestricted)
			.Where(t => t.Id == TopicId)
			.SingleOrDefaultAsync();

		if (topic is null)
		{
			return NotFound();
		}

		if (topic.Poll is not null)
		{
			topic.Poll.CloseDate = Poll.DaysOpen.HasValue
				? DateTime.UtcNow.AddDays(Poll.DaysOpen.Value)
				: null;

			var hasVotes = topic.Poll.PollOptions.SelectMany(o => o.Votes).Any();
			if (!hasVotes)
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

			SetMessage(await db.TrySaveChanges(), "Poll edited", "Unable to clear existing poll");
		}
		else
		{
			await forumService.CreatePoll(
				topic,
				new Core.Services.PollCreate(Poll.Question, Poll.DaysOpen, Poll.MultiSelect, Poll.PollOptions));
		}

		return RedirectToPage("Index", new { Id = TopicId });
	}

	public class PollCreate
	{
		[StringLength(200, MinimumLength = 8)]
		public string? Question { get; init; }

		[Range(0, 365)]
		public int? DaysOpen { get; init; }

		public bool MultiSelect { get; init; }

		public List<string> PollOptions { get; init; } = ["", ""];

		public bool IsValid =>
			!string.IsNullOrWhiteSpace(Question)
			&& Question.Length <= 200
			&& OptionsAreValid;

		public bool OptionsAreValid =>
			PollOptions.Count(o => !string.IsNullOrWhiteSpace(o)) > 1
			&& PollOptions.All(o => o.Length <= 250);

		public bool HasAnyField => !string.IsNullOrWhiteSpace(Question)
			|| DaysOpen.HasValue
			|| PollOptions.Any(o => !string.IsNullOrWhiteSpace(o));

		public bool HasVotes { get; set; }
	}
}
