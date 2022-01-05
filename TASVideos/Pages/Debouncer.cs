using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TASVideos.Core.Services;

namespace TASVideos.Pages
{
	/// <summary>
	/// Used to attempt to prevent double clicks on form submit buttons and other attempts to double post
	/// Takes advantage of the anti-forgery token to block identical requests for a short time
	/// </summary>
	public class Debouncer : IAsyncPageFilter
	{
		public async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
		{
			await Task.CompletedTask;
		}

		public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
		{
			if (context.HandlerMethod?.HttpMethod == "Post")
			{
				const string tokenName = "__RequestVerificationToken";
				var cache = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
				if (context.HttpContext.Request.HasFormContentType && context.HttpContext.Request.Form.ContainsKey(tokenName))
				{
					var token = context.HttpContext.Request.Form["__RequestVerificationToken"].ToString();
					if (cache.TryGetValue(token, out bool _))
					{
						// Block request as a duplicate
						var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Debouncer>>();
						logger.LogWarning($"Blocked duplicate POST request {context.HttpContext.Request.Path}");
						return;
					}

					cache.Set(token, true, Durations.OneMinuteInSeconds);
				}
			}

			await next.Invoke();
		}
	}
}
