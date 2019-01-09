using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

using TASVideos.Data;
using TASVideos.Extensions;

namespace TASVideos.TagHelpers
{
	[HtmlTargetElement("sortable-table-head", TagStructure = TagStructure.WithoutEndTag)]
	public class SortableTableHeadTagHelper : TagHelper
	{
		private static readonly List<string> PagedModelProperties = typeof(PagedModel)
			.GetProperties()
			.Select(p => p.Name)
			.ToList();

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		public PagedModel Paging { get; set; }
		public Type ModelType { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			if (Paging == null)
			{
				throw new ArgumentException($"{nameof(Paging)} can not be null");
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
				output.Content.AppendHtml("<th>");
				var isSortable = property.GetCustomAttribute<SortableAttribute>() != null;
				var displayName = property.DisplayName();
				var propertyName = property.Name;
				if (isSortable)
				{
					var sortDescending = Paging.SortBy == propertyName && !Paging.SortDescending;
					output.Content.AppendHtml(
						$"<a href='{page}?CurrentPage={Paging.CurrentPage}&PageSize={Paging.PageSize}&SortDescending={sortDescending}&SortBy={property.Name}{AdditionalParams()}'>");
					output.Content.AppendHtml(displayName);

					if (Paging.SortBy == propertyName)
					{
						var direction = Paging.SortDescending
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

		private string AdditionalParams()
		{
			var sb = new StringBuilder();

			var props = Paging.GetType().GetProperties().Where(p => !PagedModelProperties.Contains(p.Name));
			foreach (var prop in props)
			{
				sb.Append($"&{prop.Name}={prop.GetValue(Paging)}");
			}

			return sb.ToString();
		}
	}
}
