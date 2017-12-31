using System;

namespace TASVideos.MovieParsers
{

	/// <summary>
	/// Decorates an <see cref="IParser" /> implementation to
	/// indicate which file extension it is capable of parsing
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	internal class FileExtensionAttribute : Attribute
    {
		public string Extension { get; }

		public FileExtensionAttribute(string extension)
		{
			Extension = extension;
		}
    }
}
