using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Forum
{
	[RequirePermission(PermissionTo.EditCategories)]
	public class EditModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		public EditModel(
			ApplicationDbContext db,
			UserTasks userTasks)
			: base(userTasks)
		{
			_db = db;
		}

		[FromRoute]
		public int Id { get; set; }

		public CategoryEditModel Category { get; set; }

		public async Task<IActionResult> OnGet()
		{
			Category = await _db.ForumCategories
				.Where(c => c.Id == Id)
				.Select(c => new CategoryEditModel
				{
					Id = c.Id,
					Title = c.Title,
					Description = c.Description,
					Forums = c.Forums
						.OrderBy(f => f.Ordinal)
						.Select(f => new CategoryEditModel.ForumEditModel
						{
							Id = f.Id,
							Name = f.Name,
							Description = f.Description,
							Ordinal = f.Ordinal
						})
						.ToList()
				})
				.SingleOrDefaultAsync();

			if (Category == null)
			{
				return NotFound();
			}

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			var category = await _db.ForumCategories
				.Include(c => c.Forums)
				.SingleOrDefaultAsync(c => c.Id == Category.Id);

			if (category == null)
			{
				return NotFound();
			}

			category.Title = Category.Title;
			category.Description = Category.Description;

			foreach (var forum in category.Forums)
			{
				// This is a n squared problem but we don't anticipate enough forums in a single category to be a performance issue
				// This could be optimized away by joining model.Forums against category.Forums then looping
				var forumModel = Category.Forums.Single(f => f.Id == forum.Id);
				forum.Ordinal = forumModel.Ordinal;
			}

			await _db.SaveChangesAsync();
			return RedirectToPage("Index");
		}
	}
}
