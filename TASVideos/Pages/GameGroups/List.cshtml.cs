namespace TASVideos.Pages.GameGroups;

[AllowAnonymous]
public class ListModel(ApplicationDbContext db) : BasePageModel
{
	public List<GroupEntry> GameGroups { get; set; } = [];

	public async Task OnGet()
	{
		GameGroups = await db.GameGroups
			.Select(gg => new GroupEntry(gg.Id, gg.Name))
			.ToListAsync();
	}

	public record GroupEntry(int Id, string Name);
}
