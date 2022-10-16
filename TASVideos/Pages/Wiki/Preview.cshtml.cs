using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Wiki;

[AllowAnonymous]
[IgnoreAntiforgeryToken]
public class PreviewModel : BasePageModel
{
	private readonly IWikiPages _pages;

	public PreviewModel(IWikiPages pages)
	{
		_pages = pages;
	}

	public string Markup { get; set; } = "";

	[FromQuery]
	public string? Path { get; set; }

	public IWikiPage PageData { get; set; } = null!;

	public async Task<IActionResult> OnPost()
	{
		Markup = await new StreamReader(Request.Body, Encoding.UTF8).ReadToEndAsync();
		if (Path is not null)
		{
			var pageData = await _pages.Page(Path);
			if (pageData is null)
			{
				return NotFound();
			}

			PageData = pageData;
		}

		return Page();
	}
}
