using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using TASVideos.WikiEngine;

namespace TASVideos.Razor
{
	public class WikiMarkupFileProvider : IFileProvider
	{
		private readonly IServiceProvider _provider;

		public const string Prefix = "/Views/~~~";

		public WikiMarkupFileProvider(IServiceProvider provider)
		{
			_provider = provider;
		}
		
		public IDirectoryContents GetDirectoryContents(string subpath)
		{
			return null;
		}

		public IFileInfo GetFileInfo(string subpath)
		{
			if (!subpath.StartsWith(Prefix))
			{
				return null;
			}

			subpath = subpath.Substring(Prefix.Length);
			var tasks = (Tasks.WikiTasks)_provider.GetService(typeof(Tasks.WikiTasks));
			var continuation = tasks.GetPage(int.Parse(subpath));
			continuation.Wait();
			var result = continuation.Result;
			if (result == null)
			{
				return null;
			}
			
			var ms = new MemoryStream();
			using (var tw = new StreamWriter(ms))
			{
				Util.RenderRazor(result.PageName, result.Markup, tw);
			}

			return new MyFileInfo(result.PageName, ms.ToArray());
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
