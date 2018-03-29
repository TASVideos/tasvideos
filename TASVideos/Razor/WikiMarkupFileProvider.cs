using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using TASVideos.Tasks;
using TASVideos.WikiEngine;
using System.Threading;
using System.Collections.Generic;

namespace TASVideos.Razor
{
	public class WikiMarkupFileProvider : IFileProvider
	{
		public const string Prefix = "/Views/~~~";
		private const string PreviewNamePrefix = "/Views/~~~Preview";
		private int _previewNameIndex = 0;

		private struct PreviewMarkupCacheInfo
		{
			public DateTime Expiry;
			public string Markup;
		}

		private readonly Dictionary<string, PreviewMarkupCacheInfo> _previewCache = new Dictionary<string, PreviewMarkupCacheInfo>();

		private string GetPreviewName()
		{
			return PreviewNamePrefix + Interlocked.Increment(ref _previewNameIndex);
		}

		public string SetPreviewMarkup(string content)
		{
			var key = GetPreviewName();
			lock (_previewCache)
			{
				_previewCache.Add(key, new PreviewMarkupCacheInfo
				{
					Expiry = DateTime.UtcNow.AddMinutes(1),
					Markup = content
				};
			}
			return key;
		}

		private string GetPreviewMarkup(string key)
		{
			if (key.EndsWith("00"))
			{
				lock (_previewCache)
				{
					var now = DateTime.UtcNow;
					foreach (var kvp in _previewCache.ToList())
					{
						if (kvp.Value.Expiry < now)
						{
							_previewCache.Remove(kvp.Key);
						}
					}
					return _previewCache[key];
				}
			}
			else
			{
				return _previewCache[key];
			}
		}

		private readonly WikiTasks _wikiTasks;

		public WikiMarkupFileProvider(IServiceProvider provider)
		{
			// Unfortunatley the singleton cache in WikiTasks is different here than in a non-single class,
			// so we need to populate another cache just for this
			// Boo.
			_wikiTasks = (WikiTasks)provider.GetService(typeof(WikiTasks));
			_wikiTasks.LoadWikiCache(true).Wait();
		}

		public string PreviewMarkup { get; set; }

		public IDirectoryContents GetDirectoryContents(string subpath)
		{
			return null;
		}

		public IFileInfo GetFileInfo(string subpath)
		{
			if (!subpath.StartsWith(Prefix) && subpath != PreviewName)
			{
				return null;
			}

			string pageName, markup;

			if (subpath.StartsWith(PreviewNamePrefix))
			{
				pageName = "foobar"; // what goes here?
				markup = GetPreviewMarkup(subpath);
			}
			else
			{
				subpath = subpath.Substring(Prefix.Length);
				var continuation = _wikiTasks.GetPageById(int.Parse(subpath));
				continuation.Wait();
				var result = continuation.Result;
				if (result == null)
				{
					return null;
				}

				pageName = result.PageName;
				markup = result.Markup;
			}

			var ms = new MemoryStream();
			using (var tw = new StreamWriter(ms))
			{
				Util.RenderRazor(pageName, markup, tw);
			}

			return new MyFileInfo(pageName, ms.ToArray());
		}

		public IChangeToken Watch(string filter)
		{
			if (filter == PreviewName)
			{
				return new ForceChangeToken();
			}

			return null;
		}

		private class ForceChangeToken : IChangeToken
		{
			public bool HasChanged => true;
			public bool ActiveChangeCallbacks => false;
			public IDisposable RegisterChangeCallback(Action<object> callback, object state)
			{
				return null;
			}
		}

		private class MyFileInfo : IFileInfo
		{
			private readonly byte[] _data;

			public MyFileInfo(string name, byte[] data)
			{
				_data = data;
				Name = name;
				LastModified = DateTimeOffset.UtcNow;
			}

			public bool Exists => true;

			public long Length => _data.Length;

			public string PhysicalPath => null;

			public string Name { get; }

			public DateTimeOffset LastModified { get; }

			public bool IsDirectory => false;

			public Stream CreateReadStream()
			{
				return new MemoryStream(_data, false);
			}
		}
	}
}
