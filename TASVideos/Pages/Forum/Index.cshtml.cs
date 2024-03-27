namespace TASVideos.Pages.Forum;

[AllowAnonymous]
public class IndexModel(IForumService forumService) : BasePageModel
{
	public IReadOnlyCollection<ForumCategoryDisplayDto> Categories { get; set; } = [];

	public async Task OnGet()
	{
		Categories = await forumService.GetAllCategories();
	}
}
