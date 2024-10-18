using TASVideos.Pages;

namespace TASVideos.Extensions;

public static class HttpContextExtensions
{
	public static string CurrentPathToReturnUrl(this HttpContext? context)
	{
		return context is null ? "" : $"{context.Request.Path}{context.Request.QueryString}";
	}

	public static void SetRequiredPermissionsView(this HttpContext? context, RequirePermissionsView requiredPermissions)
	{
		if (context is not null)
		{
			context.Items["RequiredPermissions"] = requiredPermissions;
		}
	}

	public static RequirePermissionsView? GetRequiredPermissionsView(this HttpContext? context)
	{
		return context?.Items["RequiredPermissions"] as RequirePermissionsView;
	}
}
