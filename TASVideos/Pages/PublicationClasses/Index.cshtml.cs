using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.PublicationClasses;

[RequirePermission(PermissionTo.ClassMaintenance)]
public class IndexModel : BasePageModel
{
	private readonly IClassService _classService;

	public IndexModel(IClassService classService)
	{
		_classService = classService;
	}

	public IEnumerable<PublicationClass> Classes { get; set; } = new List<PublicationClass>();

	public async Task OnGet()
	{
		Classes = await _classService.GetAll();
	}
}
