using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum;

public class MoodReportModel(ApplicationDbContext db) : BasePageModel
{
	public static readonly List<ForumPostMood> Moods = Enum
		.GetValues(typeof(ForumPostMood))
		.Cast<ForumPostMood>()
		.ToList();

	[FromRoute]
	public string? UserName { get; set; }

	public List<MoodReportEntry> MoodyUsers { get; set; } = [];

	public async Task OnGet()
	{
		var query = db.Users
			.ThatHavePermission(PermissionTo.UseMoodAvatars)
			.Where(u => u.MoodAvatarUrlBase != null)
			.Select(u => new MoodReportEntry(u.UserName, u.MoodAvatarUrlBase!));

		if (!string.IsNullOrWhiteSpace(UserName))
		{
			query = query.Where(q => q.UserName == UserName);
		}

		MoodyUsers = await query.ToListAsync();
	}

	public record MoodReportEntry(string UserName, string MoodAvatarUrl);
}
