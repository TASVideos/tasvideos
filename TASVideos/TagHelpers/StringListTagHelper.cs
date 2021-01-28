﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using static TASVideos.TagHelpers.TagHelperExtensions;

namespace TASVideos.TagHelpers
{
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

			// We need at least an add button, todo: refactor so this doesn't force the server side to strip out empty strings
			if (stringList.Count == 0)
			{
				stringList.Add("");
			}

			for (int i = 0; i < stringList.Count; i++)
			{
				output.Content.AppendHtml($@"
<div class='author-row row mb-1' data-index='{i}'>
	<div class='col-10'>
		<input type='text' spellcheck='false' class='form-control' {Attr("id", $"{modelId}_{i}_")} {Attr("name", modelName)} {Attr("value", stringList[i])} />
	</div>
	<div class='col-2'>
		<button {(i == 0 ? Attr("id", modelId + "-add-btn") : "")} class='string-list-add-btn btn btn-secondary {(i > 0 ? "d-none" : "")}' type='button'><span class='fa fa-plus-square'></span></button>
		<button onclick='this.parentElement.parentElement.remove()' class='string-list-remove-btn btn btn-danger {(i == 0 ? "d-none" : "")}' type='button'><span class='fa fa-remove'></span></button>
	</div>
</div>");
			}

			var uniqueFuncName = "selectList" + context.UniqueId;
			output.Content.AppendHtml(
$@"<script>
	function {JsValue(uniqueFuncName)}() {{
		var addBtn = document.getElementById({JsValue($"{modelId}-add-btn")});
		addBtn.onclick = function() {{
			var lastIndex = Math.max.apply(null, document.querySelectorAll({JsValue($"#{parentContainerName} .author-row")})
				.toArray()
				.map(function(elem) {{
					return parseInt(elem.getAttribute('data-index'));
				}}));

			var lastElem = document.querySelector({JsValue($@"#{parentContainerName} [data-index=""' + lastIndex + '""]")});

			var newIndex = lastIndex + 1;
			var newElem = lastElem.cloneNode(true);
			newElem.setAttribute('data-index', newIndex);
			var input = newElem.querySelector('input');
			input.value = '';
			input.id = 'Authors_' + newIndex + '_';
			var addBtn = newElem.querySelector('.string-list-add-btn');
			addBtn.id = '';
			addBtn.classList.add('d-none');

			var removeBtn = newElem.querySelector('.string-list-remove-btn');
			removeBtn.classList.remove('d-none');

			document.querySelector({JsValue($@"#{parentContainerName} div[class=""string-list-container""]")}).appendChild(newElem);
		}}
	}}
	{JsValue(uniqueFuncName)}();
</script>
");
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
}
