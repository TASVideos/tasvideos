namespace TASVideos.Services;

public interface IModuleParameterTypeAdapter
{
	object? Convert(string? input);
}

public abstract class ModuleParameterTypeAdapter<T> : IModuleParameterTypeAdapter
{
	public abstract T Convert(string? input);
	object? IModuleParameterTypeAdapter.Convert(string? input)
	{
		return (object?)Convert(input);
	}
}
