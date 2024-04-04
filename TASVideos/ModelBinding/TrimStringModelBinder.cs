using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace TASVideos.ModelBinding;

public class TrimStringModelBinder : IModelBinder
{
	public Task BindModelAsync(ModelBindingContext bindingContext)
	{
		string? value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
		if (!string.IsNullOrWhiteSpace(value))
		{
			value = value.Trim();
			bindingContext.Result = ModelBindingResult.Success(value);
		}

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
