using Microsoft.AspNetCore.Authorization;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Publications;

[AllowAnonymous]
public class AuthorsModel(ApplicationDbContext db) : BasePageModel
{
	public List<AuthorListEntry> Authors { get; set; } = [];

	public async Task OnGet()
	{
		Authors = await db.Users
			.ThatArePublishedAuthors()
			.Select(u => new AuthorListEntry(
				u.Id,
				u.UserName,
				u.Publications.Count(pa => !pa.Publication!.ObsoletedById.HasValue),
				u.Publications.Count(pa => pa.Publication!.ObsoletedById.HasValue)))
			.ToListAsync();
	}

	public record AuthorListEntry(int Id, string Author, int ActivePubCount, int ObsoletePubCount);
}
