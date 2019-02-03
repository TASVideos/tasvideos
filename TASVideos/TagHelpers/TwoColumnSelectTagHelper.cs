using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

using TASVideos.Data.Entity;
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
		/// Gets or sets an override for the number of rows the select lists display
		/// </summary>
		public int? RowHeight { get; set; }

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			ValidateExpressions();
			output.TagMode = TagMode.StartTagAndEndTag;

			List<int> selectedIdList = ((IEnumerable)IdList.Model).Cast<int>().ToList();
			List<SelectListItem> availableItems = ((IEnumerable<SelectListItem>)AvailableList.Model).ToList();

			int rowSize = RowHeight ?? availableItems.Count.Clamp(8, 14); // Min and Max set by eyeballing it and deciding what looked decent

			var selectedItems = availableItems
				.Where(y => selectedIdList.Contains(int.Parse(y.Value)))
				.ToList();
			var remainingItems = availableItems.Except(selectedItems);

			var modelName = IdList.Name;
			var modelId = IdList.Name.Replace(".", "_");
			var modelContainer = modelName + "-id-container";
			var availableListName = AvailableList.Name;
			var selectedListName = "Selected" + modelId;
			var addBtnName = modelId + "addBtn";
			var addAllBtnName = modelId + "addAllBtn";
			var removeBtnName = modelId + "removeBtn";
			var removeAllBtnName = modelId + "removeAllBtn";

			var parentContainerName = $"{modelId}-two-column-select";

			output.TagName = "div";

			var idAttr = output.Attributes.FirstOrDefault(a => a.Name == "id");
			if (idAttr != null)
			{
				output.Attributes.Remove(idAttr);
			}

			output.Attributes.Add("id", parentContainerName);

			// Generate hidden form element that will contain the selected ids
			output.Content.AppendHtml($"<span id='{modelContainer}'>");
			foreach (var id in selectedIdList)
			{
				output.Content.AppendHtml($"<input type='hidden' v='{id}' name='{IdList.Name}' value='{id}' />");
			}

			output.Content.AppendHtml("</span>");
			output.Content.AppendHtml("<div class='row' style='display: flex; align-items: center;'>");

			// Left Column Div
			output.Content.AppendHtml("<div class='col-5'>");
			output.Content.AppendHtml(_htmlGenerator.GenerateLabel(
				ViewContext,
				AvailableList.ModelExplorer,
				availableListName,
				AvailableList.ModelExplorer.Metadata.DisplayName,
				new { @class = "form-control-label" }));

			output.Content.AppendHtml(
				$"<select class='form-control' id='{availableListName}' multiple='multiple' name='{availableListName}' size='{rowSize}' style='overflow-y: auto; padding-top: 7px;'>");
			output.Content.AppendHtml(
				_htmlGenerator.GenerateGroupsAndOptions(null, remainingItems));
			output.Content.AppendHtml("</select>");
			output.Content.AppendHtml("</div>");

			// Middle Column Div
			output.Content.AppendHtml($@"
<div class='col-2'>
	<div class='row'>
		<div class='offset-md-3 col-md-6'>
			<label class='form-control-label'> </label>
			<div class='row mb-1'>
				<button type='button' id='{addBtnName}' class='btn btn-primary btn-sm col-12' aria-label='Add' title='Add'>
					<i class='fa fa-chevron-right' aria-hidden='true'></i>
				</button>
			</div>
			<div class='row mb-4'>
				<button type='button' id='{addAllBtnName}' class='btn btn-primary btn-sm col-12' aria-label='Add All' title='Add All'>
					<i class='fa fa-chevron-right' aria-hidden='true'></i>
					<i class='fa fa-chevron-right' aria-hidden='true'></i>
				</button>
			</div>
			<div class='row mb-1'>
				<button type='button' id='{removeBtnName}' class='btn btn-primary btn-sm col-12' aria-label='Remove' title='Remove'>
					<i class='fa fa-chevron-left' aria-hidden='true'></i>
				</button>
			</div>
			<div class='row'>
				<button type='button' id='{removeAllBtnName}' class='btn btn-primary btn-sm col-12' aria-label='Remove All' title='Remove All'>
					<i class='fa fa-chevron-left' aria-hidden='true'></i>
					<i class='fa fa-chevron-left' aria-hidden='true'></i>
				</button>
			</div>
		</div>
	</div>
</div>
");

			// Right Column Div
			output.Content.AppendHtml("<div class='col-5'>");

			output.Content.AppendHtml(_htmlGenerator.GenerateLabel(
				ViewContext,
				IdList.ModelExplorer,
				modelName,
				IdList.ModelExplorer.Metadata.DisplayName,
				new { @class = "form-control-label", @for = selectedListName }));

			output.Content.AppendHtml(
				$"<select class='form-control' id='{selectedListName}' multiple='multiple' size='{rowSize}' style='overflow-y: auto; padding-top: 7px;'>");
			output.Content.AppendHtml(
				_htmlGenerator.GenerateGroupsAndOptions(null, selectedItems));
			output.Content.AppendHtml("</select>");

			output.Content.AppendHtml(
				_htmlGenerator.GenerateValidationMessage(ViewContext, IdList.ModelExplorer, IdList.Name, null, null, new { @class = "text-danger" }));

			output.Content.AppendHtml("</div>");

			// Script Tag
			var uniqueFuncName = "twoColumnPicker" + context.UniqueId;
			string script = $@"<script>function {uniqueFuncName}() {{
				var twoColumnChangeEvent = new CustomEvent('{modelName}Changed', {{ bubbles: true }});

				document.getElementById('{parentContainerName}').listChangedCallback = null;

				document.getElementById('{availableListName}').addEventListener('dblclick', function() {{
					document.getElementById('{addBtnName}').click()
				}});

				document.getElementById('{selectedListName}').addEventListener('dblclick', function() {{
					document.getElementById('{removeBtnName}').click()
				}});

				document.getElementById('{addBtnName}').addEventListener('click', function () {{
					var aopts = document.querySelectorAll('#{availableListName} option:checked');
					aopts.forEach(function (elem) {{
						var newInp = document.createElement('input')
						newInp.name = '{modelName}';
						newInp.type = 'hidden';
						newInp.value = elem.value;
						newInp.setAttribute('v', elem.value);
						document.getElementById('{modelContainer}').appendChild(newInp);
						document.getElementById('{selectedListName}').appendChild(elem.cloneNode(true));
						document.getElementById('{availableListName}').removeChild(elem);
					}});

					sortLists();

					if (aopts.length) {{
						document.getElementById('{parentContainerName}').dispatchEvent(twoColumnChangeEvent);
					}}
				}});

				document.getElementById('{addAllBtnName}').addEventListener('click', function () {{
					var aopts = document.querySelectorAll('#{availableListName} option:not(:disabled)');
					aopts.forEach(function (elem) {{
						var newInp = document.createElement('input')
						newInp.name = '{modelName}';
						newInp.type = 'hidden';
						newInp.value = elem.value;
						newInp.setAttribute('v', elem.value);
						document.getElementById('{modelContainer}').appendChild(newInp);
						document.getElementById('{selectedListName}').appendChild(elem.cloneNode(true));
						document.getElementById('{availableListName}').removeChild(elem);
					}});

					sortLists();

					if (aopts.length) {{
						document.getElementById('{parentContainerName}').dispatchEvent(twoColumnChangeEvent);
					}}
				}});

				document.getElementById('{removeBtnName}').addEventListener('click', function () {{
					var sopts = document.querySelectorAll('#{selectedListName} option:checked');
					sopts.forEach(function (elem) {{
						document.getElementById('{availableListName}').appendChild(elem.cloneNode(true));
						document.getElementById('{selectedListName}').removeChild(elem);

						document.querySelector('[name=""{modelName}""][v=""' + elem.value + '""]').remove();
					}});

					sortLists();

					if (sopts.length) {{
						document.getElementById('{parentContainerName}').dispatchEvent(twoColumnChangeEvent);
					}}
				}});

				document.getElementById('{removeAllBtnName}').addEventListener('click', function () {{
					var sopts = document.querySelectorAll('#{selectedListName} option:not(:disabled)');
					sopts.forEach(function (elem) {{
						document.getElementById('{availableListName}').appendChild(elem.cloneNode(true));
						document.getElementById('{selectedListName}').removeChild(elem);
						
					}});

					var container = document.getElementById('{modelContainer}');
					while (container.lastChild) {{
						container.removeChild(container.lastChild);
					}}

					sortLists();

					if (sopts.length) {{
						document.getElementById('{parentContainerName}').dispatchEvent(twoColumnChangeEvent);
					}}
				}});

				function sortLists() {{
					sortSelect(document.getElementById('{availableListName}'));
					sortSelect(document.getElementById('{selectedListName}'));
				}}

				function sortSelect(elem) {{
					var tmpAry = [];
					var selectedValue = elem[elem.selectedIndex] != undefined ? elem[elem.selectedIndex] : null
					for (var i = 0; i < elem.options.length;i++) tmpAry.push(elem.options[i]);
					tmpAry.sort(function(a, b){{ return (parseInt(a.value) < parseInt(b.value)) ? -1 : 1; }});
					while (elem.options.length > 0) elem.options[0] = null;
					for (var i = 0; i < tmpAry.length; i++) {{
						elem.options[i] = tmpAry[i];
					}}

					return;
				}}
			}};
			{uniqueFuncName}();
			</script>";

			output.Content.AppendHtml(script);
			output.Content.AppendHtml("</div>");
		}

		private void ValidateExpressions()
		{
			var idListType = IdList.ModelExplorer.ModelType;
			if (!typeof(IEnumerable).IsAssignableFrom(idListType)
				|| !idListType.IsGenericType)
			{
				throw new ArgumentException($"Invalid property type {idListType}, {nameof(IdList)} must be a generic collection");
			}

			if (!idListType.GenericTypeArguments.Contains(typeof(int))
			&& !idListType.GenericTypeArguments.Contains(typeof(SubmissionStatus))) // TODO: Hack, instead find a way that enums of type int can be supported
			{
				throw new ArgumentException($"Invalid property type {idListType}, {nameof(IdList)} must be an {nameof(IEnumerable)} of int");
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
