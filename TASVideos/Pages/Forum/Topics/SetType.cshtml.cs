using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Services;

namespace TASVideos.Pages.Forum.Topics
{
	[RequirePermission(PermissionTo.SetTopicType)]
	public class SetTypeModel : BaseForumModel
	{
		private readonly ApplicationDbContext _db;

		public SetTypeModel(
			ApplicationDbContext db,
			ITopicWatcher watcher)
			: base(db, watcher)
		{
			_db = db;
		}

		[FromRoute]
		public int TopicId { get; set; }

		[BindProperty]
		public ForumTopicType Type { get; set; }

		[BindProperty]
		public string TopicTitle { get; set; } = "";

		[BindProperty]
		public int ForumId { get; set; }

		[BindProperty]
		public string ForumName { get; set; } = "";

		public async Task<IActionResult> OnGet()
		{
			var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);

			var topic = await _db.ForumTopics
				.Include(t => t.Forum)
				.ExcludeRestricted(seeRestricted)
				.Where(t => t.Id == TopicId)
				.SingleOrDefaultAsync();

			if (topic == null)
			{
				return NotFound();
			}

			TopicTitle = topic.Title;
			Type = topic.Type;
			ForumId = topic.ForumId;
			ForumName = topic.Forum!.Name;
			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			var seeRestricted = User.Has(PermissionTo.SeeRestrictedForums);

			var topic = await _db.ForumTopics
				.ExcludeRestricted(seeRestricted)
				.Where(t => t.Id == TopicId)
				.SingleOrDefaultAsync();

			if (topic == null)
			{
				return NotFound();
			}

			topic.Type = Type;

			try
			{
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				// TODO: do this through temp data
				return RedirectToPage("SetType", new { TopicId = topic.Id });
			}

			return RedirectToPage("Index", new { topic.Id });
		}
	}
}
