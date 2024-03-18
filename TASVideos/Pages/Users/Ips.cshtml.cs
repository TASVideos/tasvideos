using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Users;

[RequirePermission(PermissionTo.ViewPrivateUserData)]
public class IpsModel(ApplicationDbContext db) : BasePageModel
{
	[FromRoute]
	public string UserName { get; set; } = "";

	public IReadOnlyCollection<IpEntry> Ips { get; set; } = new List<IpEntry>();

	public record IpEntry(string IpAddress, DateTime UsedOn);

	public async Task<IActionResult> OnGet()
	{
		var user = await db.Users.SingleOrDefaultAsync(u => u.UserName == UserName);
		if (user is null)
		{
			return NotFound();
		}

		var postIps = await db.ForumPosts
			.Where(p => p.PosterId == user.Id)
			.Where(p => p.IpAddress != null)
			.Select(p => new IpEntry(p.IpAddress ?? "", p.LastUpdateTimestamp))
			.Distinct()
			.ToListAsync();

		var voteIps = await db.ForumPollOptionVotes
			.Where(v => v.UserId == user.Id)
			.Where(p => p.IpAddress != null)
			.Select(p => new IpEntry(p.IpAddress ?? "", p.CreateTimestamp))
			.Distinct()
			.ToListAsync();

		Ips = postIps
			.Concat(voteIps)
			.Distinct()
			.ToList();

		return Page();
	}
}
