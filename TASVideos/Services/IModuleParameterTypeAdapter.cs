using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TASVideos.TagHelpers;
using TASVideos.ViewComponents;

namespace TASVideos.Services
{
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

	public static class ModuleParamHelpers
	{
		public static readonly Dictionary<Type, IModuleParameterTypeAdapter> ParamTypeAdapters = typeof(WikiMarkup)
			.Assembly
			.GetTypes()
			.Where(t => t.BaseType != null
				&& t.BaseType.IsGenericType
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


	}
}
