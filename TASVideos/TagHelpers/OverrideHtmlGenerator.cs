using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace TASVideos.TagHelpers;

public class OverrideHtmlGenerator(
	IAntiforgery antiforgery,
	IOptions<MvcViewOptions> optionsAccessor,
	IModelMetadataProvider metadataProvider,
	IUrlHelperFactory urlHelperFactory,
	HtmlEncoder htmlEncoder,
	ValidationHtmlAttributeProvider validationAttributeProvider) : DefaultHtmlGenerator(
	antiforgery,
	optionsAccessor,
	metadataProvider,
	urlHelperFactory,
	htmlEncoder,
	validationAttributeProvider)
{
	public override TagBuilder GenerateLabel(
		ViewContext viewContext,
		ModelExplorer modelExplorer,
		string expression,
		string labelText,
		object htmlAttributes)
	{
		var generatedLabelText = labelText;
		if (string.IsNullOrEmpty(generatedLabelText))
		{
			generatedLabelText = modelExplorer.Metadata.DisplayName
				?? modelExplorer.Metadata.PropertyName.SplitCamelCase();
		}

		return base.GenerateLabel(viewContext, modelExplorer, expression, generatedLabelText, htmlAttributes);
	}
}
