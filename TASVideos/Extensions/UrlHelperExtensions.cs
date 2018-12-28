using TASVideos.Controllers;

namespace Microsoft.AspNetCore.Mvc
{
	public static class UrlHelperExtensions
	{
		public static string EmailConfirmationLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
		{
			return urlHelper.Page("/Account/ConfirmEmail", null, new { userId, code }, scheme);
		}

		public static string ResetPasswordCallbackLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
		{
			return urlHelper.Action(
				action: nameof(AccountController.ResetPassword),
				controller: "Account",
				values: new { userId, code },
				protocol: scheme);
		}
	}
}
