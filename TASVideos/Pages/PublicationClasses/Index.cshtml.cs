using TASVideos.Core.Services;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.PublicationClasses;

[RequirePermission(PermissionTo.ClassMaintenance)]
public class IndexModel(IClassService classService) : BasePageModel
{
	public IEnumerable<PublicationClass> Classes { get; set; } = new List<PublicationClass>();

	public async Task OnGet()
	{
		Classes = await classService.GetAll();
	}
}
