using System.Net;
using System.Web;

namespace TASVideos.Extensions;

public static class HttpRequestExtensions
{
	private const string RequestedWithHeader = "X-Requested-With";
	private const string XmlHttpRequest = "XMLHttpRequest";

	public static bool IsAjaxRequest(this HttpRequest request)
		=> request.Headers[RequestedWithHeader] == XmlHttpRequest;

	public static bool IsRobotsTxt(this HttpRequest? request)
		=> request?.Path.Value?.EndsWith("robots.txt") ?? false;

	public static string ReturnUrl(this HttpRequest? request)
	{
		if (request is null)
		{
			return "";
		}

		if (!request.QueryString.HasValue)
		{
			return "";
		}

		var queryValues = HttpUtility.ParseQueryString(request.QueryString.Value ?? "");
		return queryValues["returnUrl"] ?? "";
	}

	public static string QueryStringValue(this HttpRequest? request, string key)
	{
		if (request is null)
		{
			return "";
		}

		if (string.IsNullOrWhiteSpace(key))
		{
			return "";
		}

		if (!request.QueryString.HasValue)
		{
			return "";
		}

		var queryValues = HttpUtility.ParseQueryString(request.QueryString.Value ?? "");
		return queryValues[key] ?? "";
	}

	public static int? QueryStringIntValue(this HttpRequest? request, string key)
	{
		var val = request.QueryStringValue(key);
		if (string.IsNullOrWhiteSpace(val))
		{
			return null;
		}

		if (int.TryParse(val, out int parsedInt))
		{
			return parsedInt;
		}

		return null;
	}

	public static bool? QueryStringBoolValue(this HttpRequest? request, string key)
	{
		var val = request.QueryStringValue(key);
		if (string.IsNullOrWhiteSpace(val))
		{
			return null;
		}

		if (bool.TryParse(val, out bool parsedBool))
		{
			return parsedBool;
		}

		return null;
	}

	private static bool FormBoolValue(this HttpRequest? request, string key)
	{
		if (request is null || !request.HasFormContentType)
		{
			return false;
		}

		if (request.Form.TryGetValue(key, out var val))
		{
			if (val == "on")
			{
				return true;
			}

			if (bool.TryParse(val, out var parsedBool))
			{
				return parsedBool;
			}
		}

		return false;
	}

	public static bool MinorEdit(this HttpRequest? request) => request.FormBoolValue("MinorEdit");

	public static IPAddress? ActualIpAddress(this HttpContext context)
	{
		var forwardedIp = context.Request.Headers["X-Forwarded-For"];
		if (!string.IsNullOrWhiteSpace(forwardedIp))
		{
			var result = IPAddress.TryParse(forwardedIp, out var address);
			if (result)
			{
				return address;
			}
		}

		return context.Connection.RemoteIpAddress;
	}

	public static string ToBaseUrl(this HttpRequest request)
		=> $"https://{request.Host}{request.PathBase}";

	public static string ToUrl(this HttpRequest request)
		=> $"https://{request.Host}{request.PathBase}{request.Path}";
}
