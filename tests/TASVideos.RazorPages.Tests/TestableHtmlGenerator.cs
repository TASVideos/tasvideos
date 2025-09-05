/*
 * taken from .NET 8 source, MIT-licensed
 * specifically https://github.com/dotnet/aspnetcore/blob/v8.0.0/src/Mvc/Mvc.TagHelpers/test/TestableHtmlGenerator.cs
 * but also I cut out most of it anyway
 */

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.WebEncoders.Testing;

namespace TASVideos.RazorPages.Tests;

// ReSharper disable EmptyConstructor
internal class TestableHtmlGenerator(
	IOptions<MvcViewOptions> options,
	IModelMetadataProvider metadataProvider) : DefaultHtmlGenerator(
		Substitute.For<IAntiforgery>(),
		options,
		metadataProvider,
		ConfigureSubstitute.For<IUrlHelperFactory>(
			mock => _ = mock.GetUrlHelper(Arg.Any<ActionContext>())
				.Returns(callInfo => new FakeUrlHelper(callInfo.ArgAt<ActionContext>(0)))),
		new HtmlTestEncoder(),
		new DefaultValidationHtmlAttributeProvider(options, metadataProvider, new()))
{
	private sealed class FakeUrlHelper(ActionContext context) : UrlHelperBase(context)
	{
		public override string Action(UrlActionContext actionContext)
			=> throw new NotSupportedException();

		public override string? RouteUrl(UrlRouteContext routeContext)
		{
			var virtualPath = ActionContext.RouteData.Routers[0].GetVirtualPath(new(
				ActionContext.HttpContext,
				AmbientValues,
				GetValuesDictionary(routeContext.Values),
				routeContext.RouteName))?.VirtualPath;
			if (virtualPath is not null)
			{
				// not sure why, but the inner Route adds the whole path again as `?path=`, so strip that
				// TODO hmm, the queryparam reflects AnchorTagHelper.Page, while the actual path reflects the route template, meaning this is discarding info that should be asserted against
				var iQuerystring = virtualPath.IndexOf('?');
				virtualPath = virtualPath[..iQuerystring];
			}

			return GenerateUrl(
				protocol: routeContext.Protocol,
				host: routeContext.Host,
				virtualPath: virtualPath,
				fragment: routeContext.Fragment);
		}
	}

	public static TestableHtmlGenerator Create(out ViewContext viewCtx, params KeyValuePair<string, string>[] routeStrs)
	{
		ServiceCollection services = [];
		services.AddSingleton(Substitute.For<IInlineConstraintResolver>());
		services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
		var routeOptionsWrapper = ConfigureSubstitute.For<IOptions<RouteOptions>>(
			mock => _ = mock.Value.Returns(new RouteOptions()));
		services.AddSingleton(routeOptionsWrapper);

		// equivalent to:
		// ObjectPool<UriBuildingContext> objPool = new DefaultObjectPoolProvider().Create(new UriBuilderContextPooledObjectPolicy());
		// DefaultTemplateBinderFactory defaultFactoryImpl = new(Substitute.For<ParameterPolicyFactory>(), objPool);
		var aspNetRoutingAsm = typeof(TemplateBinderFactory).Assembly;
		var objPool = typeof(ObjectPoolProvider).GetMethods()
			.First(mi => mi.Name is "Create" && mi.GetParameters().Length is 1)
			.MakeGenericMethod(aspNetRoutingAsm.GetType("Microsoft.AspNetCore.Routing.UriBuildingContext")!)
			.Invoke(new DefaultObjectPoolProvider(), parameters: [
				Activator.CreateInstance(aspNetRoutingAsm.GetType("Microsoft.AspNetCore.Routing.UriBuilderContextPooledObjectPolicy")!),
			]);
		var defaultFactoryImpl = (TemplateBinderFactory)Activator.CreateInstance(
			aspNetRoutingAsm.GetType("Microsoft.AspNetCore.Routing.Template.DefaultTemplateBinderFactory")!,
			Substitute.For<ParameterPolicyFactory>(),
			objPool)!;
		services.AddSingleton(defaultFactoryImpl);

		RouteHandler routeHandler = new(_ => Task.CompletedTask);
		DefaultInlineConstraintResolver constraintResolver = new(routeOptionsWrapper, Substitute.For<IServiceProvider>());
		RouteCollection router = new();
		foreach (var (name, template) in routeStrs)
		{
			router.Add(new Route(
				routeHandler,
				routeName: name,
				routeTemplate: template,
				defaults: null,
				constraints: null,
				dataTokens: null,
				constraintResolver));
		}

		ModelStateDictionary modelState = new();
		EmptyModelMetadataProvider metadataProvider = new();
		var serviceProvider = ConfigureSubstitute.For<IServiceProvider>(
			mock => _ = mock.GetService(Arg.Any<Type>()).Returns(callInfo =>
				{
					var serviceType = callInfo.ArgAt<Type>(0);
					return services.FirstOrDefault(o => o.ServiceType == serviceType) is { } found
						? found.IsKeyedService
							? found.KeyedImplementationInstance
								?? found.KeyedImplementationFactory?.Invoke(mock, found.ServiceKey)
								?? Activator.CreateInstance(found.KeyedImplementationType!, found.ServiceKey)
							: found.ImplementationInstance
								?? found.ImplementationFactory?.Invoke(mock)
								?? Activator.CreateInstance(found.ImplementationType!)
						: null;
				}));
		viewCtx = new(
			new(
				new DefaultHttpContext { RequestServices = serviceProvider },
				new() { Routers = { router } },
				new(),
				modelState),
			Substitute.For<IView>(),
			new(metadataProvider, modelState) { Model = null },
			Substitute.For<ITempDataDictionary>(),
			TextWriter.Null,
			new());
		return new(
			ConfigureSubstitute.For<IOptions<MvcViewOptions>>(
				mock => _ = mock.Value.Returns(new MvcViewOptions())),
			metadataProvider);
	}

	protected override void AddValidationAttributes(
		ViewContext viewContext,
		TagBuilder tagBuilder,
		ModelExplorer modelExplorer,
		string expression)
			=> throw new NotSupportedException();

	public override TagBuilder GenerateAntiforgery(ViewContext viewContext)
		=> new("input")
		{
			Attributes =
			{
				["name"] = "__RequestVerificationToken",
				["type"] = "hidden",
				["value"] = "olJlUDjrouRNWLen4tQJhauj1Z1rrvnb3QD65cmQU1Ykqi6S4", // 50 chars of a token.
			},
			TagRenderMode = TagRenderMode.SelfClosing,
		};
}
