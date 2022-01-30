using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;

namespace TASVideos.Pages.Forum;

public class MoodReportModel : PageModel
{
	public static readonly IEnumerable<ForumPostMood> Moods = Enum
		.GetValues(typeof(ForumPostMood))
		.Cast<ForumPostMood>()
		.ToList();

	private readonly ApplicationDbContext _db;

	public MoodReportModel(ApplicationDbContext db)
	{
		_db = db;
	}

	[FromRoute]
	public string? UserName { get; set; }

	public IEnumerable<MoodReportEntry> MoodyUsers { get; set; } = new List<MoodReportEntry>();

	public async Task OnGet()
	{
		var query = _db.Users
			.ThatHavePermission(PermissionTo.UseMoodAvatars)
			.Where(u => u.MoodAvatarUrlBase != null)
			.Select(u => new MoodReportEntry
			{
				UserName = u.UserName,
				MoodAvatarUrl = u.MoodAvatarUrlBase!
			});

		if (!string.IsNullOrWhiteSpace(UserName))
		{
			query = query.Where(q => q.UserName == UserName);
		}

		MoodyUsers = await query.ToListAsync();
	}

	public class MoodReportEntry
	{
		public string UserName { get; set; } = "";
		public string MoodAvatarUrl { get; set; } = "";
	}
}
