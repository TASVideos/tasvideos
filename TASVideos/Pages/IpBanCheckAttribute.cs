using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using TASVideos.Core.Services;
using TASVideos.Extensions;

namespace TASVideos.Pages
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
	public class IpBanCheckAttribute : Attribute, IAsyncPageFilter
	{
		public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
		{
			var banService = context.HttpContext.RequestServices.GetRequiredService<IIpBanService>();

			var banned = await banService.IsBanned(context.HttpContext.Connection.RemoteIpAddress);

			if (banned)
			{
				Denied(context);
			}
			else
			{
				await next.Invoke();
			}
		}

		public async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
		{
			await Task.CompletedTask;
		}

		protected void Denied(PageHandlerExecutingContext context)
		{
			if (context.HttpContext.Request.IsAjaxRequest())
			{
				context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
				context.Result = new EmptyResult();
			}
			else
			{
				context.Result = new RedirectToPageResult("/Account/AccessDenied");
			}
		}
	}
}
