using System;
using System.Collections.Generic;
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
				Region = RegionType.Ntsc,
				FileExtension = FileExtension
			};

			var bk2Archive = new ZipArchive(file);

			var inputLogEntry = bk2Archive.Entries.SingleOrDefault(e => e.Name == "Input Log.txt");
			if (inputLogEntry == null)
			{
				return new ErrorResult("Missing Input Log.txt, can not parse") { FileExtension = FileExtension };
			}

			using (var stream = inputLogEntry.Open())
			{
				using (var reader = new StreamReader(stream))
				{
					string[] inputLog = reader.ReadToEnd().Split('\n');
					result.Frames = inputLog.Count(i => i.StartsWith("|"));
				}
			}

			var headerEntry = bk2Archive.Entries.SingleOrDefault(e => e.Name == "Header.txt");
			if (headerEntry == null)
			{
				return new ErrorResult("Missing Header.txt, can not parse") { FileExtension = FileExtension };
			}

			using (var stream = headerEntry.Open())
			{
				using (var reader = new StreamReader(stream))
				{
					string[] headerLines = reader.ReadToEnd().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

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
						return new ErrorResult("Could not determine the System Code") { FileExtension = FileExtension };
					}

					// Some biz system ids do not match tasvideos, convert if needed
					if (BizToTasvideosSystemIds.ContainsKey(platform))
					{
						platform = BizToTasvideosSystemIds[platform];
					}

					// Check various subsystem flags
					if (GetInt(GetValue(headerLines, "is32x")) == 1)
					{
						platform = "32x";
					}
					else if (GetInt(GetValue(headerLines, "iscgbmode")) == 1)
					{
						platform = "gbc";
					}
					else if (GetValue(headerLines, "boardname") == "fds")
					{
						platform = "fds";
					}
					else if (GetInt(GetValue(headerLines, "issegacdmode")) == 1)
					{
						platform = "segacd";
					}
					else if (GetInt(GetValue(headerLines, "isggmode")) == 1)
					{
						platform = "gg";
					}
					else if (GetInt(GetValue(headerLines, "issgmode")) == 1)
					{
						platform = "sg1000";
					}

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

		private static string GetValue(string[] lines, string header) // Case insensitive
		{
			if (lines == null || !lines.Any() || string.IsNullOrWhiteSpace(header))
			{
				return "";
			}

			var row = lines.FirstOrDefault(l => l.ToLower().StartsWith(header.ToLower()))?.ToLower();
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
