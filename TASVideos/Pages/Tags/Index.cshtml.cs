using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Tags;

[RequirePermission(PermissionTo.TagMaintenance)]
public class IndexModel(ITagService tagService) : BasePageModel
{
	public IEnumerable<Tag> Tags { get; set; } = new List<Tag>();

	public async Task OnGet()
	{
		Tags = (await tagService.GetAll())
			.OrderBy(t => t.Code)
			.ToList();
	}
}
