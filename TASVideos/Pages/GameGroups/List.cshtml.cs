namespace TASVideos.Pages.GameGroups;

[AllowAnonymous]
public class ListModel(ApplicationDbContext db) : BasePageModel
{
	public List<GroupListEntry> GameGroups { get; set; } = [];

	public async Task OnGet()
	{
		GameGroups = await db.GameGroups
			.Select(gg => new GroupListEntry(gg.Id, gg.Name))
			.ToListAsync();
	}

	public record GroupListEntry(int Id, string Name);
}
