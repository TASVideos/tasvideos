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
		new FakeAntiforgery(),
		options,
		metadataProvider,
		new FakeUrlHelperFactory(),
		new HtmlTestEncoder(),
		new DefaultValidationHtmlAttributeProvider(options, metadataProvider, new()))
{
	private readonly struct FakeAntiforgery() : IAntiforgery
	{
		public readonly AntiforgeryTokenSet GetAndStoreTokens(HttpContext httpContext)
			=> throw new NotSupportedException();

		public readonly AntiforgeryTokenSet GetTokens(HttpContext httpContext)
			=> throw new NotSupportedException();

		public readonly Task<bool> IsRequestValidAsync(HttpContext httpContext)
			=> throw new NotSupportedException();

		public readonly void SetCookieTokenAndHeader(HttpContext httpContext)
			=> throw new NotSupportedException();

		public readonly Task ValidateRequestAsync(HttpContext httpContext)
			=> throw new NotSupportedException();
	}

	private readonly struct FakeOptionsWrapper<T>(T value) : IOptions<T>
		where T : class
	{
		public readonly T Value
			=> value;
	}

	private readonly struct FakeServiceProvider : IServiceProvider
	{
		private readonly ServiceCollection _services = [];

		public FakeServiceProvider(Action<IServiceCollection, IServiceProvider> initServices)
			=> initServices(_services, this);

		public readonly object? GetService(Type serviceType)
			=> _services.FirstOrDefault(o => o.ServiceType == serviceType) is { } found
				? found.IsKeyedService
					? found.KeyedImplementationInstance
						?? found.KeyedImplementationFactory?.Invoke(this, found.ServiceKey)
						?? Activator.CreateInstance(found.KeyedImplementationType!, found.ServiceKey)
					: found.ImplementationInstance
						?? found.ImplementationFactory?.Invoke(this)
						?? Activator.CreateInstance(found.ImplementationType!)
				: null;
	}

	private sealed class FakeTempDataDictionary : Dictionary<string, object?>, ITempDataDictionary
	{
		public void Keep(string key)
			=> throw new NotSupportedException();

		public void Keep()
			=> throw new NotSupportedException();

		public void Load()
			=> throw new NotSupportedException();

		public object Peek(string key)
			=> throw new NotSupportedException();

		public void Save()
			=> throw new NotSupportedException();
	}

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

	private readonly struct FakeUrlHelperFactory() : IUrlHelperFactory
	{
		public readonly IUrlHelper GetUrlHelper(ActionContext context)
			=> new FakeUrlHelper(context);
	}

	private readonly struct FakeView() : IView
	{
		public string Path
			=> throw new NotSupportedException();

		public Task RenderAsync(ViewContext context)
			=> throw new NotSupportedException();
	}

	public static TestableHtmlGenerator Create(out ViewContext viewCtx, params string[] routeStrs)
	{
		FakeOptionsWrapper<RouteOptions> options = new(new());
		DefaultInlineConstraintResolver constraintResolver = new(options, new FakeServiceProvider((_, _) => { }));
		FakeServiceProvider serviceProvider = new((services, self) =>
		{
			services.AddSingleton<IInlineConstraintResolver>(constraintResolver);
			services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
			services.AddSingleton<IOptions<RouteOptions>>(options);

			var aspNetRoutingAsm = typeof(TemplateBinderFactory).Assembly;
			var policyFactory = (ParameterPolicyFactory)Activator.CreateInstance(
				aspNetRoutingAsm.GetType("Microsoft.AspNetCore.Routing.DefaultParameterPolicyFactory")!,
				options,
				self)!;
			var objPool = typeof(ObjectPoolProvider).GetMethods()
				.First(mi => mi.Name is "Create" && mi.GetParameters().Length is 1)
				.MakeGenericMethod(aspNetRoutingAsm.GetType("Microsoft.AspNetCore.Routing.UriBuildingContext")!)
				.Invoke(new DefaultObjectPoolProvider(), parameters: [
					Activator.CreateInstance(aspNetRoutingAsm.GetType("Microsoft.AspNetCore.Routing.UriBuilderContextPooledObjectPolicy")!),
				]);
			var defaultFactoryImpl = (TemplateBinderFactory)Activator.CreateInstance(
				aspNetRoutingAsm.GetType("Microsoft.AspNetCore.Routing.Template.DefaultTemplateBinderFactory")!,
				policyFactory,
				objPool)!;
			services.AddSingleton<TemplateBinderFactory>(_ => defaultFactoryImpl);
		});

		RouteHandler routeHandler = new(_ => Task.CompletedTask);
		RouteCollection router = new();
		foreach (var route in routeStrs)
		{
			router.Add(new Route(routeHandler, route, constraintResolver));
		}

		ModelStateDictionary modelState = new();
		EmptyModelMetadataProvider metadataProvider = new();
		viewCtx = new(
			new(
				new DefaultHttpContext { RequestServices = serviceProvider },
				new() { Routers = { router } },
				new(),
				modelState),
			new FakeView(),
			new(metadataProvider, modelState) { Model = null },
			new FakeTempDataDictionary(),
			TextWriter.Null,
			new());
		return new(new FakeOptionsWrapper<MvcViewOptions>(new()), metadataProvider);
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
