using System;
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

			if (request.Headers != null)
			{
				return request.Headers[RequestedWithHeader] == XmlHttpRequest;
			}

			return false;
		}

		public static bool IsRobotsTxt(this HttpRequest request)
		{
			return request?.Path.Value?.EndsWith("robots.txt") ?? false;
		}
	}
}
