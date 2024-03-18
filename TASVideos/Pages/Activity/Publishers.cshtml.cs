using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.Activity.Model;

namespace TASVideos.Pages.Activity;

[AllowAnonymous]
public class PublishersModel(ApplicationDbContext db) : BasePageModel
{
	public IEnumerable<MovieEntryModel> Publications { get; set; } = [];

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
			.Where(p => p.Submission!.PublisherId == user.Id)
			.Select(s => new MovieEntryModel
			{
				Id = s.Id,
				CreateTimestamp = s.CreateTimestamp,
				Title = s.Title,
				Class = s.PublicationClass!.Name
			})
			.ToListAsync();

		return Page();
	}
}
