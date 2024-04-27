namespace TASVideos.Pages.Forum;

[RequirePermission(PermissionTo.EditCategories)]
public class EditModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public CategoryEdit Category { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var category = await db.ForumCategories
			.Where(c => c.Id == Id)
			.Select(c => new CategoryEdit
			{
				Title = c.Title,
				Description = c.Description,
				Forums = c.Forums
					.OrderBy(f => f.Ordinal)
					.Select(f => new CategoryEdit.ForumEdit
					{
						Id = f.Id,
						Name = f.Name,
						Description = f.Description,
						Ordinal = f.Ordinal
					})
					.ToList()
			})
			.SingleOrDefaultAsync();

		if (category is null)
		{
			return NotFound();
		}

		Category = category;
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var category = await db.ForumCategories
			.Include(c => c.Forums)
			.SingleOrDefaultAsync(c => c.Id == Id);

		if (category is null)
		{
			return NotFound();
		}

		category.Title = Category.Title;
		category.Description = Category.Description;

		foreach (var forum in category.Forums)
		{
			// This is an n squared problem, but we don't anticipate enough forums in a single category to be a performance issue
			// This could be optimized away by joining model.Forums against category.Forums then looping
			var forumModel = Category.Forums.Single(f => f.Id == forum.Id);
			forum.Ordinal = forumModel.Ordinal;
		}

		await db.SaveChangesAsync();
		return BasePageRedirect("Index");
	}

	public class CategoryEdit
	{
		[StringLength(30)]
		public string Title { get; init; } = "";
		public string? Description { get; init; }
		public List<ForumEdit> Forums { get; init; } = [];

		public class ForumEdit
		{
			public int Id { get; init; }

			[StringLength(50)]
			public string Name { get; init; } = "";

			[StringLength(1000)]
			public string? Description { get; init; }
			public int Ordinal { get; init; }
		}
	}
}
