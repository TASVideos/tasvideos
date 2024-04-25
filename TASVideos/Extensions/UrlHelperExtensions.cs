namespace Microsoft.AspNetCore.Mvc;

public static class UrlHelperExtensions
{
	public static string EmailConfirmationLink(this IUrlHelper urlHelper, string userId, string code)
	{
		return urlHelper.Page("/Account/ConfirmEmail", null, new { userId, code }, "https") ?? "";
	}

	public static string EmailChangeConfirmationLink(this IUrlHelper urlHelper, string code)
	{
		return urlHelper.Page("/Account/ConfirmEmailChange", null, new { code }, "https") ?? "";
	}

	public static string ResetPasswordCallbackLink(this IUrlHelper urlHelper, string userId, string code)
	{
		return urlHelper.Page("/Account/ResetPassword", null, new { userId, code }, "https") ?? "";
	}
}
