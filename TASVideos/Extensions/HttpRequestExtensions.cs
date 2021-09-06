using System;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace TASVideos.Extensions
{
	public static class HttpRequestExtensions
	{
		private const string RequestedWithHeader = "X-Requested-With";
		private const string XmlHttpRequest = "XMLHttpRequest";

		public static bool IsAjaxRequest(this HttpRequest request)
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			return request.Headers[RequestedWithHeader] == XmlHttpRequest;
		}

		public static bool IsRobotsTxt(this HttpRequest? request)
		{
			return request?.Path.Value?.EndsWith("robots.txt") ?? false;
		}

		public static string ReturnUrl(this HttpRequest? request)
		{
			if (request == null)
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
			if (request == null)
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
	}
}
