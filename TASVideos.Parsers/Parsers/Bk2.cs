using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using TASVideos.MovieParsers.Extensions;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers
{
	[FileExtension("bk2")]
	internal class Bk2 : IParser
	{
		public static string FileExtension => "bk2";

		public IParseResult Parse(Stream file)
		{
			var result = new ParseResult
			{
				Region = RegionType.Ntsc,
				FileExtension = FileExtension
			};

			var bk2Archive = new ZipArchive(file);

			var headerEntry = bk2Archive.Entries.SingleOrDefault(e => e.Name == "Header.txt");
			if (headerEntry == null)
			{
				return Error("Missing Header.txt, can not parse");
			}

			using (var stream = headerEntry.Open())
			{
				using (var reader = new StreamReader(stream))
				{
					var headerLines = reader
						.ReadToEnd()
						.LineSplit();

					string platform = headerLines.GetValueFor("platform");
					if (string.IsNullOrWhiteSpace(platform))
					{
						return Error("Could not determine the System Code");
					}

					int? rerecordVal = headerLines.GetValueFor("rerecordcount").ToInt();
					if (rerecordVal.HasValue)
					{
						result.RerecordCount = rerecordVal.Value;
					}
					else
					{
						result.WarningList.Add(ParseWarnings.MissingRerecordCount);
					}

					// Some biz system ids do not match tasvideos, convert if needed
					if (BizToTasvideosSystemIds.ContainsKey(platform))
					{
						platform = BizToTasvideosSystemIds[platform];
					}

					// Check various subsystem flags
					if (headerLines.GetValueFor("is32x").ToInt() == 1)
					{
						platform = "32x";
					}
					else if (headerLines.GetValueFor("iscgbmode").ToInt() == 1)
					{
						platform = "gbc";
					}
					else if (headerLines.GetValueFor("boardname") == "fds")
					{
						platform = "fds";
					}
					else if (headerLines.GetValueFor("issegacdmode").ToInt() == 1)
					{
						platform = "segacd";
					}
					else if (headerLines.GetValueFor("isggmode").ToInt() == 1)
					{
						platform = "gg";
					}
					else if (headerLines.GetValueFor("issgmode").ToInt() == 1)
					{
						platform = "sg1000";
					}

					result.SystemCode = platform;

					if (headerLines.GetValueFor("pal").ToInt() == 1)
					{
						result.Region = RegionType.Pal;
					}
				}
			}

			var inputLogEntry = bk2Archive.Entries.SingleOrDefault(e => e.Name == "Input Log.txt");
			if (inputLogEntry == null)
			{
				return Error("Missing Input Log.txt, can not parse");
			}

			using (var stream = inputLogEntry.Open())
			{
				using (var reader = new StreamReader(stream))
				{
					result.Frames = reader
						.ReadToEnd()
						.LineSplit()
						.Count(i => i.StartsWith("|"));
				}
			}

			return result;
		}

		private static readonly Dictionary<string, string> BizToTasvideosSystemIds = new Dictionary<string, string>
		{
			["gen"] = "genesis",
			["sat"] = "saturn",
			["dgb"] = "gb",
			["a26"] = "a2600",
			["a78"] = "a7800",
			["uze"] = "uzebox",
			["vb"] = "vboy",
			["zxspectrum"] = "zxs"
		};

		private static ErrorResult Error(string errorMsg)
		{
			return new ErrorResult(errorMsg)
			{
				FileExtension = FileExtension
			};
		}
	}
}
