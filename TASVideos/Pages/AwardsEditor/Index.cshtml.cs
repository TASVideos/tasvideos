namespace TASVideos.Pages.AwardsEditor;

[RequirePermission(PermissionTo.CreateAwards)]
public class IndexModel(IAwards awards) : BasePageModel
{
	[FromRoute]
	public int? Year { get; set; }

	public ICollection<AwardAssignment> Assignments { get; set; } = [];

	public async Task<IActionResult> OnGet()
	{
		if (!Year.HasValue)
		{
			return BasePageRedirect("Index", new { DateTime.UtcNow.Year });
		}

		Assignments = await awards.ForYear(Year.Value);

		return Page();
	}
}
