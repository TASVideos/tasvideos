using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Namotion.Reflection;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;
using TASVideos.WikiEngine.AST;

namespace TASVideos.Services
{
	public class WikiToTextRenderer : IWikiToTextRenderer
	{
		private readonly AppSettings _settings;
		private readonly IServiceProvider _serviceProvider;

		public WikiToTextRenderer(AppSettings settings, IServiceProvider serviceProvider)
		{
			_settings = settings;
			_serviceProvider = serviceProvider;
		}

		public async Task<string> RenderWikiForYoutube(WikiPage page)
		{
			var sw = new StringWriter();
			await Util.RenderTextAsync(page.Markup, sw, new WriterHelper(_settings.BaseUrl, _serviceProvider, page));
			return sw.ToString();
		}

		private class WriterHelper : IWriterHelper
		{
			private readonly string _host;
			private readonly IServiceProvider _serviceProvider;
			private readonly WikiPage _wikiPage;

			public WriterHelper(string host, IServiceProvider serviceProvider, WikiPage wikiPage)
			{
				_host = host;
				_serviceProvider = serviceProvider;
				_wikiPage = wikiPage;
			}

			public bool CheckCondition(string condition)
			{
				bool result = false;

				if (condition.StartsWith('!'))
				{
					result = true;
					condition = condition.TrimStart('!');
				}

				switch (condition)
				{
					case "1":
						result ^= true;
						break;
					default:
						result ^= false;
						break;
				}

				return result;
			}

			public async Task RunViewComponentAsync(TextWriter w, string name, IReadOnlyDictionary<string, string> pp)
			{
				var componentExists = ModuleParamHelpers.TextComponents.TryGetValue(name, out Type? textComponent);
				if (!componentExists)
				{
					return;
				}

				var invokeMethod = textComponent!.GetMethod("RenderTextAsync");
				//?? textComponent.GetMethod("RenderText"); // TODO

				if (invokeMethod == null)
				{
					throw new InvalidOperationException($"Could not find an RenderText method on ViewComponent {textComponent}");
				}

				var paramObject = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
				{
					{ "pageData", _wikiPage }
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

					if (!ModuleParamHelpers.ParamTypeAdapters.TryGetValue(adapterKeyType, out var adapter))
					{
						// These should all exist at compile time.
						throw new InvalidOperationException($"Unknown ViewComponent Argument Type: {adapterKeyType}");
					}

					pp.TryGetValue(paramCandidate.Name!, out var ppvalue);
					var result = adapter.Convert(ppvalue);

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

				var module = _serviceProvider.GetRequiredService(textComponent);
				var x = paramObject.Values.ToArray();
				var task = (Task)invokeMethod.Invoke(module, x)!;
				await task;
				var resultProperty = task.GetType().GetProperty("Result")!;
				var str = resultProperty.GetValue(task);
				w.Write(str);
			}

			public string AbsoluteUrl(string url)
			{
				if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var parsed))
				{
					return url;
				}

				if (!parsed.IsAbsoluteUri)
				{
					return _host.TrimEnd('/') + url;
				}

				return url;
			}
		}
	}
}
