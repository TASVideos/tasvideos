using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using TASVideos.Data.Entity;

namespace TASVideos.RazorPages.Tests;

public static class RazorTestHelpers
{
	public static void AddAuthenticatedUser(PageModel page, User user, IEnumerable<PermissionTo> permissions)
	{
		var identity = new GenericIdentity(user.UserName);
		foreach (var p in permissions)
		{
			identity.AddClaim(new Claim(CustomClaimTypes.Permission, ((int)p).ToString()));
		}

		identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

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

	public static ClaimsPrincipal CreateClaimsPrincipalWithPermissions(IEnumerable<PermissionTo> permissions)
	{
		var identity = new GenericIdentity("Test User");
		foreach (var p in permissions)
		{
			identity.AddClaim(new Claim(CustomClaimTypes.Permission, ((int)p).ToString()));
		}

		return new ClaimsPrincipal(identity);
	}

	public static IFormFile CreateMockFormFile(string fileName, string contentType)
	{
		var formFile = Substitute.For<IFormFile>();
		formFile.FileName.Returns(fileName);
		formFile.ContentType.Returns(contentType);
		formFile.Length.Returns(100);

		var stream = new MemoryStream([1, 2, 3, 4]);
		formFile.OpenReadStream().Returns(stream);
		formFile.CopyToAsync(Arg.Any<Stream>()).Returns(Task.CompletedTask)
			.AndDoes(x => stream.CopyTo((Stream)x.Args()[0]));

		return formFile;
	}
}
