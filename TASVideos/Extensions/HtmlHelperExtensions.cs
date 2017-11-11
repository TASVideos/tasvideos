using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;

namespace TASVideos.Extensions
{
	// TODO: convert to tag helpers?
	public static class HtmlHelperExtensions
	{
		/// <summary>
		/// Creates a two panel multi-select UI with add/remove buttons
		/// </summary>
		/// <param name="commaSeparatedIdListExpression">a string Model value that will contain a comma separated list of Ids</param>
		/// <param name="availableListExpression">a select list containing all available values will show in the list</param>
		/// <param name="rowHeight">if provided, the multi-select list heights will be forced to this value, otherwise this helper will make its own decisions about height</param>
		public static IHtmlContent TwoColumnPickerFor<TModel, TProperty1, TProperty2>(
			this IHtmlHelper<TModel> html,
			Expression<Func<TModel, TProperty1>> commaSeparatedIdListExpression,
			Expression<Func<TModel, TProperty2>> availableListExpression,
			int? rowHeight = null)
		{
			var idListMember = (MemberExpression)commaSeparatedIdListExpression.Body;

			var idListType = ((PropertyInfo)idListMember.Member).PropertyType;
			if (idListType != typeof(string))
			{
				throw new ArgumentException($"Invalid property type {idListType}, commaSeparatedIdListExpression must be a string");
			}

			var availableListMember = (MemberExpression)availableListExpression.Body;
			var availableListType = ((PropertyInfo)availableListMember.Member).PropertyType;

			if (!typeof(IEnumerable).IsAssignableFrom(availableListType) || !availableListType.IsGenericType)
			{
				throw new ArgumentException($"Invalid property type {availableListType}, availableListExpression must be a generic collection");
			}

			if (availableListType.GenericTypeArguments.First() != typeof(SelectListItem))
			{
				throw new ArgumentException($"Invalid property type {availableListType}, availableListExpression must be a collection of SelectListItem");
			}

			string selectedIds =
				(string)ExpressionMetadataProvider.FromLambdaExpression(commaSeparatedIdListExpression, html.ViewData, html.MetadataProvider).Model ?? "";

			List<SelectListItem> availableItems =
				((IEnumerable<SelectListItem>)ExpressionMetadataProvider.FromLambdaExpression(availableListExpression, html.ViewData, html.MetadataProvider).Model)
				.ToList();

			int rowSize = rowHeight ?? availableItems.Count.Clamp(8, 14); // Min and Max set by eyeballing it and deciding what looked decent

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

			var modelName = html.NameFor(commaSeparatedIdListExpression);
			var availableListName = html.NameFor(availableListExpression);
			var selectedListName = "Selected" + modelName;
			var addBtnName = modelName + "addBtn";
			var addAllBtnName = modelName + "addAllBtn";
			var removeBtnName = modelName + "removeBtn";
			var removeAllBtnName = modelName + "removeAllBtn";

			var mainDiv = new TagBuilder("div");
			mainDiv.AddCssClass("row");
			mainDiv.Attributes.Add("style", "display: flex; align-items: center;");

			mainDiv.InnerHtml.AppendHtml(
				html.TextBoxFor(commaSeparatedIdListExpression, new { style = "visibility: hidden; width: 0" }));

			var leftColumnDiv = new TagBuilder("div");
			leftColumnDiv.AddCssClass("col-xs-5");
			leftColumnDiv.InnerHtml.AppendHtml(html.LabelFor(availableListExpression, new { @class = "control-label" }));
			leftColumnDiv.InnerHtml.AppendHtml(html.ListBoxFor(availableListExpression, new MultiSelectList(remainingItems, "Value", "Text"), new
			{
				@class = "form-control",
				size = rowSize,
				Multiple = "multiple",
				style = "overflow-y: auto; padding-top: 7px;"
			}));

			mainDiv.InnerHtml.AppendHtml(leftColumnDiv);

			var middleColumnDiv = $@"
				<div class='col-xs-2'>
					<div class='col-sm-offset-3 col-sm-6'>
						<label class='control-label'> </label>
						<div class='row' style='margin-bottom: 3px'>
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
						<div class='row' style='margin-bottom: 3px'>
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
				</div>";

			mainDiv.InnerHtml.AppendHtml(middleColumnDiv);

			var rightColumnDiv = new TagBuilder("div");
			rightColumnDiv.AddCssClass("col-xs-5");
			rightColumnDiv.InnerHtml.AppendHtml(
				html.LabelFor(commaSeparatedIdListExpression, new { @class = "control-label", @for = selectedListName }));
			rightColumnDiv.InnerHtml.AppendHtml(
				html.ListBox(selectedListName, selectedItems,
					new
					{
						@class = "form-control",
						size = rowSize,
						Multiple = "multiple",
						style = "overflow-y: auto; padding-top: 7px;"
					}));
			mainDiv.InnerHtml.AppendHtml(rightColumnDiv);

			var uniqueFuncName = "twoColumnPicker" + Guid.NewGuid().ToString().Replace("-", "");
			string script = $@"<script>function {uniqueFuncName}() {{
				var selectedItemSelector = '{selectedListName}';
				var hiddenIdsSelector = '{modelName}';
				var availableNameSelector = '{availableListName}';
				var addBtnSelector = '{addBtnName}';
				var addAllBtnSelector = '{addAllBtnName}';
				var removeBtnSelector = '{removeBtnName}';
				var removeAllBtnSelector = '{removeAllBtnName}';

				// IE Hack
				(function () {{
					if (typeof NodeList.prototype.forEach === 'function') return false;
					NodeList.prototype.forEach = Array.prototype.forEach;
				}})();

				document.getElementById(availableNameSelector).addEventListener('dblclick', function() {{
					document.getElementById(addBtnSelector).click()
				}});

				document.getElementById(selectedItemSelector).addEventListener('dblclick', function() {{
					document.getElementById(removeBtnSelector).click()
				}});

				document.getElementById(addBtnSelector).addEventListener('click', function () {{
					var selectedIds = document.getElementById(hiddenIdsSelector).value
					var tempVals = selectedIds ? selectedIds.split(',') : new Array();

					var aopts = document.querySelectorAll('#' + availableNameSelector + ' option:checked');
					aopts.forEach(function (elem) {{
						tempVals.push(elem.value);
						document.getElementById(selectedItemSelector).appendChild(elem.cloneNode(true));
						document.getElementById(availableNameSelector).removeChild(elem);
					}});

					document.getElementById(hiddenIdsSelector).value = tempVals.join();
				}});

				document.getElementById(addAllBtnSelector).addEventListener('click', function () {{
					var aopts = document.querySelectorAll('#' + availableNameSelector + ' option');
					var tempVals = new Array();

					var existingIds = document.getElementById(hiddenIdsSelector).value;
					if (existingIds) {{
						tempVals = existingIds.split(',');
					}}

					aopts.forEach(function (elem) {{
						tempVals.push(elem.value);
						document.getElementById(selectedItemSelector).appendChild(elem.cloneNode(true));
						document.getElementById(availableNameSelector).removeChild(elem);
					}});

					document.getElementById(hiddenIdsSelector).value = tempVals.join();
				}});

				document.getElementById(removeBtnSelector).addEventListener('click', function () {{
					var selectedIds = document.getElementById(hiddenIdsSelector).value
					var tempVals = selectedIds ? selectedIds.split(',') : new Array();

					var sopts = document.querySelectorAll('#' + selectedItemSelector + ' option:checked');
					sopts.forEach(function (elem) {{
						document.getElementById(availableNameSelector).appendChild(elem.cloneNode(true));
						document.getElementById(selectedItemSelector).removeChild(elem);
						var index = tempVals.indexOf(elem.value);
						if (index >= 0) {{
							tempVals.splice(index, 1);
						}}
					}});

					document.getElementById(hiddenIdsSelector).value = tempVals.join();
				}});

				document.getElementById(removeAllBtnSelector).addEventListener('click', function () {{
					var sopts = document.querySelectorAll('#' + selectedItemSelector + ' option');
					sopts.forEach(function (elem) {{
						document.getElementById(availableNameSelector).appendChild(elem.cloneNode(true));
						document.getElementById(selectedItemSelector).removeChild(elem);
						document.getElementById(hiddenIdsSelector).value = '';
					}});
				}});
			}};
			{uniqueFuncName}();
			</script>";

			mainDiv.InnerHtml.AppendHtml(script);
			return mainDiv;
		}
	}
}
