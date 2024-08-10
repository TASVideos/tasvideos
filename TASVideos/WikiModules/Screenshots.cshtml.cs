using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.Screenshots)]
public class Screenshots(ApplicationDbContext db) : WikiViewComponent
{
	public PageOf<ScreenshotEntry> List { get; set; } = null!;

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

		List = await query
			.Select(p => new ScreenshotEntry
			{
				Id = p.PublicationId,
				Title = p.Publication!.Title,
				Description = p.Description ?? "",
				Screenshot = p.Path
			})
			.SortedPageOf(GetPaging());

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
}
