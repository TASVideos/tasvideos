using System;
using System.Collections.Generic;
using System.Linq;

namespace TASVideos.TagHelpers
{
	public partial class WikiMarkup
	{
		/// <summary>
		/// For supporting tests only
		/// </summary>
		public static T ConvertParameter<T>(string? input)
		{
			return ((ModuleParameterTypeAdapter<T>)ParamTypeAdapters[typeof(T)]).Convert(input);
		}

		private static readonly Dictionary<Type, IModuleParameterTypeAdapter> ParamTypeAdapters = typeof(WikiMarkup)
			.Assembly
			.GetTypes()
			.Where(t => t.BaseType != null
				&& t.BaseType.IsGenericType
				&& t.BaseType.GetGenericTypeDefinition() == typeof(ModuleParameterTypeAdapter<>))
			.ToDictionary(
				t => t.BaseType!.GetGenericArguments()[0],
				t => (IModuleParameterTypeAdapter)Activator.CreateInstance(t)!);

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

		private class StringConverter : ModuleParameterTypeAdapter<string?>
		{
			public override string? Convert(string? input) => input;
		}

		private class IntConverter : ModuleParameterTypeAdapter<int?>
		{
			public override int? Convert(string? input)
			{
				return int.TryParse(input, out var tmp) ? tmp : null;
			}
		}

		private class IntArrayConverter : ModuleParameterTypeAdapter<IList<int>>
		{
			public override IList<int> Convert(string? input)
			{
				input ??= "";
				return input.Split(',')
					.Select(s =>
					{
						var b = int.TryParse(s, out var i);
						return new { b, i };
					})
					.Where(a => a.b)
					.Select(a => a.i)
					.ToList();
			}
		}

		private class DoubleConverter : ModuleParameterTypeAdapter<double?>
		{
			public override double? Convert(string? input)
			{
				return double.TryParse(input, out var tmp) ? tmp : null;
			}
		}

		private class StringArrayConverter : ModuleParameterTypeAdapter<IList<string>>
		{
			public override IList<string> Convert(string? input)
			{
				return (input ?? "")
					.Split(",")
					.Where(s => !string.IsNullOrWhiteSpace(s))
					.Select(s => s.Trim())
					.ToList();
			}
		}

		private class BoolConverter : ModuleParameterTypeAdapter<bool?>
		{
			public override bool? Convert(string? input)
			{
				return input != null;
			}
		}

		private class DateTimeConverter : ModuleParameterTypeAdapter<DateTime?>
		{
			public override DateTime? Convert(string? input)
			{
				if (input?.Length >= 1 && (input[0] == 'Y' || input[0] == 'y'))
				{
					var tmp = int.TryParse(input[1..], out var year);
					if (tmp)
					{
						return new DateTime(year, 1, 1);
					}
				}

				return null;
			}
		}
	}
}
