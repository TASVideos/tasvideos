using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Namotion.Reflection;
using TASVideos.Data.Entity;
using TASVideos.RazorPages.Extensions;
using TASVideos.RazorPages.ViewComponents;
using TASVideos.WikiEngine;
using TASVideos.WikiEngine.AST;

namespace TASVideos.RazorPages.TagHelpers
{
	public partial class WikiMarkup : TagHelper, IWriterHelper
	{
		private readonly IViewComponentHelper _viewComponentHelper;

		public WikiMarkup(IViewComponentHelper viewComponentHelper)
		{
			_viewComponentHelper = viewComponentHelper;
		}

		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext ViewContext { get; set; } = new ();

		public string Markup { get; set; } = "";
		public WikiPage PageData { get; set; } = new ();

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			((IViewContextAware)_viewComponentHelper).Contextualize(ViewContext);
			output.TagName = "article";
			output.AddCssClass("wiki");
			await Util.RenderHtmlAsync(Markup, new TagHelperTextWriter(output.Content), this);
		}

		bool IWriterHelper.CheckCondition(string condition)
		{
			return HtmlExtensions.WikiCondition(ViewContext, condition);
		}

		private static readonly IDictionary<string, Type> ViewComponents = Assembly
			.GetAssembly(typeof(WikiModuleAttribute))
			!.GetTypes()
			.Where(t => t.GetCustomAttribute(typeof(WikiModuleAttribute)) != null)
			.ToDictionary(tkey => ((WikiModuleAttribute)tkey.GetCustomAttribute(typeof(WikiModuleAttribute))!).Name, tvalue => tvalue, StringComparer.InvariantCultureIgnoreCase);

		async Task IWriterHelper.RunViewComponentAsync(TextWriter w, string name, IReadOnlyDictionary<string, string> pp)
		{
			var componentExists = ViewComponents.TryGetValue(name, out Type? viewComponent);
			if (!componentExists)
			{
				throw new InvalidOperationException($"Unknown ViewComponent: {name}");
			}

			var paramObject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
			{
				{ "pageData", PageData }
			};

			var invokeMethod = viewComponent!.GetMethod("InvokeAsync")
				?? viewComponent.GetMethod("Invoke");

			if (invokeMethod == null)
			{
				throw new InvalidOperationException($"Could not find an Invoke method on ViewComponent {viewComponent}");
			}

			var paramCandidates = invokeMethod
				.GetParameters()
				.Where(p => !paramObject.ContainsKey(p.Name!)); // filter out any already supplied parameters

			foreach (var paramCandidate in paramCandidates)
			{
				var paramType = paramCandidate.ParameterType;
				var adapterKeyType = paramType;
				var doNullableWrap = paramType.IsValueType
					&& (!paramType.IsGenericType || paramType.GetGenericTypeDefinition() != typeof(Nullable<>));

				if (doNullableWrap)
				{
					adapterKeyType = typeof(Nullable<>).MakeGenericType(adapterKeyType);
				}

				if (!ParamTypeAdapters.TryGetValue(adapterKeyType, out var adapter))
				{
					// These should all exist at compile time.
					throw new InvalidOperationException($"Unknown ViewComponent Argument Type: {adapterKeyType}");
				}

				pp.TryGetValue(paramCandidate.Name!, out var ppvalue);
				var result = adapter.Convert(ppvalue);

				if (result == null)
				{
					// Conversion failed.  See if the parameter type is a failable type.
					var needsNonNull = paramType.IsValueType && doNullableWrap
						|| !paramType.IsValueType && paramType.ToContextualType().Nullability == Nullability.NotNullable;
					if (needsNonNull)
					{
						// TODO: Better styling, or something
						w.Write($"MODULE ERROR for `{name}`: Missing parameter value for {paramCandidate.Name}");
						return;
					}
				}

				paramObject[paramCandidate.Name!] = result;
			}

			var content = await _viewComponentHelper.InvokeAsync(viewComponent, paramObject);

			content.WriteTo(w, HtmlEncoder.Default);
		}
	}
}
