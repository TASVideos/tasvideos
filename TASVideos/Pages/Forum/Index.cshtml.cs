﻿using Microsoft.AspNetCore.Authorization;
using TASVideos.Core.Services;

namespace TASVideos.Pages.Forum;

[AllowAnonymous]
public class IndexModel(IForumService forumService) : BasePageModel
{
	public IReadOnlyCollection<ForumCategoryDisplayDto> Categories { get; set; } = new List<ForumCategoryDisplayDto>();

	public async Task OnGet()
	{
		Categories = await forumService.GetAllCategories();
	}
}
