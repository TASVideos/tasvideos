using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TASVideos.Extensions;
using TASVideos.WikiEngine;
using TASVideos.WikiEngine.AST;

namespace TASVideos.TagHelpers
{
	[HtmlTargetElement("wiki-markup", TagStructure = TagStructure.WithoutEndTag)]
	public class WikiMarkup : TagHelper, IWriterHelper
	{
		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext ViewContext { get; set; }

		public string Markup { get; set; }

		private readonly IHtmlHelper _htmlHelper;

		public WikiMarkup(IHtmlHelper htmlHelper)
		{
			_htmlHelper = htmlHelper;
		}

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			(_htmlHelper as IViewContextAware).Contextualize(ViewContext);
			output.TagName = "div";
			// output.AddCssClass("what are we using here");
			var sw = new StringWriter();
			Util.RenderHtmlDynamic(Markup, sw, this);
			output.Content.SetHtmlContent(sw.ToString());


			//var content = (await output.GetChildContentAsync()).GetContent();
			//output.Content.SetHtmlContent($@"<div class=""col-12"">{content}</div>");
		}

        bool IWriterHelper.CheckCondition(string condition)
        {
            return HtmlExtensions.WikiCondition(ViewContext, condition);
        }
    }
}
