using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using static TASVideos.TagHelpers.TagHelperExtensions;

namespace TASVideos.TagHelpers;

[HtmlTargetElement("string-list", TagStructure = TagStructure.WithoutEndTag, Attributes = "asp-for")]
public class StringListTagHelper : TagHelper
{
	public ModelExpression AspFor { get; set; } = null!;

	public override void Process(TagHelperContext context, TagHelperOutput output)
	{
		ValidateExpression();
		output.TagMode = TagMode.StartTagAndEndTag;
		output.TagName = "div";

		var modelName = AspFor.Name;
		var modelId = AspFor.Name.Replace(".", "_");
		var parentContainerName = $"{modelId}-string-list";
		output.Attributes.Add("id", parentContainerName);
		output.Content.AppendHtml("<div class='string-list-container'>");

		List<string> stringList = (AspFor.Model as IEnumerable<string>)?.ToList() ?? new List<string>();
		stringList = stringList.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

		// We need at least one line to clone, todo: refactor so this doesn't force the server side to strip out empty strings
		if (stringList.Count == 0)
		{
			stringList.Add("");
		}

		for (int i = 0; i < stringList.Count; i++)
		{
			output.Content.AppendHtml(
$@"<div class='author-row row mb-1' data-index='{i}'>
	<div class='col'>
		<input type='text' spellcheck='false' class='form-control' {Attr("id", $"{modelId}_{i}_")} {Attr("name", modelName)} {Attr("value", stringList[i])} />
	</div>
	<div class='col-auto'>
		<button onclick='var fec=""firstElementChild"";var cur=this.parentElement.parentElement;var prv=cur.previousElementSibling;if(prv&&prv.classList.contains(""author-row"")){{var tmp=cur[fec][fec].value;cur[fec][fec].value=prv[fec][fec].value;prv[fec][fec].value=tmp;}}' class='btn btn-secondary' type='button'><i class='fa fa-chevron-up'></i></button>
		<button onclick='var fec=""firstElementChild"";var cur=this.parentElement.parentElement;var nxt=cur.nextElementSibling;if(nxt&&nxt.classList.contains(""author-row"")){{var tmp=cur[fec][fec].value;cur[fec][fec].value=nxt[fec][fec].value;nxt[fec][fec].value=tmp;}}' class='btn btn-secondary' type='button'><i class='fa fa-chevron-down'></i></button>
		<button onclick='if(document.querySelectorAll({JsValue($"#{parentContainerName} .author-row")}).length>1){{this.parentElement.parentElement.remove();}}' class='btn btn-danger' type='button'><i class='fa fa-remove'></i></button>
	</div>
</div>");
		}

		output.Content.AppendHtml(
$@"<button {Attr("id", modelId + "-add-btn")} class='string-list-add-btn btn btn-secondary' type='button'><i class='fa fa-plus-square'></i></button>");

		output.Content.AppendHtml(
$@"<script>
	{{
		let addBtn = document.getElementById({JsValue($"{modelId}-add-btn")});
		addBtn.onclick = function() {{
			let lastIndex = Math.max.apply(null, Array.from(document.querySelectorAll({JsValue($"#{parentContainerName} .author-row")}))
				.map(element => parseInt(element.getAttribute('data-index')))
			);

			let lastElem = document.querySelector({JsValue($"#{parentContainerName} ")} + '[data-index=""' + lastIndex + '""]');

			const newIndex = lastIndex + 1;
			let newElem = lastElem.cloneNode(true);
			newElem.setAttribute('data-index', newIndex);
			let input = newElem.querySelector('input');
			input.value = '';
			input.id = 'Authors_' + newIndex + '_';

			document.querySelector({JsValue($@"#{parentContainerName} div[class=""string-list-container""]")}).insertBefore(newElem,addBtn);
		}}
	}}
</script>");
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
