using System;
using System.IO;
using System.IO.Compression;
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

					switch (ext)
					{
						default:
							return Error($".{ext} files are not currently supported.");
						case "bk2":
							using (var movieFileStream = movieFile.Open())
							{
								return new Bk2().Parse(movieFileStream);
							}
					}
				}
			}
			catch (Exception)
			{
				// TODO: do we want to log here? or catch at a higher layer?
				return Error("An general error occured while processing the movie file.");
			}
		}

		private IParseResult Error(string errorMsg)
		{
			return new ErrorResult(errorMsg);
		}
	}
}
