using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.ViewComponents;
using TASVideos.WikiEngine;
using TASVideos.WikiEngine.AST;

namespace TASVideos.TagHelpers
{
	public class WikiMarkup : TagHelper, IWriterHelper
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

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			((IViewContextAware)_viewComponentHelper).Contextualize(ViewContext);
			output.TagName = "article";
			output.AddCssClass("wiki");

			var sw = new StringWriter();
			Util.RenderHtmlDynamic(Markup, sw, this);
			output.Content.SetHtmlContent(sw.ToString());
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

		void IWriterHelper.RunViewComponent(TextWriter w, string name, string pp)
		{
			// TODO: Do we want to asyncify this entire thingy?
			var result = ViewComponents.TryGetValue(name, out Type? viewComponent);
			if (!result)
			{
				throw new InvalidOperationException($"Unknown ViewComponent: {name}");
			}

			var content = _viewComponentHelper.InvokeAsync(viewComponent, new { pageData = PageData, pp }).Result;
			content.WriteTo(w, HtmlEncoder.Default);
		}
	}
}
