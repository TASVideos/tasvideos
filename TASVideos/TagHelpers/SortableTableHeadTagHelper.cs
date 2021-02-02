﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

using TASVideos.Data;
using TASVideos.Extensions;
using static TASVideos.TagHelpers.TagHelperExtensions;

namespace TASVideos.TagHelpers
{
	[HtmlTargetElement("sortable-table-head", TagStructure = TagStructure.WithoutEndTag, Attributes = "sorting,model-type")]
	public class SortableTableHeadTagHelper : TagHelper
	{
		private static readonly List<string> SortingProperties = typeof(ISortable)
			.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Select(p => p.Name)
			.ToList();

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; } = new ();

		public ISortable Sorting { get; set; } = null!;
		public Type ModelType { get; set; } = null!;

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			if (Sorting == null)
			{
				throw new ArgumentException($"{nameof(Sorting)} can not be null");
			}

			if (ModelType == null)
			{
				throw new ArgumentException($"{nameof(ModelType)} can not be null");
			}

			var page = ViewContext.ActionDescriptor.DisplayName;

			output.TagName = "tr";
			output.TagMode = TagMode.StartTagAndEndTag;
			foreach (PropertyInfo property in ModelType.UnderlyingSystemType.GetProperties())
			{
				if (property.GetCustomAttribute<TableIgnoreAttribute>() is not null)
				{
					continue;
				}

				output.Content.AppendHtml("<th>");
				var isSortable = property.GetCustomAttribute<SortableAttribute>() is not null;
				var displayName = property.DisplayName();
				var propertyName = property.Name;

				if (isSortable)
				{
					var isSort = Sorting.IsSortingParam(propertyName);
					var isDescending = Sorting.IsDescending(propertyName);

					// TODO: support multiple sorts
					var sortStr = propertyName;
					if (isSort && !isDescending)
					{
						sortStr = "-" + propertyName;
					}

					output.Content.AppendHtml(
						$"<a {Attr("href", ComputeHref(sortStr))}>");
					output.Content.AppendHtml(displayName);

					if (isSort)
					{
						var direction = isDescending
							? "up"
							: "down";

						output.Content.AppendHtml($"<span class='fa fa-arrow-{direction}'></span>");
					}

					output.Content.AppendHtml("</a>");
				}
				else
				{
					output.Content.AppendHtml(displayName);
				}

				output.Content.AppendHtml("</th>");
			}
		}

		private string ComputeHref(string sortStr)
		{
			// TODO: Probably need some URL escaping in here
			var page = ViewContext.ActionDescriptor.DisplayName;
			return $"{page}?Sort={sortStr}{AdditionalParams()}";
		}

		private string AdditionalParams()
		{
			var sb = new StringBuilder();

			var props = Sorting.GetType().GetProperties().Where(p => !SortingProperties.Contains(p.Name));
			foreach (var prop in props)
			{
				sb.Append($"&{prop.Name}={prop.ToValue(Sorting)}");
			}

			return sb.ToString();
		}
	}
}
