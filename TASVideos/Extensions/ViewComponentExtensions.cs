using TASVideos.Core;

namespace TASVideos.Extensions;

public static class ViewComponentExtensions
{
	public static PagingModel GetPagingModel(this ViewComponent viewComponent, int pageSizeDefault = 25)
	{
		return new PagingModel
		{
			Sort = viewComponent.Request.QueryStringValue("Sort"),
			PageSize = viewComponent.Request.QueryStringIntValue("PageSize") ?? pageSizeDefault,
			CurrentPage = viewComponent.Request.QueryStringIntValue("CurrentPage") ?? 1
		};
	}

	public static void SetPagingToViewData(this ViewComponent viewComponent, PagingModel paging)
	{
		viewComponent.ViewData["PagingModel"] = paging;
		viewComponent.ViewData["CurrentPage"] = viewComponent.HttpContext.Request.Path.Value;
	}
}
