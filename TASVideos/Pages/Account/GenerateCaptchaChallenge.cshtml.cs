namespace TASVideos.Pages.Account;

[AllowAnonymous]
[IpBanCheck]
public class GenerateCaptchaChallengeModel : BasePageModel
{
	public async Task<IActionResult> OnGet([FromServices] ICaptchaService captcha)
		=> new OkObjectResult(await captcha.GenerateChallengeAsync());
}
