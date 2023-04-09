namespace TASVideos.Extensions;

public static class HttpContextExtensions
{
	public static string CurrentPathToReturnUrl(this HttpContext context)
	{
		return $"{context.Request.Path}{context.Request.QueryString}";
	}
}
