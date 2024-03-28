namespace TASVideos.Pages.PublicationClasses;

[RequirePermission(PermissionTo.ClassMaintenance)]
public class IndexModel(IClassService classService) : BasePageModel
{
	public IReadOnlyCollection<PublicationClass> Classes { get; set; } = [];

	public async Task OnGet()
	{
		Classes = await classService.GetAll();
	}
}
