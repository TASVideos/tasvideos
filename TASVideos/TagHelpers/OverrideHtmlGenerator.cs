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

	public override TagBuilder GenerateValidationMessage(ViewContext viewContext, ModelExplorer modelExplorer, string expression, string message, string tag, object htmlAttributes)
	{
		var builder = base.GenerateValidationMessage(viewContext, modelExplorer, expression, message, tag, htmlAttributes);
		builder.AddCssClass("text-danger");
		return builder;
	}

	public override TagBuilder GenerateTextBox(ViewContext viewContext, ModelExplorer modelExplorer, string expression, object value, string format, object htmlAttributes)
	{
		var builder = base.GenerateTextBox(viewContext, modelExplorer, expression, value, format, htmlAttributes);
		if (builder.Attributes.TryGetValue("type", out var type))
		{
			if (type is "text" or "email" or "number" or "url" or "file" or "datetime-local")
			{
				builder.AddCssClass("form-control");
			}
		}

		return builder;
	}

	public override TagBuilder GenerateTextArea(ViewContext viewContext, ModelExplorer modelExplorer, string expression, int rows, int columns, object htmlAttributes)
	{
		var builder = base.GenerateTextArea(viewContext, modelExplorer, expression, rows, columns, htmlAttributes);
		builder.AddCssClass("form-control");
		return builder;
	}

	public override TagBuilder GenerateSelect(ViewContext viewContext, ModelExplorer modelExplorer, string optionLabel, string expression, IEnumerable<SelectListItem> selectList, ICollection<string> currentValues, bool allowMultiple, object htmlAttributes)
	{
		var builder = base.GenerateSelect(viewContext, modelExplorer, optionLabel, expression, selectList, currentValues, allowMultiple, htmlAttributes);
		builder.AddCssClass("form-select");
		return builder;
	}
}
