using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;

namespace TASVideos.Pages.Account;

[Authorize]
[IgnoreAntiforgeryToken]
public class LogoutModel : BasePageModel
{
	private readonly SignInManager _signInManager;

	public LogoutModel(SignInManager signInManager)
	{
		_signInManager = signInManager;
	}

	public IActionResult OnGet()
	{
		return Login();
	}

	public async Task<IActionResult> OnPost()
	{
		await _signInManager.Logout(User);
		return Login();
	}
}
