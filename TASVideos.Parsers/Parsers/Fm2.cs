using System.Text;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("fm2")]
internal class Fm2 : Parser, IParser
{
	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc,
			SystemCode = SystemCodes.Nes
		};

		(var header, int initialFrameCount) = await file.PipeBasedMovieHeaderAndFrameCount();

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

		if (header.GetBoolFor(Keys.Fds))
		{
			result.SystemCode = SystemCodes.Fds;
		}

		int? rerecordVal = header.GetPositiveIntFor(Keys.RerecordCount);
		if (rerecordVal.HasValue)
		{
			result.RerecordCount = rerecordVal.Value;
		}
		else
		{
			result.WarnNoRerecords();
		}

		if (header.GetBoolFor(Keys.Pal))
		{
			result.Region = RegionType.Pal;
		}

		if (header.HasValue(Keys.StartsFromSavestate))
		{
			result.StartType = MovieStartType.Savestate;
		}

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
		public const string RerecordCount = "rerecordcount";
		public const string Pal = "palFlag";
		public const string Binary = "binary";
		public const string Length = "length";
		public const string Fds = "fds";
		public const string StartsFromSavestate = "savestate";
		public const string RomChecksum = "romChecksum";
	}
}
