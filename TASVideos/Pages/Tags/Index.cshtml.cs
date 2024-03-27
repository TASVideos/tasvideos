namespace TASVideos.Pages.Tags;

[RequirePermission(PermissionTo.TagMaintenance)]
public class IndexModel(ITagService tagService) : BasePageModel
{
	public ICollection<Tag> Tags { get; set; } = [];

	public async Task OnGet()
	{
		Tags = await tagService.GetAll();
	}
}
