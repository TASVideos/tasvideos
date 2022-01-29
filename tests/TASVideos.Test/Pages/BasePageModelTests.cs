using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace TASVideos.RazorPages.Tests.Pages;

public class BasePageModelTests
{
	public PageContext TestPageContext()
	{
		var httpContext = new DefaultHttpContext();
		var modelState = new ModelStateDictionary();
		var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
		var modelMetadataProvider = new EmptyModelMetadataProvider();
		var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
		return new PageContext(actionContext)
		{
			ViewData = viewData
		};
	}
}
