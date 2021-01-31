using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Namotion.Reflection;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.ViewComponents;
using TASVideos.WikiEngine;
using TASVideos.WikiEngine.AST;

namespace TASVideos.TagHelpers
{
	public class WikiMarkup : TagHelper, IWriterHelper
	{
		private readonly IViewComponentHelper _viewComponentHelper;

		public WikiMarkup(IViewComponentHelper viewComponentHelper)
		{
			_viewComponentHelper = viewComponentHelper;
		}

		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext ViewContext { get; set; } = new ();

		public string Markup { get; set; } = "";
		public WikiPage PageData { get; set; } = new ();

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			((IViewContextAware)_viewComponentHelper).Contextualize(ViewContext);
			output.TagName = "article";
			output.AddCssClass("wiki");

			var sw = new StringWriter();
			Util.RenderHtmlDynamic(Markup, sw, this);
			output.Content.SetHtmlContent(sw.ToString());
		}

		bool IWriterHelper.CheckCondition(string condition)
		{
			return HtmlExtensions.WikiCondition(ViewContext, condition);
		}

		private static readonly IDictionary<string, Type> ViewComponents = Assembly
			.GetAssembly(typeof(WikiModuleAttribute))
			!.GetTypes()
			.Where(t => t.GetCustomAttribute(typeof(WikiModuleAttribute)) != null)
			.ToDictionary(tkey => ((WikiModuleAttribute)tkey.GetCustomAttribute(typeof(WikiModuleAttribute))!).Name, tvalue => tvalue, StringComparer.InvariantCultureIgnoreCase);

		void IWriterHelper.RunViewComponent(TextWriter w, string name, IReadOnlyDictionary<string, string> pp)
		{
			var componentExists = ViewComponents.TryGetValue(name, out Type? viewComponent);
			if (!componentExists)
				throw new InvalidOperationException($"Unknown ViewComponent: {name}");

			var paramObject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
			{
				{ "pageData", PageData }
			};

			var invokeMethod = viewComponent!.GetMethod("InvokeAsync")
				?? viewComponent.GetMethod("Invoke");

			if (invokeMethod == null)
				throw new InvalidOperationException($"Could not find an Invoke method on ViewComponent {viewComponent}");

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
					adapterKeyType = typeof(Nullable<>).MakeGenericType(adapterKeyType);

				if (!ParamTypeAdapters.TryGetValue(adapterKeyType, out var adapter))
				{
					// These should all exist at compile time.
					throw new InvalidOperationException($"Unknown ViewComponent Argument Type: {adapterKeyType}");
				}

				pp.TryGetValue(paramCandidate.Name!, out var ppvalue);
				var result = (object?)((dynamic)adapter).Convert(ppvalue);

				if (result == null)
				{
					// Conversion failed.  See if the parameter type is a failable type.
					var needsNonNull = paramType.IsValueType && doNullableWrap
						|| !paramType.IsValueType && paramType.ToContextualType().Nullability == Nullability.NotNullable;
					if (needsNonNull)
					{
						// TODO: Better styling, or something
						w.Write($"MODULE ERROR for `{name}`: Missing parameter value for {paramCandidate.Name}");
						return;
					}
				}

				paramObject[paramCandidate.Name!] = result;
			}

			var content = _viewComponentHelper.InvokeAsync(viewComponent, paramObject)
				.Result; // TODO: Do we want to asyncify this entire thingy?

			content.WriteTo(w, HtmlEncoder.Default);
		}

		private static readonly Dictionary<Type, object> ParamTypeAdapters = typeof(WikiMarkup)
			.Assembly
			.GetTypes()
			.Select(t => new
			{
				Type = t,
				Interface = t.GetInterfaces()
					.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IModuleParameterTypeAdapter<>))
			})
			.Where(a => a.Interface != null)
			.ToDictionary(a => a.Interface.GetGenericArguments()[0], a => Activator.CreateInstance(a.Type)!);
	}

	public interface IModuleParameterTypeAdapter<T>
	{
		T Convert(string? input);
	}

	public class StringConverter : IModuleParameterTypeAdapter<string?>
	{
		public string? Convert(string? input) => input;
	}

	public class IntConverter : IModuleParameterTypeAdapter<int?>
	{
		public int? Convert(string? input)
		{
			return int.TryParse(input, out var tmp) ? tmp : null;
		}
	}

	public class IntArrayConverter : IModuleParameterTypeAdapter<IList<int>>
	{
		public IList<int> Convert(string? input)
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

	public class DoubleConverter : IModuleParameterTypeAdapter<double?>
	{
		public double? Convert(string? input)
		{
			return double.TryParse(input, out var tmp) ? tmp : null;
		}
	}

	public class StringArrayConverter : IModuleParameterTypeAdapter<IList<string>>
	{
		public IList<string> Convert(string? input)
		{
			return (input ?? "")
				.Split(",")
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Select(s => s.Trim())
				.ToList();
		}
	}

	public class BoolConverter : IModuleParameterTypeAdapter<bool?>
	{
		public bool? Convert(string? input)
		{
			return input != null;
		}
	}

	public class DateTimeConverter : IModuleParameterTypeAdapter<DateTime?>
	{
		public DateTime? Convert(string? input)
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
