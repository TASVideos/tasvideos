using System.Text;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("fm3")]
internal class Fm3 : Parser, IParser
{
	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc,
			SystemCode = SystemCodes.Nes
		};

		(var header, int initialFrameCount) = await file.PipeBasedMovieHeaderAndFrameCount();

		// Validate this is actually an FM3 file by checking version
		var version = header.GetPositiveIntFor(Keys.Version);
		if (version != 3)
		{
			return Error($"Invalid FM3 version. Expected 3, found {version?.ToString() ?? "none"}");
		}

		// Check if we have a valid ROM filename (required for FM3)
		var romFilename = header.GetValueFor(Keys.RomFilename);
		if (string.IsNullOrWhiteSpace(romFilename))
		{
			return Error("Missing required romFilename field");
		}

		// Check if we have a valid ROM checksum (required for FM3)
		var romChecksum = header.GetValueFor(Keys.RomChecksum);
		if (string.IsNullOrWhiteSpace(romChecksum))
		{
			return Error("Missing required romChecksum field");
		}

		// Check if we have a GUID (required for FM3)
		var guid = header.GetValueFor(Keys.Guid);
		if (string.IsNullOrWhiteSpace(guid))
		{
			return Error("Missing required guid field");
		}

		// Handle frame count - similar to FM2 logic
		if (header.GetBoolFor(Keys.Binary))
		{
			int? frameCount = header.GetPositiveIntFor(Keys.Length);
			if (frameCount.HasValue)
			{
				result.Frames = frameCount.Value;
			}
			else
			{
				return Error("No frame count found for binary format");
			}
		}
		else
		{
			result.Frames = initialFrameCount;
		}

		// Handle system detection
		if (header.GetBoolFor(Keys.Fds))
		{
			result.SystemCode = SystemCodes.Fds;
		}

		// Handle rerecord count
		int? rerecordVal = header.GetPositiveIntFor(Keys.RerecordCount);
		if (rerecordVal.HasValue)
		{
			result.RerecordCount = rerecordVal.Value;
		}
		else
		{
			result.WarnNoRerecords();
		}

		// Handle region detection
		if (header.GetBoolFor(Keys.Pal))
		{
			result.Region = RegionType.Pal;
		}

		// Handle start type
		if (header.HasValue(Keys.StartsFromSavestate))
		{
			result.StartType = MovieStartType.Savestate;
		}

		// Handle ROM checksum hash extraction
		var hashLine = header.GetValueFor(Keys.RomChecksum);
		if (!string.IsNullOrWhiteSpace(hashLine))
		{
			var hashSplit = hashLine.Split(':');
			var base64Line = hashSplit.Length == 2 ? hashSplit[1] : "";
			if (!string.IsNullOrWhiteSpace(base64Line))
			{
				try
				{
					byte[] data = Convert.FromBase64String(base64Line);
					string hash = BytesToHexString(data.AsSpan());
					if (hash.Length == 32)
					{
						result.Hashes.Add(HashType.Md5, hash.ToLower());
					}
				}
				catch
				{
					// Treat an invalid base64 hash as a missing hash
				}
			}
		}

		return result;
	}

	private static string BytesToHexString(ReadOnlySpan<byte> bytes)
	{
		StringBuilder sb = new(capacity: 2 * bytes.Length, maxCapacity: 2 * bytes.Length);
		foreach (var b in bytes)
		{
			sb.Append($"{b:X2}");
		}

		return sb.ToString();
	}

	private static class Keys
	{
		public const string Version = "version";
		public const string RomFilename = "romFilename";
		public const string RomChecksum = "romChecksum";
		public const string Guid = "guid";
		public const string RerecordCount = "rerecordCount";
		public const string Pal = "palFlag";
		public const string Binary = "binary";
		public const string Length = "length";
		public const string Fds = "FDS";
		public const string StartsFromSavestate = "savestate";
		public const string Comment = "comment";
		public const string Subtitle = "subtitle";
		public const string EmuVersion = "emuVersion";
		public const string NewPpu = "NewPPU";
		public const string Fourscore = "fourscore";
	}
}
