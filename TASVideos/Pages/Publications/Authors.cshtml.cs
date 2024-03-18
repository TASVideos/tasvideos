using Microsoft.AspNetCore.Authorization;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Publications.Models;

namespace TASVideos.Pages.Publications;

[AllowAnonymous]
public class AuthorsModel(ApplicationDbContext db) : BasePageModel
{
	public IEnumerable<AuthorListEntry> Authors { get; set; } = [];

	public async Task OnGet()
	{
		Authors = await db.Users
			.ThatArePublishedAuthors()
			.Select(u => new AuthorListEntry
			{
				Id = u.Id,
				UserName = u.UserName,
				ActivePublicationCount = u.Publications.Count(pa => !pa.Publication!.ObsoletedById.HasValue),
				ObsoletePublicationCount = u.Publications.Count(pa => pa.Publication!.ObsoletedById.HasValue)
			})
			.ToListAsync();
	}
}
