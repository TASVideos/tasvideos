using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using TASVideos.MovieParsers.Parsers;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers
{
	/// <summary>
	/// The entry point for movie file parsers
	/// Takes a stream of the zip file containing a movie file
	/// The file must have precisely one file
	/// The file is processed and a <see cref="IParseResult"/>
	/// is returned
	/// </summary>
	/// <seealso cref="IParseResult"/>
	public sealed class MovieParser
	{
		private static readonly ICollection<Type> ParserTypes =
			typeof(IParser).Assembly
				.GetTypes()
				.Where(t => typeof(IParser).IsAssignableFrom(t))
				.Where(t => t != typeof(IParser))
				.Where(t => t.GetCustomAttributes().OfType<FileExtensionAttribute>().Any())
				.ToList();

		public IParseResult Parse(Stream stream)
		{
			try
			{
				using (var zip = new ZipArchive(stream))
				{
					if (zip.Entries.Count > 1)
					{
						return Error("Multiple files detected in the .zip, only one file is allowed");
					}

					var movieFile = zip.Entries[0];
					var ext = Path.GetExtension(movieFile.Name).Trim('.').ToLower();

					var parser = GetParser(ext);
					if (parser == null)
					{
						return Error($".{ext} files are not currently supported.");
					}

					using (var movieFileStream = movieFile.Open())
					{
						return parser.Parse(movieFileStream);
					}
				}
			}
			catch (Exception)
			{
				// TODO: do we want to log here? or catch at a higher layer?
				return Error("An general error occured while processing the movie file.");
			}
		}

		public IEnumerable<string> SupportedMovieExtensions => ParserTypes
			.Select(t => "." + (t.GetCustomAttribute(typeof(FileExtensionAttribute)) as FileExtensionAttribute)
					?.Extension);

		private IParser GetParser(string ext)
		{
			var type = ParserTypes
				.SingleOrDefault(t => (t.GetCustomAttribute(typeof(FileExtensionAttribute)) as FileExtensionAttribute)
					?.Extension == ext);
			
			if (type == null)
			{
				return null;
			}

			return Activator.CreateInstance(type) as IParser;
		}

		private static IParseResult Error(string errorMsg)
		{
			return new ErrorResult(errorMsg);
		}
	}
}
