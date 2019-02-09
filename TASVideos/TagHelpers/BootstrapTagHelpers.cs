using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers
{
	public class Fullrow : TagHelper
	{
		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "div";
			output.AddCssClass("row");

			var content = (await output.GetChildContentAsync()).GetContent();
			output.Content.SetHtmlContent($@"<div class=""col-12"">{content}</div>");
		}
	}

	public class RowTagHelper : TagHelper
	{
		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "div";
			output.AddCssClass("row");
		}
	}

	public class ColumnTagHelper : TagHelper
	{
		public int? Xs { get; set; }
		public int? Sm { get; set; } 
		public int? Md { get; set; } 
		public int? Lg { get; set; } 
		
		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			Validate();
			output.TagName = "div";

			var classList = new List<string>();
			if (Xs.HasValue)
			{
				classList.Add("col-xs-" + Xs);
			}

			if (Sm.HasValue)
			{
				classList.Add("col-sm-" + Sm);
			}

			if (Md.HasValue)
			{
				classList.Add("col-md-" + Md);
			}

			if (Lg.HasValue)
			{
				classList.Add("col-lg-" + Lg);
			}

			if (!classList.Any())
			{
				classList.Add("col-12");
			}

			foreach (var c in classList)
			{
				output.AddCssClass(c);
			}
		}

		private void Validate()
		{
			if (Xs.HasValue && (Xs < 1 || Xs > 12))
			{
				throw new ArgumentException($"{nameof(Xs)} must be in the range of 1-12");
			}

			if (Sm.HasValue && (Sm < 1 || Sm > 12))
			{
				throw new ArgumentException($"{nameof(Sm)} must be in the range of 1-12");
			}

			if (Xs.HasValue && (Md < 1 || Md > 12))
			{
				throw new ArgumentException($"{nameof(Md)} must be in the range of 1-12");
			}

			if (Xs.HasValue && (Lg < 1 || Lg > 12))
			{
				throw new ArgumentException($"{nameof(Lg)} must be in the range of 1-12");
			}
		}
	}

	public class FormGroupTagHelper : TagHelper
	{
		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "fieldset";
			output.AddCssClass("form-group");
		}
	}

	public class CardTagHelper : TagHelper
	{
		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "div";
			output.AddCssClass("card");
		}
	}

	public class CardheaderTagHelper : TagHelper
	{
		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "div";
			output.AddCssClass("card-header");
		}
	}

	public class CardbodyTagHelper : TagHelper
	{
		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "div";
			output.AddCssClass("card-body");
		}
	}

	public class CardfooterTagHelper : TagHelper
	{
		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "div";
			output.AddCssClass("card-footer");
		}
	}

	public class AlertTagHelper : TagHelper
	{
		public bool Dismissible { get; set; }

		public virtual string Type { get; set; } = "info";

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			var content = (await output.GetChildContentAsync()).GetContent();
			output.TagName = "div";
			output.Attributes.Add("role", "alert");
			output.AddCssClass($"alert alert-{Type} text-center");
			if (Dismissible)
			{
				output.AddCssClass("alert-dismissible");
				output.Content.SetHtmlContent(
$@"<button type=""button"" class=""close"" data-dismiss=""alert"" aria-label=""close"">
	<span aria-hidden=""true"">&times;</span>
</button>
{content}");
			}
		}
	}

	public class InfoAlertTagHelper : AlertTagHelper
	{
		public override string Type { get; set; } = "info";
	}

	public class WarningAlertTagHelper : AlertTagHelper
	{
		public override string Type { get; set; } = "warning";
	}

	public class DangerAlertTagHelper : AlertTagHelper
	{
		public override string Type { get; set; } = "danger";
	}

	public class DeleteButtonTagHelper : TagHelper
	{
		private readonly IHtmlHelper _htmlHelper;

		public DeleteButtonTagHelper(IHtmlHelper helper)
		{
			_htmlHelper = helper;
		}

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		public string AspHref { get; set; }

		public string WarningMessage { get; set; } = "Are you sure you want to delete this record?";

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			var existingClassAttr = output.Attributes.FirstOrDefault(a => a.Name == "class");
			var existingCssClass = existingClassAttr?.Value.ToString() ?? "";
			if (existingClassAttr != null)
			{
				output.Attributes.Remove(existingClassAttr);
			}

			((IViewContextAware)_htmlHelper).Contextualize(ViewContext);
			var content = (await output.GetChildContentAsync()).GetContent();
			output.TagName = "span";
			var uniqueId = UniqueId();

			var antiForgeryToken = _htmlHelper.AntiForgeryToken().GetString();

			output.Content.SetHtmlContent($@"
<button type='button' class='btn btn-danger {existingCssClass}' data-toggle='modal' data-target='#areYouSureModal{uniqueId}'>{content}</button>
<div id='areYouSureModal{uniqueId}' class='modal fade' role='dialog'>
	<div class='modal-dialog'>
		<div class='modal-content'>
			<div class='modal-header'>
				<h5 class='modal-title text-danger'>Delete Warning!</h5>
				<button type='button' class='close' data-dismiss='modal'><span aria-hidden='true'>&times;</span></button>
			</div>
			<div class='modal-body'>
				<p>{WarningMessage}</p>
			</div>
			<div class='modal-footer'>
				<form action='{WebUtility.UrlDecode(AspHref)}' method='post'>
					{antiForgeryToken}
					<button type='submit' class='text-center btn btn-danger'>Yes</button>
				</form>
				<button type='button' class='btn btn-secondary' data-dismiss='modal'>No</button>
			</div>
		</div>
	</div>
</div>
");
		}

		private string UniqueId()
		{
			return Guid.NewGuid().ToString().Replace("-", "");
		}
	}
}
