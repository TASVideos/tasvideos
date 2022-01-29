using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

using TASVideos.Data.Entity;
using TASVideos.Extensions;
using static TASVideos.TagHelpers.TagHelperExtensions;

namespace TASVideos.TagHelpers;

[HtmlTargetElement("two-column-select", TagStructure = TagStructure.WithoutEndTag, Attributes = "id-list,available-list")]
public class TwoColumnSelectTagHelper : TagHelper
{
	private readonly IHtmlGenerator _htmlGenerator;
	public TwoColumnSelectTagHelper(IHtmlGenerator htmlGenerator)
	{
		_htmlGenerator = htmlGenerator;
	}

	public ModelExpression IdList { get; set; } = null!;

	public ModelExpression AvailableList { get; set; } = null!;

	/// <summary>
	/// Gets or sets an override for the number of rows the select lists display
	/// </summary>
	public int? RowHeight { get; set; }

	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; } = new();

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
		if (idAttr is not null)
		{
			output.Attributes.Remove(idAttr);
		}

		output.Attributes.Add("id", parentContainerName);

		// Generate hidden form element that will contain the selected ids
		output.Content.AppendHtml($"<span {Attr("id", modelContainer)}>");
		foreach (var id in selectedIdList)
		{
			output.Content.AppendHtml($"<input type='hidden' v='{id}' {Attr("name", IdList.Name)} value='{id}' />");
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

		output.Content.AppendHtml($@"
				<select
					class='form-control'
					{Attr("id", availableListName)}
					multiple='multiple'
					{Attr("name", availableListName)}
					size='{rowSize}'
					style='overflow-y: auto; padding-top: 7px;'
				>
			");
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
								<button type='button' {Attr("id", addBtnName)} class='btn btn-primary btn-sm col-12' aria-label='Add' title='Add'>
									<i class='fa fa-chevron-right' aria-hidden='true'></i>
								</button>
							</div>
							<div class='row mb-4'>
								<button type='button' {Attr("id", addAllBtnName)} class='btn btn-primary btn-sm col-12' aria-label='Add All' title='Add All'>
									<i class='fa fa-chevron-right' aria-hidden='true'></i>
									<i class='fa fa-chevron-right' aria-hidden='true'></i>
								</button>
							</div>
							<div class='row mb-1'>
								<button type='button' {Attr("id", removeBtnName)} class='btn btn-primary btn-sm col-12' aria-label='Remove' title='Remove'>
									<i class='fa fa-chevron-left' aria-hidden='true'></i>
								</button>
							</div>
							<div class='row'>
								<button type='button' {Attr("id", removeAllBtnName)} class='btn btn-primary btn-sm col-12' aria-label='Remove All' title='Remove All'>
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
			$"<select class='form-control' {Attr("id", selectedListName)} multiple='multiple' size='{rowSize}' style='overflow-y: auto; padding-top: 7px;'>");
		output.Content.AppendHtml(
			_htmlGenerator.GenerateGroupsAndOptions(null, selectedItems));
		output.Content.AppendHtml("</select>");

		output.Content.AppendHtml(
			_htmlGenerator.GenerateValidationMessage(ViewContext, IdList.ModelExplorer, IdList.Name, null, null, new { @class = "text-danger" }));

		output.Content.AppendHtml("</div>");

		// Script Tag
		string script = $@"<script>{{
				const twoColumnChangeEvent = new CustomEvent({JsValue(modelName + "Changed")}, {{ bubbles: true }});

				document.getElementById({JsValue(parentContainerName)}).listChangedCallback = null;

				document.getElementById({JsValue(availableListName)}).addEventListener('dblclick', function() {{
					document.getElementById({JsValue(addBtnName)}).click()
				}});

				document.getElementById({JsValue(selectedListName)}).addEventListener('dblclick', function() {{
					document.getElementById({JsValue(removeBtnName)}).click()
				}});

				document.getElementById({JsValue(addBtnName)}).addEventListener('click', function () {{
					const aopts = document.querySelectorAll({JsValue($"#{availableListName} option:checked")});
					aopts.forEach(function (elem) {{
						let newInp = document.createElement('input')
						newInp.name = {JsValue(modelName)};
						newInp.type = 'hidden';
						newInp.value = elem.value;
						newInp.setAttribute('v', elem.value);
						document.getElementById({JsValue(modelContainer)}).appendChild(newInp);
						document.getElementById({JsValue(selectedListName)}).appendChild(elem.cloneNode(true));
						document.getElementById({JsValue(availableListName)}).removeChild(elem);
					}});

					sortLists();

					if (aopts.length) {{
						document.getElementById({JsValue(parentContainerName)}).dispatchEvent(twoColumnChangeEvent);
					}}
				}});

				document.getElementById({JsValue(addAllBtnName)}).addEventListener('click', function () {{
					const aopts = document.querySelectorAll({JsValue($"#{availableListName} option:not(:disabled)")});
					aopts.forEach(function (elem) {{
						let newInp = document.createElement('input')
						newInp.name = {JsValue(modelName)};
						newInp.type = 'hidden';
						newInp.value = elem.value;
						newInp.setAttribute('v', elem.value);
						document.getElementById({JsValue(modelContainer)}).appendChild(newInp);
						document.getElementById({JsValue(selectedListName)}).appendChild(elem.cloneNode(true));
						document.getElementById({JsValue(availableListName)}).removeChild(elem);
					}});

					sortLists();

					if (aopts.length) {{
						document.getElementById({JsValue(parentContainerName)}).dispatchEvent(twoColumnChangeEvent);
					}}
				}});

				document.getElementById({JsValue(removeBtnName)}).addEventListener('click', function () {{
					const sopts = document.querySelectorAll({JsValue($"#{selectedListName} option:checked")});
					sopts.forEach(function (elem) {{
						document.getElementById({JsValue(availableListName)}).appendChild(elem.cloneNode(true));
						document.getElementById({JsValue(selectedListName)}).removeChild(elem);

						document.querySelector({JsValue($@"[name=""{modelName}""][v=""")} + elem.value + '""]').remove();
					}});

					sortLists();

					if (sopts.length) {{
						document.getElementById({JsValue(parentContainerName)}).dispatchEvent(twoColumnChangeEvent);
					}}
				}});

				document.getElementById({JsValue(removeAllBtnName)}).addEventListener('click', function () {{
					const sopts = document.querySelectorAll({JsValue($"#{selectedListName} option:not(:disabled)")});
					sopts.forEach(function (elem) {{
						document.getElementById({JsValue(availableListName)}).appendChild(elem.cloneNode(true));
						document.getElementById({JsValue(selectedListName)}).removeChild(elem);
						
					}});

					let container = document.getElementById({JsValue(modelContainer)});
					while (container.lastChild) {{
						container.removeChild(container.lastChild);
					}}

					sortLists();

					if (sopts.length) {{
						document.getElementById({JsValue(parentContainerName)}).dispatchEvent(twoColumnChangeEvent);
					}}
				}});

				function sortLists() {{
					sortSelect(document.getElementById({JsValue(availableListName)}));
					sortSelect(document.getElementById({JsValue(selectedListName)}));
				}}

				function sortSelect(elem) {{
					let tmpAry = [];
					let selectedValue = elem[elem.selectedIndex] != undefined ? elem[elem.selectedIndex] : null
					for (let i = 0; i < elem.options.length;i++) tmpAry.push(elem.options[i]);
					tmpAry.sort(function(a, b){{ return (parseInt(a.value) < parseInt(b.value)) ? -1 : 1; }});
					while (elem.options.length > 0) elem.options[0] = null;
					for (let i = 0; i < tmpAry.length; i++) {{
						elem.options[i] = tmpAry[i];
					}}

					return;
				}}
			}}
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
