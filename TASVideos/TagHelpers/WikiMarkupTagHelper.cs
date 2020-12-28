using System;
using System.Collections.Generic;
using System.IO;
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
		private readonly List<KeyValuePair<Regex, string>> _tableAttributeRunners = new ();

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

		bool IWriterHelper.AddTdStyleFilter(string pp)
		{
			var regex = ParamHelper.GetValueFor(pp, "pattern");
			var style = ParamHelper.GetValueFor(pp, "style");
			if (string.IsNullOrWhiteSpace(regex) || string.IsNullOrWhiteSpace(style))
			{
				return false;
			}

			try
			{
				// TODO: What's actually going on with these @s?
				if (regex[0] == '@')
				{
					regex = regex[1..];
				}

				if (regex[^1] == '@')
				{
					regex = regex[..^1];
				}

				var r = new Regex(regex, RegexOptions.None, TimeSpan.FromSeconds(1));
				_tableAttributeRunners.Add(new KeyValuePair<Regex, string>(r, style));
			}
			catch
			{
				return false;
			}

			return true;
		}

		bool IWriterHelper.CheckCondition(string condition)
		{
			return HtmlExtensions.WikiCondition(ViewContext, condition);
		}

		string? IWriterHelper.RunTdStyleFilters(string text)
		{
			foreach (var (key, value) in _tableAttributeRunners)
			{
				if (key.Match(text).Success)
				{
					return value;
				}
			}

			return null;
		}

		void IWriterHelper.RunViewComponent(TextWriter w, string name, string pp)
		{
			// TODO: Do we want to asyncify this entire thingy?
			var content = _viewComponentHelper.InvokeAsync(name, new { pageData = PageData, pp }).Result;
			content.WriteTo(w, HtmlEncoder.Default);
		}
	}
}
