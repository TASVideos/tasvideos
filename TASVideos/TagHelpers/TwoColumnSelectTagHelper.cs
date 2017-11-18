using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
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
			ValidateExpressions();
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

			var modelName = IdList.ModelExplorer.Metadata.PropertyName;
			var availableListName = AvailableList.ModelExplorer.Metadata.PropertyName;
			var selectedListName = "Selected" + modelName;
			var addBtnName = modelName + "addBtn";
			var addAllBtnName = modelName + "addAllBtn";
			var removeBtnName = modelName + "removeBtn";
			var removeAllBtnName = modelName + "removeAllBtn";

			output.TagName = "div";
			output.Attributes.Add("style", "display: flex; align-items: center;");

			// Generate hidden form element that will contain the selected ids
			output.Content.AppendHtml(_htmlGenerator.GenerateTextBox(
				ViewContext,
				IdList.ModelExplorer,
				IdList.Name,
				IdList.Model,
				null,
				new
				{
					style = "visibility: hidden; width: 0"
				}));

			// Left Column Div
			output.Content.AppendHtml("<div class='col-xs-5'>");
			output.Content.AppendHtml(_htmlGenerator.GenerateLabel(
				ViewContext,
				AvailableList.ModelExplorer,
				availableListName,
				AvailableList.ModelExplorer.Metadata.DisplayName,
				new { @class = "control-label" }));

			output.Content.AppendHtml(
				$"<select class='form-control' id='{availableListName}' multiple='multiple' name='{availableListName}' size='{rowSize}' style='overflow-y: auto; padding-top: 7px;'>");
			output.Content.AppendHtml(
				_htmlGenerator.GenerateGroupsAndOptions(null, remainingItems));
			output.Content.AppendHtml("<select>");
			output.Content.AppendHtml("</div>");

			// Middle Column Div
			output.Content.AppendHtml($@"
<div class='col-xs-2'>
	<div class='col-sm-offset-3 col-sm-6'>
		<label class='control-label'> </label>
		<div class='row mb-s'>
			<button type='button' id='{addBtnName}' class='btn btn-primary btn-xs col-xs-12' aria-label='Add' title='Add'>
				<span class='glyphicon glyphicon-chevron-right' aria-hidden='true'></span>
			</button>
		</div>
		<div class='row'>
			<button type='button' id='{addAllBtnName}' class='btn btn-primary btn-xs col-xs-12' aria-label='Add All' title='Add All'>
				<span class='glyphicon glyphicon-chevron-right' aria-hidden='true'></span>
				<span class='glyphicon glyphicon-chevron-right' aria-hidden='true'></span>
			</button>
		</div><br />
		<div class='row mb-s'>
			<button type='button' id='{removeBtnName}' class='btn btn-primary btn-xs col-xs-12' aria-label='Remove' title='Remove'>
				<span class='glyphicon glyphicon-chevron-left' aria-hidden='true'></span>
			</button>
		</div>
		<div class='row'>
			<button type='button' id='{removeAllBtnName}' class='btn btn-primary btn-xs col-xs-12' aria-label='Remove All' title='Remove All'>
				<span class='glyphicon glyphicon-chevron-left' aria-hidden='true'></span>
				<span class='glyphicon glyphicon-chevron-left' aria-hidden='true'></span>
			</button>
		</div>
	</div>
</div>
");
			// Right Column Div
			output.Content.AppendHtml("<div class='col-xs-5'>");
			output.Content.AppendHtml($"<label class='control-label' for='{selectedListName}'>{IdList.ModelExplorer.Metadata.DisplayName}</label>");
			output.Content.AppendHtml(
				$"<select class='form-control' id='{selectedListName}' multiple='multiple' size='{rowSize}' style='overflow-y: auto; padding-top: 7px;'>");
			output.Content.AppendHtml(
				_htmlGenerator.GenerateGroupsAndOptions(null, selectedItems));
			output.Content.AppendHtml("<select>");

			output.Content.AppendHtml(
				_htmlGenerator.GenerateValidationMessage(ViewContext, IdList.ModelExplorer, IdList.Name, null, null, new { @class = "text-danger" }));

			output.Content.AppendHtml("</div>");

			// Script Tag
			var uniqueFuncName = "twoColumnPicker" + context.UniqueId;
			string script = $@"<script>function {uniqueFuncName}() {{
				document.getElementById('{availableListName}').addEventListener('dblclick', function() {{
					document.getElementById('{addBtnName}').click()
				}});

				document.getElementById('{selectedListName}').addEventListener('dblclick', function() {{
					document.getElementById('{removeBtnName}').click()
				}});

				document.getElementById('{addBtnName}').addEventListener('click', function () {{
					var selectedIds = document.getElementById('{modelName}').value
					var tempVals = selectedIds ? selectedIds.split(',') : new Array();

					var aopts = document.querySelectorAll('#{availableListName} option:checked');
					aopts.forEach(function (elem) {{
						tempVals.push(elem.value);
						document.getElementById('{selectedListName}').appendChild(elem.cloneNode(true));
						document.getElementById('{availableListName}').removeChild(elem);
					}});

					document.getElementById('{modelName}').value = tempVals.join();
				}});

				document.getElementById('{addAllBtnName}').addEventListener('click', function () {{
					var aopts = document.querySelectorAll('#{availableListName} option');
					var tempVals = new Array();

					var existingIds = document.getElementById('{modelName}').value;
					if (existingIds) {{
						tempVals = existingIds.split(',');
					}}

					aopts.forEach(function (elem) {{
						tempVals.push(elem.value);
						document.getElementById('{selectedListName}').appendChild(elem.cloneNode(true));
						document.getElementById('{availableListName}').removeChild(elem);
					}});

					document.getElementById('{modelName}').value = tempVals.join();
				}});

				document.getElementById('{removeBtnName}').addEventListener('click', function () {{
					var selectedIds = document.getElementById('{modelName}').value
					var tempVals = selectedIds ? selectedIds.split(',') : new Array();

					var sopts = document.querySelectorAll('#{selectedListName} option:checked');
					sopts.forEach(function (elem) {{
						document.getElementById('{availableListName}').appendChild(elem.cloneNode(true));
						document.getElementById('{selectedListName}').removeChild(elem);
						var index = tempVals.indexOf(elem.value);
						if (index >= 0) {{
							tempVals.splice(index, 1);
						}}
					}});

					document.getElementById('{modelName}').value = tempVals.join();
				}});

				document.getElementById('{removeAllBtnName}').addEventListener('click', function () {{
					var sopts = document.querySelectorAll('#{selectedListName} option');
					sopts.forEach(function (elem) {{
						document.getElementById('{availableListName}').appendChild(elem.cloneNode(true));
						document.getElementById('{selectedListName}').removeChild(elem);
						document.getElementById('{modelName}').value = '';
					}});
				}});
			}};
			{uniqueFuncName}();
			</script>";

			output.Content.AppendHtml(script);
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
