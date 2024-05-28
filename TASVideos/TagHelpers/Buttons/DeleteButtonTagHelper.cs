using System.Net;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TASVideos.TagHelpers;

public class DeleteButtonTagHelper(IHtmlHelper helper) : TagHelper
{
	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; } = new();

	public string AspHref { get; set; } = "";

	public string WarningMessage { get; set; } = "Are you sure you want to delete this record?";

	public string ActionName { get; set; } = "Delete";

	public bool AskReason { get; set; }

	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		var existingClassAttr = output.Attributes.FirstOrDefault(a => a.Name == "class");
		var existingCssClass = existingClassAttr?.Value.ToString() ?? "";
		if (existingClassAttr is not null)
		{
			output.Attributes.Remove(existingClassAttr);
		}

		((IViewContextAware)helper).Contextualize(ViewContext);
		var content = (await output.GetChildContentAsync()).GetContent();
		if (string.IsNullOrWhiteSpace(content))
		{
			content = "<i title=\"Delete\" class=\"fa fa-remove\"></i>";
		}

		output.TagName = "span";
		var uniqueId = UniqueId();

		var antiForgeryToken = helper.AntiForgeryToken().GetString();

		string reasonInput = "";
		if (AskReason)
		{
			reasonInput = """
							<div class="container">
								<label for="reason">Reason</label>
								<textarea rows="4" id="reason" name="reason" class="form-control"></textarea>
							</div>
							""";
		}

		output.Content.SetHtmlContent(
			$"""
				<button type='button' class='btn btn-danger {existingCssClass}' data-bs-toggle='modal' data-bs-target='#areYouSureModal{uniqueId}'>{content}</button>
				<div id='areYouSureModal{uniqueId}' class='modal fade' role='dialog'>
					<div class='modal-dialog'>
						<div class='modal-content'>
							<div class='modal-header'>
								<h5 class='modal-title text-danger'>Delete Warning!</h5>
								<button type='button' class='btn-close' data-bs-dismiss='modal'></button>
							</div>
							<div class='modal-body'>
								<p>{TagHelperExtensions.Text(WarningMessage)}</p>
							</div>
							<form action='{WebUtility.UrlDecode(AspHref)}' method='post'>
								<div class='modal-footer'>
									{antiForgeryToken}
									{reasonInput}
									<button type='submit' class='text-center btn btn-danger'>{ActionName}</button>
									<button type='button' class='btn btn-secondary' data-bs-dismiss='modal'>Cancel</button>
								</div>
							</form>
						</div>
					</div>
				</div>
				""");
	}

	private static string UniqueId() => Guid.NewGuid().ToString().Replace("-", "");
}
