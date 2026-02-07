using System.Net;
using System.Web;

namespace TASVideos.Extensions;

public static class HttpRequestExtensions
{
	private const string RequestedWithHeader = "X-Requested-With";
	private const string XmlHttpRequest = "XMLHttpRequest";

	extension(HttpRequest? request)
	{
		public bool IsAjaxRequest() => request is not null && request.Headers[RequestedWithHeader] == XmlHttpRequest;
		public bool IsRobotsTxt() => request?.Path.Value?.EndsWith("robots.txt") ?? false;
		public bool MinorEdit() => request.FormBoolValue("MinorEdit");
		public string ToBaseUrl() => request is not null ? $"https://{request.Host}{request.PathBase}" : "";
		public string ToUrl() => request is not null ? $"https://{request.Host}{request.PathBase}{request.Path}" : "";
		public string ReturnUrl()
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

		public string QueryStringValue(string key)
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

		public int? QueryStringIntValue(string key)
		{
			var val = request.QueryStringValue(key);
			if (string.IsNullOrWhiteSpace(val))
			{
				return null;
			}

			if (int.TryParse(val, out var parsedInt))
			{
				return parsedInt;
			}

			return null;
		}

		public bool? QueryStringBoolValue(string key)
		{
			var val = request.QueryStringValue(key);
			if (string.IsNullOrWhiteSpace(val))
			{
				return null;
			}

			if (bool.TryParse(val, out var parsedBool))
			{
				return parsedBool;
			}

			return null;
		}

		private bool FormBoolValue(string key)
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
	}

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
}
