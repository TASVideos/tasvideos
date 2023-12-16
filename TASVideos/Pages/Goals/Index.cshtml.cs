using Microsoft.AspNetCore.Authorization;
using TASVideos.Data;

namespace TASVideos.Pages.Goals;

[AllowAnonymous]
public class IndexModel : BasePageModel
{
	private readonly ApplicationDbContext _db;

	public IndexModel(ApplicationDbContext db)
	{
		_db = db;
	}

	public record GoalListEntry(int Id, string Name, int GameCount);

	public List<GoalListEntry> Goals { get; set; } = new();

	public async Task OnGet()
	{
		Goals = await _db.Goals
			.Select(g => new GoalListEntry(g.Id, g.DisplayName, g.GameGoals.Count))
			.ToListAsync();
	}
}
