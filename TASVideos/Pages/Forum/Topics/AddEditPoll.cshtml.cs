using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Forum.Topics.Models;

namespace TASVideos.Pages.Forum.Topics
{
	[RequirePermission(PermissionTo.CreateForumPolls)]
	public class AddEditPollModel : BaseForumModel
	{
		private readonly ApplicationDbContext _db;

		public AddEditPollModel(ApplicationDbContext db) : base(db)
		{
			_db = db;
		}

		[FromRoute]
		public int TopicId { get; set; }

		public string TopicTitle { get; set; }

		public int? PollId { get; set; }

		[BindProperty]
		public PollCreateModel Poll { get; set; } = new PollCreateModel();

		public async Task<IActionResult> OnGet()
		{
			var topic = await _db.ForumTopics
				.Include(t => t.Poll)
				.ThenInclude(p => p.PollOptions)
				.ThenInclude(o => o.Votes)
				.Where(t => t.Id == TopicId)
				.SingleOrDefaultAsync();

			if (topic == null)
			{
				return NotFound();
			}

			TopicTitle = topic.Title;

			if (topic.Poll != null)
			{
				if (topic.Poll.PollOptions.SelectMany(o => o.Votes).Any())
				{
					return BadRequest("A poll with existing votes can not be modified");
				}

				Poll = new PollCreateModel
				{
					Question = topic.Poll.Question,
					DaysOpen = topic.Poll.CloseDate.HasValue
						? (int)(topic.Poll.CloseDate.Value - DateTime.Now).TotalDays
						: (int?)null,
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

			var topic = await _db.ForumTopics
				.Include(t => t.Poll)
				.ThenInclude(p => p.PollOptions)
				.ThenInclude(o => o.Votes)
				.Where(t => t.Id == TopicId)
				.SingleOrDefaultAsync();

			if (topic == null)
			{
				return NotFound();
			}

			if (topic.Poll != null)
			{
				if (topic.Poll.PollOptions.SelectMany(o => o.Votes).Any())
				{
					return BadRequest("A poll with existing votes can not be modified");
				}
				
				topic.Poll.Question = Poll.Question;
				topic.Poll.CloseDate = Poll.DaysOpen.HasValue
					? DateTime.UtcNow.AddDays(Poll.DaysOpen.Value)
					: (DateTime?)null;

				topic.Poll.PollOptions.Clear();
				topic.Poll = null;

				try
				{
					await _db.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					return BadRequest("Unable to clear existing poll");
				}
			}

			await CreatePoll(topic, Poll);

			return RedirectToPage("Index", new { Id = TopicId });
		}
	}
}
