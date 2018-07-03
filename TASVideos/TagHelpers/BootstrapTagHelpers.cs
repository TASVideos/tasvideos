using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers
{
    public class RowTagHelper : TagHelper
    {
		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "div";
			output.AddCssClass("row");
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

	public abstract class AlertTagHelper : TagHelper
	{
		public bool Dismissible { get; set; }

		protected abstract string Type { get; }

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
		protected override string Type { get; } = "info";
	}

	public class WarningAlertTagHelper : AlertTagHelper
	{
		protected override string Type { get; } = "warning";
	}

	public class DangerAlertTagHelper : AlertTagHelper
	{
		protected override string Type { get; } = "danger";
	}

	public class DeleteButtonTagHelper : TagHelper
	{
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

			var content = (await output.GetChildContentAsync()).GetContent();
			output.TagName = "span";
			var uniqueId = UniqueId();
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
				<a href='{WebUtility.UrlDecode(AspHref)}' class='text-center btn btn-danger'>Yes</a>
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
