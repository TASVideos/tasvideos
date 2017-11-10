using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;

namespace TASVideos.Extensions
{
	// TODO: convert to tag helpers?
	public static class HtmlHelperExtensions
	{
		public static IHtmlContent Dummy<TModel>(this IHtmlHelper<TModel> html)
		{
			var sb = new StringBuilder();
			sb
				.Append("<div>")
				.Append("<p>This <b>is</b> some dummy content")
				.Append("</div>");

			var htmlString = new HtmlString(sb.ToString());
			return htmlString;
		}

		public static IHtmlContent Dummy2<TModel>(this IHtmlHelper<TModel> html)
		{
			var tb = new TagBuilder("div");
			tb.AddCssClass("row");
			tb.Attributes.Add("style", "margin-top: 30px");
			
			var innertb1 = new TagBuilder("p");
			innertb1.InnerHtml.Append("This <b>is</b> paragraph 1");
			var innertb2 = new TagBuilder("p");
			innertb2.InnerHtml.Append("This <b>is</b> paragraph 2");

			tb.InnerHtml.AppendHtml(innertb1);
			tb.InnerHtml.AppendHtml(innertb2);

			return tb;
			//var htmlString = new HtmlString(tb);
			//return htmlString;
		}

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

			var genericType = availableListType.GenericTypeArguments.First();
			if (genericType != typeof(SelectListItem))
			{
				throw new ArgumentException($"Invalid property type {availableListType}, availableListExpression must be a collection of SelectListItem");
			}

			string selectedIds =
				((string)ExpressionMetadataProvider.FromLambdaExpression(commaSeparatedIdListExpression, html.ViewData, html.MetadataProvider).Model) ?? string.Empty;

			List<SelectListItem> availableItems =
				((IEnumerable<SelectListItem>)ExpressionMetadataProvider.FromLambdaExpression(availableListExpression, html.ViewData, html.MetadataProvider).Model)
				.ToList();

			int rowSize = rowHeight ?? availableItems.Count.Clamp(7, 12); // Min and Max set by eyeballing it and deciding what looked decent

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

			var hiddenIdsName = html.NameFor(commaSeparatedIdListExpression);
			var availableListName = html.NameFor(availableListExpression);
			var selectedListName = "Selected" + hiddenIdsName;
			var addBtnName = hiddenIdsName + "addBtn";
			var addAllBtnName = hiddenIdsName + "addAllBtn";
			var removeBtnName = hiddenIdsName + "removeBtn";
			var removeAllBtnName = hiddenIdsName + "removeAllBtn";

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
                        <div class='row'>
                            <button type='button' id='{addBtnName}' class='btn btn-primary btn-xs col-sm-12 yardChange' aria-label='Add' title='Add'>
                                <span class='glyphicon glyphicon-chevron-right' aria-hidden='true'></span>
                            </button>
                        </div>
                        <div class='row'>
                            <button type='button' id='{addAllBtnName}' class='btn btn-primary btn-xs col-sm-12 yardChange' aria-label='Add All' title='Add All'>
                                <span class='glyphicon glyphicon-chevron-right' aria-hidden='true'></span>
                                <span class='glyphicon glyphicon-chevron-right' aria-hidden='true'></span>
                            </button>
                        </div><br />
                        <div class='row'>
                            <button type='button' id='{removeBtnName}' class='btn btn-primary btn-xs col-sm-12 yardChange' aria-label='Remove' title='Remove'>
                                <span class='glyphicon glyphicon-chevron-left' aria-hidden='true'></span>
                            </button>
                        </div>
                        <div class='row'>
                            <button type='button' id='{removeAllBtnName}' class='btn btn-primary btn-xs col-sm-12 yardChange' aria-label='Remove All' title='Remove All'>
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
			string script = @"<script>function " + uniqueFuncName + @"() {
                var selectedYardsSelector = '" + selectedListName + @"';
                var hiddenIdsSelector = '" + hiddenIdsName + @"';
                var availableNameSelector = '" + availableListName + @"';
                var addBtnSelector = '" + addBtnName + @"';
                var addAllBtnSelector = '" + addAllBtnName + @"';
                var removeBtnSelector = '" + removeBtnName + @"';
                var removeAllBtnSelector = '" + removeAllBtnName + @"';

