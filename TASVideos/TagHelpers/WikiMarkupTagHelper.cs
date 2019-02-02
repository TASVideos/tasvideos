using System.IO;
using System.Text.Encodings.Web;
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
		private readonly IHtmlHelper _htmlHelper;
		private readonly IViewComponentHelper _viewComponentHelper;

		public WikiMarkup(IHtmlHelper htmlHelper, IViewComponentHelper viewComponentHelper)
		{
			_htmlHelper = htmlHelper;
			_viewComponentHelper = viewComponentHelper;
		}

		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext ViewContext { get; set; }

		public string Markup { get; set; }
		public WikiPage PageData { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			(_htmlHelper as IViewContextAware)?.Contextualize(ViewContext);
			((IViewContextAware)_viewComponentHelper).Contextualize(ViewContext);
			output.TagName = "div";

			var sw = new StringWriter();
			Util.RenderHtmlDynamic(Markup, sw, this);
			output.Content.SetHtmlContent(sw.ToString());
		}

		bool IWriterHelper.CheckCondition(string condition)
		{
			return HtmlExtensions.WikiCondition(ViewContext, condition);
		}

		void IWriterHelper.RunViewComponent(TextWriter w, string name, string pp)
		{
			// TODO: Do we want to asyncify this entire thingy?
			var content = _viewComponentHelper.InvokeAsync(name, new { pageData = PageData, pp }).Result;
			content.WriteTo(w, HtmlEncoder.Default);
		}
	}
}
