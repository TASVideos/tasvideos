using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents;

[WikiModule(WikiModules.PublicationsByTag)]
public class PublicationsByTag(ITagService tags) : ViewComponent
{
	public async Task<IViewComponentResult> InvokeAsync()
	{
		var tags1 = await tags.GetAll();
		return View(tags1);
	}
}
