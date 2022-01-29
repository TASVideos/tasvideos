﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;

namespace TASVideos.Pages.Messages;

[Authorize]
public class IndexModel : BasePageModel
{
	private readonly UserManager _userManager;

	public IndexModel(UserManager userManager)
	{
		_userManager = userManager;
	}

	[FromRoute]
	public int Id { get; set; }

	public PrivateMessageDto PrivateMessage { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		var message = await _userManager.GetMessage(User.GetUserId(), Id);

		if (message == null)
		{
			return NotFound();
		}

		PrivateMessage = message;
		return Page();
	}
}
