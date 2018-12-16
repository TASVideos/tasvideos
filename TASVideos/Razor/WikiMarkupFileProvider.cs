using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using TASVideos.Services;
using TASVideos.WikiEngine;

namespace TASVideos.Razor
{
	public class WikiMarkupFileProvider : IFileProvider
	{
		public const string Prefix = "/Views/~~~";
		private const string PreviewPrefix = "/Views/Preview/~~~";

		private readonly Dictionary<string, PreviewMarkupCacheInfo> _previewCache = new Dictionary<string, PreviewMarkupCacheInfo>();

		private int _previewNameIndex;

		public IWikiPages WikiPages { get; set; }

		public string SetPreviewMarkup(string content)
		{
			var key = GetPreviewName();
			lock (_previewCache)
			{
				_previewCache.Add(key, new PreviewMarkupCacheInfo
				{
					Expiry = DateTime.UtcNow.AddMinutes(1),
					Markup = content
				});
			}

			return key;
		}

		public IDirectoryContents GetDirectoryContents(string subpath)
		{
			return null;
		}

		public IFileInfo GetFileInfo(string subpath)
		{
			if (!subpath.StartsWith(Prefix) && !subpath.StartsWith(PreviewPrefix))
			{
				return null;
			}

			string pageName, markup;

			if (subpath.StartsWith(PreviewPrefix))
			{
				pageName = "foobar"; // what goes here?
				markup = GetPreviewMarkup(subpath);
			}
			else
			{
				subpath = subpath.Substring(Prefix.Length);
				var continuation = WikiPages.Revision(int.Parse(subpath));
				var result = continuation;
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
			if (filter.StartsWith(PreviewPrefix))
			{
				return new ForceChangeToken();
			}

			return null;
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

					return _previewCache[key].Markup;
				}
			}

			return _previewCache[key].Markup;
		}

		private string GetPreviewName()
		{
			return PreviewPrefix + Interlocked.Increment(ref _previewNameIndex);
		}

		private struct PreviewMarkupCacheInfo
		{
			public DateTime Expiry;
			public string Markup;
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
