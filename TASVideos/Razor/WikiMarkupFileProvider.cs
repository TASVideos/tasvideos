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
		public const string Prefix = "/Pages/~~~";

		private int _previewNameIndex;

		public IWikiPages WikiPages { get; set; }

		public IDirectoryContents GetDirectoryContents(string subPath)
		{
			return null;
		}

		public IFileInfo GetFileInfo(string subPath)
		{
			if (!subPath.StartsWith(Prefix))
			{
				return null;
			}

			string pageName, markup;


				subPath = subPath.Substring(Prefix.Length);
				var continuation = WikiPages.Revision(int.Parse(subPath));
				var result = continuation;
				if (result == null)
				{
					return null;
				}

				pageName = result.PageName;
				markup = result.Markup;

			var ms = new MemoryStream();
			using (var tw = new StreamWriter(ms))
			{
				Util.RenderRazor(pageName, markup, tw);
			}

			return new MyFileInfo(pageName, ms.ToArray());
		}

		public IChangeToken Watch(string filter)
		{
			return null;
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
