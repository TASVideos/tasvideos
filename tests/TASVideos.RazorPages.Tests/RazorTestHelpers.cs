using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using TASVideos.Pages;

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

	public static void SetBody(PageModel page, string text)
	{
		var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
		page.PageContext.HttpContext.Request.Body = bodyStream;
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

	public static void AssertBadRequest(IActionResult result)
	{
		Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
		var badRequestResult = (BadRequestObjectResult)result;
		Assert.IsFalse(string.IsNullOrWhiteSpace(badRequestResult.Value?.ToString()));
	}

	public static void AssertAccessDenied(IActionResult result)
		=> AssertRedirect(result, "/Account/AccessDenied");

	public static void AssertForumNotFound(IActionResult result)
		=> AssertRedirect(result, "/Forum/NotFound");

	public static void AssertRedirectHome(IActionResult result)
		=> AssertRedirect(result, "/Index");

	public static void AssertRedirectError(IActionResult result)
		=> AssertRedirect(result, "/Error");

	public static void AssertRedirect(IActionResult result, string path, int? routeId = null)
	{
		Assert.IsNotNull(result);
		Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
		var redirectResult = (RedirectToPageResult)result;
		Assert.AreEqual(path, redirectResult.PageName);

		if (routeId != null)
		{
			Assert.IsNotNull(redirectResult.RouteValues);
			Assert.AreEqual(routeId, redirectResult.RouteValues!["Id"]);
		}
	}

	public static void AssertHasPermission(Type pageModelType, PermissionTo permission)
	{
		var attribute = pageModelType
			.GetCustomAttributes(typeof(RequirePermissionAttribute), false)
			.FirstOrDefault() as RequirePermissionAttribute;

		Assert.IsNotNull(attribute);
		Assert.IsTrue(attribute.RequiredPermissions.Contains(permission));
	}

	public static void AssertAllowsAnonymousUsers(Type pageModelType)
		=> AssertHasAttribute(pageModelType, typeof(AllowAnonymousAttribute));

	public static void AssertHasIpBanCheck(Type pageModelType)
		=> AssertHasAttribute(pageModelType, typeof(IpBanCheckAttribute));

	public static void AssertRequiresAuthorization(Type pageModelType)
		=> AssertHasAttribute(pageModelType, typeof(AuthorizeAttribute));

	public static void AssertRequiresCurrentPermissions(Type pageModelType)
		=> AssertHasAttribute(pageModelType, typeof(RequireCurrentPermissions));

	private static void AssertHasAttribute(Type pageModelType, Type attributeType)
	{
		var attribute = pageModelType
			.GetCustomAttributes(attributeType, inherit: false);

		Assert.IsTrue(attribute.Length > 0);
	}

	public static IUrlHelper GeMockUrlHelper(string url)
	{
		var urlHelper = Substitute.For<IUrlHelper>();
		urlHelper
			.RouteUrl(Arg.Any<UrlRouteContext>())
			.Returns(url);
		urlHelper.ActionContext
			.Returns(new ActionContext { RouteData = new RouteData() });
		return urlHelper;
	}
}
