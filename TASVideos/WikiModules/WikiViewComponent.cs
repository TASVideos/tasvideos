using System.Globalization;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace TASVideos.WikiModules;

public abstract class WikiViewComponent : ViewComponent
{
	private string WikiViewPath => string.Format(CultureInfo.InvariantCulture, "/WikiModules/{0}.cshtml", GetType().Name);

	public new ViewViewComponentResult View()
	{
		return View(viewName: WikiViewPath, model: this);
	}

	public ContentViewComponentResult Empty() => new("");
	public ContentViewComponentResult Error(string str) => new($"<<< Error: {str} >>>");
	public ContentViewComponentResult String(string str) => new(str);

	protected int DefaultPageSize { get; set; } = 25;
	protected string? DefaultSort { get; set; }

	public PagingModel GetPaging()
	{
		string? sort = Request.QueryStringValue("Sort");
		if (string.IsNullOrWhiteSpace(sort))
		{
			sort = DefaultSort;
		}

		return new PagingModel
		{
			Sort = sort,
			PageSize = Request.QueryStringIntValue("PageSize") ?? DefaultPageSize,
			CurrentPage = Request.QueryStringIntValue("CurrentPage") ?? 1
		};
	}

	public string CurrentPage => Request.Path.Value?.Trim('/') ?? "";
}
