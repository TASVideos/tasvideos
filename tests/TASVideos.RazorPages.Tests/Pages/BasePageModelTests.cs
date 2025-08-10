using System.Collections.Specialized;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using TASVideos.Pages;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages;

[TestClass]
public class BasePageModelTests : TestDbBase
{
	protected static PageContext TestPageContext()
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

	[TestMethod]
	[DataRow("", "", new string[0])]
	[DataRow("/Index", "/Index", new string[0])]
	[DataRow("/Index", "/Index?GameId=12", new[] { "GameId", "12" })]
	[DataRow("/Index", "/Index?GameId=12&OtherId=345", new[] { "GameId", "12", "OtherId", "345" })]
	[DataRow("/Index?Change=67&ChangeToo=890&Unchanged=99", "/Index?Change=444&ChangeToo=555&Unchanged=99&New=11", new[] { "Change", "444", "ChangeToo", "555", "New", "11" })]
	[DataRow("/", "/?GameId=12", new[] { "GameId", "12" })]
	public void AddAdditionalParamsTests(string relativeUrl, string expected, string[] additionalParamsStrings)
	{
		Assert.AreEqual(0, additionalParamsStrings.Length % 2);

		NameValueCollection? additionalParams = null;
		if (additionalParamsStrings.Length > 0)
		{
			additionalParams = [];
			for (int i = 0; i < additionalParamsStrings.Length; i += 2)
			{
				additionalParams[additionalParamsStrings[i]] = additionalParamsStrings[i + 1];
			}
		}

		string actual = BasePageModel.AddAdditionalParams(relativeUrl, additionalParams);
		Assert.AreEqual(expected, actual);
	}
}
