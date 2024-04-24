namespace TASVideos.Pages.Publications;

[AllowAnonymous]
public class AuthorsModel(ApplicationDbContext db) : BasePageModel
{
	public List<AuthorEntry> Authors { get; set; } = [];

	public async Task OnGet()
	{
		Authors = await db.Users
			.ThatArePublishedAuthors()
			.Select(u => new AuthorEntry(
				u.Id,
				u.UserName,
				u.Publications.Count(pa => !pa.Publication!.ObsoletedById.HasValue),
				u.Publications.Count(pa => pa.Publication!.ObsoletedById.HasValue)))
			.ToListAsync();
	}

	public record AuthorEntry(int Id, string Author, int ActivePubCount, int ObsoletePubCount);
}
