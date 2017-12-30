using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers
{
	[HtmlTargetElement("string-list", TagStructure = TagStructure.WithoutEndTag)]
	public class StringListTagHelper : TagHelper
	{
		public ModelExpression AspFor { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			ValidateExpression();
			output.TagMode = TagMode.StartTagAndEndTag;
			output.TagName = "div";

			var modelName = AspFor.ModelExplorer.Metadata.PropertyName;
			var parentContainerName = $"{modelName}-string-list";
			output.Attributes.Add("id", parentContainerName);
			output.Content.AppendHtml("<div class='row'>");

			List<string> stringList = ((IEnumerable<string>)AspFor.Model)?.ToList() ?? new List<string>();
			for (int i = 0; i < stringList.Count; i++)
			{
				output.Content.AppendHtml($@"
<div class='author-row' data-index='{i}'>
	<div class='col-xs-10'>
		<input type='text' spellcheck='false' class='form-control' id='{modelName}_{i}_' name='{modelName}' value='{stringList[i]}' />
	</div>
	<div class='col-xs-2'>
		<button {(i == 0 ? "id='" + modelName + "-add-btn'" : "")} class='string-list-add-btn btn btn-default {(i > 0 ? "hide": "")}' type='button'><span class='glyphicon glyphicon-plus'></span></button>
		<button onclick='this.parentElement.parentElement.remove()' class='string-list-remove-btn btn btn-danger {(i == 0 ? "hide" : "")}' type='button'><span class='glyphicon glyphicon-remove'></span></button>
	</div>
</div>");
			}

			var uniqueFuncName = "selectList" + context.UniqueId;
			output.Content.AppendHtml(
$@"<script>
	function {uniqueFuncName}() {{
		var addBtn = document.getElementById('{modelName}-add-btn');
		addBtn.onclick = function() {{
			var lastIndex = Math.max.apply(null, document.querySelectorAll('#{parentContainerName} .author-row')
				.toArray()
				.map(function(elem) {{
					return parseInt(elem.getAttribute('data-index'));
				}}));

			var lastElem = document.querySelector('#{parentContainerName} [data-index=""' + lastIndex + '""]');

			var newIndex = lastIndex + 1;
			var newElem = lastElem.cloneNode(true);
			newElem.setAttribute('data-index', newIndex);
			var input = newElem.querySelector('input');
			input.value = '';
			input.id = 'Authors_' + newIndex + '_';
			var addBtn = newElem.querySelector('.string-list-add-btn');
			addBtn.id = '';
			addBtn.classList.add('hide');

			var removeBtn = newElem.querySelector('.string-list-remove-btn');
			removeBtn.classList.remove('hide');

			document.querySelector('#{parentContainerName} div[class=""row""]').appendChild(newElem);
		}}
	}}
	{uniqueFuncName}();
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
