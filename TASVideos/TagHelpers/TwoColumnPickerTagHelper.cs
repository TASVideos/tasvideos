using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TASVideos.Extensions;

namespace TASVideos.TagHelpers
{
	[HtmlTargetElement("two-column-select", TagStructure = TagStructure.WithoutEndTag)]
	public class TwoColumnSelectTagHelper : TagHelper
	{
		private readonly IHtmlGenerator _htmlGenerator;
		public TwoColumnSelectTagHelper(IHtmlGenerator htmlGenerator)
		{
			_htmlGenerator = htmlGenerator;
		}

		public ModelExpression IdList { get; set; }

		public ModelExpression AvailableList { get; set; }

		/// <summary>
		/// An override for the number of rows
		/// </summary>
		public int? RowHeight { get; set; }

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagMode = TagMode.StartTagAndEndTag;

			string selectedIds = (string)IdList.Model;
			List<SelectListItem> availableItems = ((IEnumerable<SelectListItem>) AvailableList.Model).ToList();

			int rowSize = RowHeight ?? availableItems.Count.Clamp(8, 14); // Min and Max set by eyeballing it and deciding what looked decent

			var selectedIdList = !string.IsNullOrWhiteSpace(selectedIds)
				? selectedIds
					.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
					.Select(int.Parse)
					.ToList()
				: new List<int>();

			var selectedItems = availableItems
				.Where(y => selectedIdList.Contains(int.Parse(y.Value)))
				.ToList();
			var remainingItems = availableItems.Except(selectedItems);

			output.TagName = "div";
			output.Attributes.Add("style", "display: flex; align-tiems: center;");

			output.Content.AppendHtml("<input style='visibility: hidden; width: 0' />");

			// Generate hidden form element that will contain the selected ids
			output.Content.AppendHtml(_htmlGenerator.GenerateTextBox(
				ViewContext,
				IdList.ModelExplorer,
				IdList.Name, null, null, new
				{
					style = "visibility: hidden; width: 0"
				}));

			output.Content.AppendHtml("<div class='col-xs-5'>");
			output.Content.AppendHtml(_htmlGenerator.GenerateLabel(
				ViewContext,
				IdList.ModelExplorer,
				IdList.Name,
				IdList.Name,
				new { @class = "control-label" }));
			output.Content.AppendHtml("</div>");








		}

		private void ValidateExpressions()
		{
			var idListType = IdList.ModelExplorer.ModelType;
			if (idListType != typeof(string))
			{
				throw new ArgumentException($"Invalid property type {idListType}, {nameof(IdList)} must be a string");
			}

			var availableListType = AvailableList.ModelExplorer.ModelType;
			if (!typeof(IEnumerable).IsAssignableFrom(availableListType)
				|| !availableListType.IsGenericType)
			{
				throw new ArgumentException($"Invalid property type {availableListType}, {nameof(AvailableList)} must be a generic collection");
			}

			if (!availableListType.GenericTypeArguments.Contains(typeof(SelectListItem)))
			{
				throw new ArgumentException($"Invalid property type {availableListType}, {nameof(AvailableList)} must be an {nameof(IEnumerable)} of {nameof(SelectListItem)}");
			}
		}
	}
}
