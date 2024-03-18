using Microsoft.AspNetCore.Mvc;
using TASVideos.Core;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.Screenshots)]
public class Screenshots(ApplicationDbContext db) : ViewComponent
{
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
				Path = p.Path,
			})
			.SortedPageOf(paging);

		this.SetPagingToViewData(paging);

		var model = new ScreenshotPageOf<ScreenshotEntry>(screenshots)
		{
			PageSize = screenshots.PageSize,
			CurrentPage = screenshots.CurrentPage,
			RowCount = screenshots.RowCount,
			Sort = screenshots.Sort,
			OnlyDescriptions = onlyDescriptions
		};

		return View(model);
	}
}
