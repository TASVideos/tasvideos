using System.Reflection;
using Namotion.Reflection;
using TASVideos.Data.Entity;
using TASVideos.TagHelpers;
using TASVideos.ViewComponents;

namespace TASVideos.Services;

public static class ModuleParamHelpers
{
	public static readonly Dictionary<Type, IModuleParameterTypeAdapter> ParamTypeAdapters = typeof(WikiMarkup)
		.Assembly
		.GetTypes()
		.Where(t => t.BaseType is { IsGenericType: true }
			&& t.BaseType.GetGenericTypeDefinition() == typeof(ModuleParameterTypeAdapter<>))
		.ToDictionary(
			t => t.BaseType!.GetGenericArguments()[0],
			t => (IModuleParameterTypeAdapter)Activator.CreateInstance(t)!);

	public static readonly IDictionary<string, Type> ViewComponents = Assembly
		.GetAssembly(typeof(WikiModuleAttribute))
		!.GetTypes()
		.Where(t => t.GetCustomAttribute(typeof(WikiModuleAttribute)) != null)
		.ToDictionary(tkey => ((WikiModuleAttribute)tkey.GetCustomAttribute(typeof(WikiModuleAttribute))!).Name, tvalue => tvalue, StringComparer.InvariantCultureIgnoreCase);

	// TODO: reuse code above
	public static readonly IDictionary<string, Type> TextComponents = Assembly
		.GetAssembly(typeof(WikiModuleAttribute))
		!.GetTypes()
		.Where(t => t.GetCustomAttribute(typeof(WikiModuleAttribute)) != null)
		.Where(t => t.GetCustomAttribute(typeof(TextModuleAttribute)) != null)
		.ToDictionary(tkey => ((WikiModuleAttribute)tkey.GetCustomAttribute(typeof(WikiModuleAttribute))!).Name, tvalue => tvalue, StringComparer.InvariantCultureIgnoreCase);

	public static IDictionary<string, object?> GetParameterData(
		TextWriter w,
		string name,
		MethodInfo invokeMethod,
		WikiPage pageData,
		IReadOnlyDictionary<string, string> pp)
	{
		var paramObject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
			{
				{ "pageData", pageData }
			};

		var paramCandidates = invokeMethod
			.GetParameters()
			.Where(p => !paramObject.ContainsKey(p.Name!)); // filter out any already supplied parameters

		foreach (var paramCandidate in paramCandidates)
		{
			var paramType = paramCandidate.ParameterType;
			var adapterKeyType = paramType;
			var doNullableWrap = paramType.IsValueType
				&& (!paramType.IsGenericType || paramType.GetGenericTypeDefinition() != typeof(Nullable<>));

			if (doNullableWrap)
			{
				adapterKeyType = typeof(Nullable<>).MakeGenericType(adapterKeyType);
			}

			if (!ParamTypeAdapters.TryGetValue(adapterKeyType, out var adapter))
			{
				// These should all exist at compile time.
				throw new InvalidOperationException($"Unknown ViewComponent Argument Type: {adapterKeyType}");
			}

			pp.TryGetValue(paramCandidate.Name!, out var ppValue);
			var result = adapter.Convert(ppValue);

			if (result is null)
			{
				// Conversion failed.  See if the parameter type is a failable type.
				var needsNonNull = paramType.IsValueType && doNullableWrap
					|| !paramType.IsValueType && paramType.ToContextualType().Nullability == Nullability.NotNullable;
				if (needsNonNull)
				{
					// TODO: Better styling, or something
					// TODO: do not pass in TextWriter and this isn't how it should report that failure. We should return some failure state to the caller which can then handle output
					w.Write($"MODULE ERROR for `{name}`: Missing parameter value for {paramCandidate.Name}");
					return new Dictionary<string, object?>();
				}
			}

			paramObject[paramCandidate.Name!] = result;
		}

		return paramObject;
	}
}
