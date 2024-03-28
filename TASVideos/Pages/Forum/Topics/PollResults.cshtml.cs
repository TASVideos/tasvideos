namespace TASVideos.Pages.Forum.Topics;

[RequirePermission(PermissionTo.SeePollResults)]
public class PollResultsModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	public PollResult Poll { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var poll = await db.ForumPolls
			.Where(p => p.Id == Id)
			.Select(p => new PollResult
			{
				TopicTitle = p.Topic!.Title,
				TopicId = p.TopicId,
				Question = p.Question,
				Votes = p.PollOptions
					.SelectMany(po => po.Votes)
					.Select(v => new PollResult.VoteResult(
						v.UserId,
						v.User!.UserName,
						v.PollOption!.Ordinal,
						v.PollOption.Text,
						v.CreateTimestamp,
						v.IpAddress))
					.ToList()
			})
			.SingleOrDefaultAsync();

		if (poll is null)
		{
			return NotFound();
		}

		Poll = poll;
		return Page();
	}

	public async Task<IActionResult> OnPostResetVote(int userId)
	{
		if (!User.Has(PermissionTo.ResetPollResults))
		{
			return AccessDenied();
		}

		var poll = await db.ForumPolls
			.Include(p => p.PollOptions)
			.ThenInclude(o => o.Votes)
			.Where(p => p.Id == Id)
			.SingleOrDefaultAsync();

		if (poll is null)
		{
			return NotFound();
		}

		var votes = poll.PollOptions
			.SelectMany(o => o.Votes)
			.Where(v => v.UserId == userId)
			.ToList();

		db.ForumPollOptionVotes.RemoveRange(votes);

		await ConcurrentSave(db, "Poll reset", "Unable to reset poll");
		return RedirectToPage("PollResults", new { Id });
	}

	public class PollResult
	{
		public string TopicTitle { get; init; } = "";
		public int TopicId { get; init; }
		public string Question { get; init; } = "";
		public List<VoteResult> Votes { get; init; } = [];
		public record VoteResult(int UserId, string UserName, int Ordinal, string OptionText, DateTime CreateTimestamp, string? IpAddress);
	}
}
