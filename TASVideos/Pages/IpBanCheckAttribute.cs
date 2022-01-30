using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TASVideos.Core.Services;

namespace TASVideos.Pages;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public class IpBanCheckAttribute : Attribute, IAsyncPageFilter
{
	public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
	{
		var banService = context.HttpContext.RequestServices.GetRequiredService<IIpBanService>();

		var ip = context.HttpContext.ActualIpAddress();
		var banned = await banService.IsBanned(ip);

		if (banned)
		{
			var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<IpBanCheckAttribute>>();
			logger.LogWarning($"An attempt to use banned ip {ip} was made");
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
