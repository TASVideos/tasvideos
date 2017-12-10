using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace TASVideos.Razor
{
	public class WikiMarkupFileProvider : IFileProvider
	{
		private IServiceProvider _provider;

		public WikiMarkupFileProvider(IServiceProvider provider)
		{
			_provider = provider;
		}
		public const string Prefix = "/Views/~~~";
		public IDirectoryContents GetDirectoryContents(string subpath)
		{
			return null;
		}

		public IFileInfo GetFileInfo(string subpath)
		{
			if (!subpath.StartsWith(Prefix))
				return null;
			subpath = subpath.Substring(Prefix.Length);
			var tasks = (Tasks.WikiTasks)_provider.GetService(typeof(Tasks.WikiTasks));
			var continuation = tasks.GetPage(int.Parse(subpath));
			continuation.Wait();
			var result = continuation.Result;
			if (result == null)
				return null;
			
			var ms = new MemoryStream();
			using (var tw = new StreamWriter(ms))
			{
				TASVideos.WikiEngine.Util.DebugWriteHtml(result.Markup, tw);
			}
			return new MyFileInfo(result.PageName, ms.ToArray());
		}

		public IChangeToken Watch(string filter)
		{
			return null;
		}

		private class MyFileInfo : IFileInfo
		{
			private byte[] _data;

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
