using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.PublicationsByTag)]
public class PublicationsByTag : ViewComponent
{
	private readonly ITagService _tags;

	public PublicationsByTag(ITagService tags)
	{
		_tags = tags;
	}

	public async Task<IViewComponentResult> InvokeAsync()
	{
		var tags = await _tags.GetAll();
		return View(tags);
	}
}
