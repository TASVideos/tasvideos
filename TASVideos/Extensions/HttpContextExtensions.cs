using TASVideos.Pages;

namespace TASVideos.Extensions;

public static class HttpContextExtensions
{
	extension(HttpContext? context)
	{
		public string CurrentPathToReturnUrl()
			=> context is null ? "" : $"{context.Request.Path}{context.Request.QueryString}";

		public void SetRequiredPermissionsView(RequirePermissionsView requiredPermissions)
			=> context?.Items["RequiredPermissions"] = requiredPermissions;

		public RequirePermissionsView? GetRequiredPermissionsView()
			=> context?.Items["RequiredPermissions"] as RequirePermissionsView;
	}
}