                // IE Hack
                (function () {
                    if (typeof NodeList.prototype.forEach === 'function') return false;
                    NodeList.prototype.forEach = Array.prototype.forEach;
                })();

                document.getElementById(availableNameSelector).addEventListener('dblclick', function() {
                    document.getElementById(addBtnSelector).click()
                });

                document.getElementById(selectedYardsSelector).addEventListener('dblclick', function() {
                    document.getElementById(removeBtnSelector).click()
                });

                document.getElementById(addBtnSelector).addEventListener('click', function () {
                    var selectedIds = document.getElementById(hiddenIdsSelector).value
                    var tempVals = selectedIds ? selectedIds.split(',') : new Array();

                    var aopts = document.querySelectorAll('#' + availableNameSelector + ' option:checked');
                    aopts.forEach(function (elem) {
                        tempVals.push(elem.value);
                        document.getElementById(selectedYardsSelector).appendChild(elem.cloneNode(true));
                        document.getElementById(availableNameSelector).removeChild(elem);
                    });

                    document.getElementById(hiddenIdsSelector).value = tempVals.join();
                });

                document.getElementById(addAllBtnSelector).addEventListener('click', function () {
                    var aopts = document.querySelectorAll('#' + availableNameSelector + ' option');
                    var tempVals = new Array();

                    var existingIds = document.getElementById(hiddenIdsSelector).value;
                    if (existingIds) {
                        tempVals = existingIds.split(',');
                    }

                    aopts.forEach(function (elem) {
                        tempVals.push(elem.value);
                        document.getElementById(selectedYardsSelector).appendChild(elem.cloneNode(true));
                        document.getElementById(availableNameSelector).removeChild(elem);
                    });

                    document.getElementById(hiddenIdsSelector).value = tempVals.join();
                });

                document.getElementById(removeBtnSelector).addEventListener('click', function () {
                    var selectedIds = document.getElementById(hiddenIdsSelector).value
                    var tempVals = selectedIds ? selectedIds.split(',') : new Array();

                    var sopts = document.querySelectorAll('#' + selectedYardsSelector + ' option:checked');
                    sopts.forEach(function (elem) {
                        document.getElementById(availableNameSelector).appendChild(elem.cloneNode(true));
                        document.getElementById(selectedYardsSelector).removeChild(elem);
                        var index = tempVals.indexOf(elem.value);
                        if (index >= 0) {
                            tempVals.splice(index, 1);
                        }
                    });

                    document.getElementById(hiddenIdsSelector).value = tempVals.join();
                });

                document.getElementById(removeAllBtnSelector).addEventListener('click', function () {
                    var sopts = document.querySelectorAll('#' + selectedYardsSelector + ' option');
                    sopts.forEach(function (elem) {
                        document.getElementById(availableNameSelector).appendChild(elem.cloneNode(true));
                        document.getElementById(selectedYardsSelector).removeChild(elem);
                        document.getElementById(hiddenIdsSelector).value = '';
                    });
                });
            };
            " + uniqueFuncName + @"();
            </script>";

			mainDiv.InnerHtml.AppendHtml(script);

			//var test = GetString(mainDiv);
			//var htmlString = new HtmlString(test);
			//return htmlString;
			return mainDiv;
		}

		//public static IHtmlContent GetContent(this IHtmlHelper helper)
		//{
		//	var content = new HtmlContentBuilder()
		//		.AppendHtml("<ol class='content-body'><li>")
		//		.AppendHtml(helper.ActionLink("Home", "Index", "Home"))
		//		.AppendHtml("</li>");

		//	if (SomeCondition())
		//	{
		//		content.AppendHtml(@"<div>
  //          Note `HtmlContentBuilder.AppendHtml()` is Mutable
  //          as well as Fluent/Chainable.
  //      </div>");
		//	}

		//	return content;
		//}

		public static string GetString(IHtmlContent content)
		{
			var writer = new StringWriter();
			content.WriteTo(writer, HtmlEncoder.Default);
			return writer.ToString();
		}
	}
}
