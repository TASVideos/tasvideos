using System.Web;
using Microsoft.AspNetCore.Mvc.Razor;

namespace TASVideos.Services;

public sealed class SelfIdentifyRazorPageActivator(RazorPageActivator @base) : IRazorPageActivator
{
	public void Activate(IRazorPage page, ViewContext context)
	{
		context.Writer.Write($"<!--start {HttpUtility.HtmlEncode(context.ExecutingFilePath)}-->");
		@base.Activate(page, context);
	}
}
