using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TASVideos.Pages.Forum.Legacy;

// Handles legacy forum links to moodreport.php
[AllowAnonymous]
public class MoodReportModel : BaseForumModel
{
	public IActionResult OnGet()
	{
		return BasePageRedirect("/Forum/MoodReport");
	}
}
