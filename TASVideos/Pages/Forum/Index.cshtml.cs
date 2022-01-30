using Microsoft.AspNetCore.Authorization;
using TASVideos.Core.Services;

namespace TASVideos.Pages.Forum;

[AllowAnonymous]
public class IndexModel : BasePageModel
{
	private readonly IForumService _forumService;

	public IndexModel(IForumService forumService)
	{
		_forumService = forumService;
	}

	public ICollection<ForumCategoryDisplayDto> Categories { get; set; } = new List<ForumCategoryDisplayDto>();

	public async Task OnGet()
	{
		Categories = await _forumService.GetAllCategories();
	}
}
