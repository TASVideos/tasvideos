using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace TASVideos.Services;

public class DevFallbackFileProvider(IFileProvider originalProvider) : IFileProvider
{
	public IDirectoryContents GetDirectoryContents(string subpath) => originalProvider.GetDirectoryContents(subpath);

	public IFileInfo GetFileInfo(string subpath)
	{
		var fileInfo = originalProvider.GetFileInfo(subpath);
		if (!fileInfo.Exists && subpath.StartsWith("/media/"))
		{
			return originalProvider.GetFileInfo("/images/tasvideos_rss.png");
		}

		return fileInfo;
	}

	public IChangeToken Watch(string filter) => originalProvider.Watch(filter);
}
