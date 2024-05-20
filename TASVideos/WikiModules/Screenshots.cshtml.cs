using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.Screenshots)]
public class Screenshots(ApplicationDbContext db) : WikiViewComponent
{
	public ScreenshotPageOf<ScreenshotEntry> List { get; set; } = null!;

	public async Task<IViewComponentResult> InvokeAsync()
	{
		DefaultSort = "-PublicationId";

		var query = db.PublicationFiles.Where(pf => pf.Type == FileType.Screenshot);

		var onlyDescriptions = Request.QueryStringBoolValue("OnlyDescriptions");
		if (onlyDescriptions == true)
		{
			query = query.Where(pf => !string.IsNullOrEmpty(pf.Description));
		}
		else if (onlyDescriptions == false)
		{
			query = query.Where(pf => string.IsNullOrEmpty(pf.Description));
		}

		var screenshots = await query
			.Select(p => new ScreenshotEntry
			{
				Id = p.PublicationId,
				Title = p.Publication!.Title,
				Description = p.Description ?? "",
				Screenshot = p.Path
			})
			.SortedPageOf(GetPaging());

		List = new ScreenshotPageOf<ScreenshotEntry>(screenshots)
		{
			PageSize = screenshots.PageSize,
			CurrentPage = screenshots.CurrentPage,
			RowCount = screenshots.RowCount,
			Sort = screenshots.Sort,
			OnlyDescriptions = onlyDescriptions
		};

		return View();
	}

	public class ScreenshotEntry
	{
		public string Screenshot { get; init; } = "";

		[Sortable]
		public int Id { get; init; }

		[Sortable]
		public string Title { get; init; } = "";

		[Sortable]
		public string Description { get; init; } = "";
	}

	public class ScreenshotPageOf<T>(IEnumerable<T> items) : PageOf<T>(items)
	{
		public bool? OnlyDescriptions { get; set; }
	}
}
