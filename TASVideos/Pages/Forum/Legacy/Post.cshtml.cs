using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Forum.Legacy;

// Handles legacy forum links to viewTopic.php
[AllowAnonymous]
public class PostModel : BaseForumModel
{
	[FromRoute]
	public int? Id { get; set; }

	public IActionResult OnGet()
	{
		return BasePageRedirect("/Forum/Posts/Index", new { Id });
	}
}
