using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using System.ComponentModel.DataAnnotations;

namespace TASVideos.Core.Services;

public class TrimStringModelBinder : IModelBinder
{
	public Task BindModelAsync(ModelBindingContext bindingContext)
	{
		string? value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
		if (bindingContext.ModelMetadata.ConvertEmptyStringToNull && string.IsNullOrWhiteSpace(value))
		{
			value = null;
		}
		else if (value is not null)
		{
			value = value.Trim();
		}

		bindingContext.Result = ModelBindingResult.Success(value);
		return Task.CompletedTask;
	}
}

public class TrimStringModelBinderProvider : IModelBinderProvider
{
	public IModelBinder? GetBinder(ModelBinderProviderContext context)
	{
		if (context.Metadata.ModelType == typeof(string))
		{
			if (!(context.Metadata is DefaultModelMetadata metadata && metadata.Attributes.Attributes.Any(a => a is DoNotTrimAttribute || (a is DataTypeAttribute dataType && dataType.DataType == DataType.Password))))
			{
				return new TrimStringModelBinder();
			}
		}

		return null;
	}
}
