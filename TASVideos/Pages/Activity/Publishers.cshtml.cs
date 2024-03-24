using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;

namespace TASVideos.Pages.Activity;

[AllowAnonymous]
public class PublishersModel(ApplicationDbContext db) : BasePageModel
{
	public List<MovieEntryModel> Publications { get; set; } = [];

	[FromRoute]
	public string UserName { get; set; } = "";

	public async Task<IActionResult> OnGet()
	{
		if (string.IsNullOrWhiteSpace(UserName))
		{
			return NotFound();
		}

		var user = await db.Users.SingleOrDefaultAsync(u => u.UserName == UserName);
		if (user is null)
		{
			return NotFound();
		}

		Publications = await db.Publications
			.ThatHaveBeenPublishedBy(user.Id)
			.Select(s => new MovieEntryModel(
				s.Id,
				s.CreateTimestamp,
				s.Title,
				s.PublicationClass!.Name))
			.ToListAsync();

		return Page();
	}

	public record MovieEntryModel(int Id, DateTime CreateTimestamp, string Title, string Class);
}
