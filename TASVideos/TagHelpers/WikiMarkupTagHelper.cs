using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.WikiEngine;
using TASVideos.WikiEngine.AST;

namespace TASVideos.TagHelpers
{
	public class WikiMarkup : TagHelper, IWriterHelper
	{
		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext ViewContext { get; set; }

		public string Markup { get; set; }

		public WikiPage PageData { get; set; }

		private readonly IHtmlHelper _htmlHelper;
		private readonly IViewComponentHelper _viewComponentHelper;

		public WikiMarkup(IHtmlHelper htmlHelper, IViewComponentHelper viewComponentHelper)
		{
			_htmlHelper = htmlHelper;
			_viewComponentHelper = viewComponentHelper;
		}

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			(_htmlHelper as IViewContextAware).Contextualize(ViewContext);
			((IViewContextAware)_viewComponentHelper).Contextualize(ViewContext);
			output.TagName = "div";
			// output.AddCssClass("what are we using here");
			var sw = new StringWriter();
			Util.RenderHtmlDynamic(Markup, sw, this);
			output.Content.SetHtmlContent(sw.ToString());
		}

        bool IWriterHelper.CheckCondition(string condition)
        {
            return HtmlExtensions.WikiCondition(ViewContext, condition);
        }

		string IWriterHelper.RunViewComponent(string name, string pp)
		{
			// TODO: Do we want to asyncify this entire thingy?
			var content = _viewComponentHelper.InvokeAsync(name, new { pageData = PageData, pp }).Result;
			return content.ToString();
		}
	}
}
