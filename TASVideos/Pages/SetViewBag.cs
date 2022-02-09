using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

using TASVideos.Data.Entity;

namespace TASVideos.Pages;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public class SetPageViewBagAttribute : ResultFilterAttribute
{
	private static readonly FileVersionInfo VersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
	private static string Version => $"{VersionInfo.FileMajorPart}.{VersionInfo.FileMinorPart}.{(VersionInfo.ProductVersion ?? "").Split('+').Skip(1).First().Split('.').First()}";
	private static string VersionSha => (VersionInfo.ProductVersion ?? "").Split('+').Skip(1).First().Split('.').Last();

	public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
	{
		if (context.Result is PageResult pageResult)
		{
			var viewData = pageResult.ViewData;
			viewData["Version"] = Version;
			viewData["VersionSha"] = VersionSha;

			var user = context.HttpContext.User;
			if (user.IsLoggedIn())
			{
				viewData["UserPermissions"] = user.Permissions();
			}
			else
			{
				viewData["UserPermissions"] = Enumerable.Empty<PermissionTo>();
			}
		}

		await next.Invoke();
	}
}
