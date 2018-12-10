using System;

namespace TASVideos.MovieParsers.Parsers
{
	/// <summary>
	/// Decorates an <see cref="IParser" /> implementation to
	/// indicate which file extension it is capable of parsing
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	internal class FileExtensionAttribute : Attribute
    {
		public FileExtensionAttribute(string extension)
		{
			Extension = extension;
		}

		public string Extension { get; }
	}
}
