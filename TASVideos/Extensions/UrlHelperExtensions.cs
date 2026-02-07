namespace Microsoft.AspNetCore.Mvc;

public static class UrlHelperExtensions
{
	extension(IUrlHelper urlHelper)
	{
		public string EmailConfirmationLink(string userId, string code)
			=> urlHelper.Page("/Account/ConfirmEmail", null, new { userId, code }, "https") ?? "";

		public string EmailChangeConfirmationLink(string code)
			=> urlHelper.Page("/Account/ConfirmEmailChange", null, new { code }, "https") ?? "";

		public string ResetPasswordCallbackLink(string userId, string code)
			=> urlHelper.Page("/Account/ResetPassword", null, new { userId, code }, "https") ?? "";
	}
}
