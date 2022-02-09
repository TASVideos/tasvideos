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
	public int? Id { get; set; }

	public WikiPage PageData { get; set; } = new();

	public async Task<IActionResult> OnPost()
	{
		Markup = await new StreamReader(Request.Body, Encoding.UTF8).ReadToEndAsync();
		if (Id.HasValue)
		{
			var pageData = await _pages.Revision(Id.Value);
			if (pageData == null)
			{
				return NotFound();
			}

			PageData = pageData;
		}

		return Page();
	}
}
