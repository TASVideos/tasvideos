using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace TASVideos.MovieParsers
{
	[FileExtension("bk2")]
	internal class Bk2 : IParser
	{
		public string FileExtension => "bk2";

		public IParseResult Parse(Stream file)
		{
			var result = new ParseResult
			{
				Region = RegionType.Ntsc
			};

			var bk2Archive = new ZipArchive(file);

			var inputLogEntry = bk2Archive.Entries.Single(e => e.Name == "Input Log.txt");
			using (var stream = inputLogEntry.Open())
			{
				using (var reader = new StreamReader(stream))
				{
					string[] inputLog = reader.ReadToEnd().Split('\n');
					result.Frames = inputLog.Count(i => i.StartsWith('|'));
				}
			}

			var headerEntry = bk2Archive.Entries.Single(e => e.Name == "Header.txt");
			using (var stream = headerEntry.Open())
			{
				using (var reader = new StreamReader(stream))
				{
					string[] headerLines = reader.ReadToEnd().Split(new [] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

					int? rerecordVal = GetInt(GetValue(headerLines, "rerecordcount"));
					if (rerecordVal.HasValue)
					{
						result.RerecordCount = rerecordVal.Value;
					}
					else
					{
						result.WarningList.Add("Could not determine the rerecord count, using 0 instead");
					}

					string platform = GetValue(headerLines, "platform");
					if (string.IsNullOrWhiteSpace(platform))
					{
						return new ErrorResult("Could not determine the System Code");
					}

					// TODO: bk2's are more complex than this, we need to map bizhawks codes with tasvideos
					// in bk2's, there are flags for systems like sg, pcecd, etc
					result.SystemCode = platform;

					int? pal = GetInt(GetValue(headerLines, "pal"));
					if (pal == 1)
					{
						result.Region = RegionType.Pal;
					}
				}
			}

			return result;
		}

		private static string GetValue(string[] lines, string header) // Case insensitive
		{
			if (lines == null || !lines.Any() || string.IsNullOrWhiteSpace(header))
			{
				return "";
			}

			var row = lines.FirstOrDefault(l => l.ToLower().StartsWith(header?.ToLower()))?.ToLower();
			if (!string.IsNullOrWhiteSpace(row))
			{
				var valstr = row.Replace(header.ToLower(), "").Trim().Replace("\r", "").Replace("\n", "");
				
				return valstr;
			}

			return "";
		}

		private static int? GetInt(string val)
		{
			var result = int.TryParse(val, out int parsedVal);
			if (result)
			{
				return parsedVal;
			}

			return null;
		}
	}
}
