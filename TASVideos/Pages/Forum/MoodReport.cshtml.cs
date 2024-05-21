using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum;

public class MoodReportModel(ApplicationDbContext db) : BasePageModel
{
	public static readonly ForumPostMood[] Moods = Enum.GetValues<ForumPostMood>();

	[FromRoute]
	public string? UserName { get; set; }

	public List<Entry> MoodyUsers { get; set; } = [];

	public async Task OnGet()
	{
		var query = db.Users
			.ThatHavePermission(PermissionTo.UseMoodAvatars)
			.Where(u => u.MoodAvatarUrlBase != null);

		if (!string.IsNullOrWhiteSpace(UserName))
		{
			query = query.ForUser(UserName);
		}

		MoodyUsers = await query
			.Select(u => new Entry(u.UserName, u.MoodAvatarUrlBase!))
			.ToListAsync();
	}

	public record Entry(string UserName, string MoodAvatarUrl);
}
