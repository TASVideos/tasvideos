using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Tags;

[RequirePermission(PermissionTo.TagMaintenance)]
public class IndexModel : BasePageModel
{
	private readonly ITagService _tagService;

	public IndexModel(ITagService tagService)
	{
		_tagService = tagService;
	}

	public IEnumerable<Tag> Tags { get; set; } = new List<Tag>();

	public async Task OnGet()
	{
		Tags = (await _tagService.GetAll())
			.OrderBy(t => t.Code)
			.ToList();
	}
}
