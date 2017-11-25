using System;
using System.Reflection;

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

			var controller = ViewContext.Controller();
			var action = ViewContext.Action();

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
						$"<a href='/{controller}/{action}?CurrentPage={Paging.CurrentPage}&PageSize={Paging.PageSize}&SortDescending={sortDescending}&SortBy={property.Name}'>");
					output.Content.AppendHtml(displayName);

					if (Paging.SortBy == propertyName)
					{
						var direction = Paging.SortDescending
							? "up"
							: "down";

						output.Content.AppendHtml($"<span class='glyphicon glyphicon-arrow-{direction}'></span>");
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
	}
}
