using System.Collections;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using static TASVideos.TagHelpers.TagHelperExtensions;

namespace TASVideos.TagHelpers;

[HtmlTargetElement("string-list", TagStructure = TagStructure.WithoutEndTag, Attributes = "asp-for")]
public class StringListTagHelper : TagHelper
{
	public ModelExpression AspFor { get; set; } = null!;

	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; } = new();

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		ValidateExpression();
		ViewContext.ViewData.UseStringList();
		output.TagMode = TagMode.StartTagAndEndTag;
		output.TagName = "div";

		var modelName = AspFor.Name;
		var modelId = AspFor.Name.Replace(".", "_");
		var parentContainerName = $"{modelId}-string-list";
		output.Attributes.Add("id", parentContainerName);
		output.Attributes.Add("data-model-id", modelId);
		output.Content.AppendHtml("<div class='string-list-container'>");

		List<string> stringList = (AspFor.Model as IEnumerable<string>)?.ToList() ?? [];
		stringList = stringList.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

		// We need at least one line to clone, todo: refactor so this doesn't force the server side to strip out empty strings
		if (stringList.Count == 0)
		{
			stringList.Add("");
		}

		for (int i = 0; i < stringList.Count; i++)
		{
			output.Content.AppendHtml(
				$"""
				<div class='string-list-row row mb-1' data-index='{i}'>
					<div class='col'>
						<input type='text' spellcheck='false' class='form-control' {Attr("id", $"{modelId}_{i}_")} {Attr("name", modelName)} {Attr("value", stringList[i])} />
					</div>
					<div class='col-auto'>
						<button class='btn btn-secondary move-up-btn' type='button'><i class='fa fa-chevron-up'></i></button>
						<button class='btn btn-secondary move-down-btn' type='button'><i class='fa fa-chevron-down'></i></button>
						<button class='btn btn-danger delete-entry-btn' type='button'><i class='fa fa-remove'></i></button>
					</div>
				</div>
				""");
		}

		output.Content.AppendHtml(
$"<button {Attr("id", modelId + "-add-btn")} class='string-list-add-btn btn btn-secondary' type='button'><i class='fa fa-plus-square'></i></button>");

		output.Content.AppendHtml("</div>");
	}

	private void ValidateExpression()
	{
		var stringListType = AspFor.ModelExplorer.ModelType;
		if (!typeof(IEnumerable).IsAssignableFrom(stringListType)
			|| !stringListType.IsGenericType)
		{
			throw new ArgumentException($"Invalid property type {stringListType}, {nameof(AspFor)} must be a generic collection");
		}

		if (!stringListType.GenericTypeArguments.Contains(typeof(string)))
		{
			throw new ArgumentException($"Invalid property type {stringListType}, {nameof(AspFor)} must be an {nameof(IEnumerable)} of strings");
		}
	}
}
