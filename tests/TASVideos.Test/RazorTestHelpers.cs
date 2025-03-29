using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using TASVideos.Data.Entity;

namespace TASVideos.RazorPages.Tests;

public static class RazorTestHelpers
{
	public static void AddAuthenticatedUser(PageModel page, string userName, IEnumerable<PermissionTo> permissions)
	{
		var identity = new GenericIdentity(userName);
		foreach (var p in permissions)
		{
			identity.AddClaim(new Claim(CustomClaimTypes.Permission, ((int)p).ToString()));
		}

		var principle = new ClaimsPrincipal(identity);

		var httpContext = new DefaultHttpContext
		{
			User = principle
		};

		var modelState = new ModelStateDictionary();
		var modelMetadataProvider = new EmptyModelMetadataProvider();
		var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
		var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
		page.PageContext = new PageContext(actionContext)
		{
			ViewData = viewData
		};
	}
}
