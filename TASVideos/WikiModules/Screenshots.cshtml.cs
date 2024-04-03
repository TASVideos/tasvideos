using TASVideos.Core;
using TASVideos.WikiEngine;

namespace TASVideos.WikiModules;

[WikiModule(ModuleNames.Screenshots)]
public class Screenshots(ApplicationDbContext db) : WikiViewComponent
{
	public ScreenshotPageOf<ScreenshotEntry> List { get; set; } = null!;

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var paging = this.GetPagingModel();
		if (string.IsNullOrWhiteSpace(paging.Sort))
		{
			paging.Sort = "-PublicationId";
		}

		var query = db.PublicationFiles
			.Where(pf => pf.Type == FileType.Screenshot);

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
				PublicationId = p.PublicationId,
				PublicationTitle = p.Publication!.Title,
				Description = p.Description ?? "",
				Path = p.Path
			})
			.SortedPageOf(paging);

		this.SetPagingToViewData(paging);

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
		[Display(Name = "Screenshot")]
		public string Path { get; init; } = "";

		[Sortable]
		[Display(Name = "Id")]
		public int PublicationId { get; init; }

		[Sortable]
		[Display(Name = "Title")]
		public string PublicationTitle { get; init; } = "";

		[Sortable]
		public string Description { get; init; } = "";
	}

	public class ScreenshotPageOf<T>(IEnumerable<T> items) : PageOf<T>(items)
	{
		public bool? OnlyDescriptions { get; set; }
	}
}
