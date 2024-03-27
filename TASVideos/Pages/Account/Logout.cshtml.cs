namespace TASVideos.Pages.Account;

[Authorize]
[IgnoreAntiforgeryToken]
public class LogoutModel(SignInManager signInManager) : BasePageModel
{
	public IActionResult OnGet()
	{
		return Login();
	}

	public async Task<IActionResult> OnPost()
	{
		await signInManager.Logout(User);
		return Login();
	}
}
