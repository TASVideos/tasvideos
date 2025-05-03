using System.Security.Cryptography;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("bk2")]
internal class Bk2 : Parser, IParser
{
	private const string HeaderFile = "header";
	private const string InputFile = "input log";
	private const string CommentFile = "comments";

	// hacky framerate fields, taken from platform framerates
	private const double NtscNesFramerate = 60.0988138974405;
	private const double NtscSnesFramerate = 60.0988138974405;
	private const double PalSnesFramerate = 50.0069789081886;
	private const double NtscSatFramerate = 59.8830284837373;

	// mednafen values to match current octoshock
	private const double NtscPsxFramerate = 59.94006013870239;
	private const double PalPsxFramerate = 50.00028192996979;

	protected virtual string[] InvalidArchiveEntries =>
	[
		"greenzonesettings.txt",
		"laglog",
		"markers.txt",
		"clientsettings.json",
		"session.txt",
		"greenzone"
	];

	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc
		};

		var archive = await file.OpenZipArchiveRead();

		foreach (var entry in InvalidArchiveEntries)
		{
			if (archive.HasEntry(entry))
			{
				return Error($"Invalid {FileExtension}, cannot contain a {entry} file");
			}
		}

		// guard against branch header files, which have a number in their name
		var headerEntry = archive.Entries.SingleOrDefault(
			e => e.Key.StartsWith(HeaderFile, StringComparison.InvariantCultureIgnoreCase) && !e.Key.Any(char.IsDigit));
		if (headerEntry is null)
		{
			return Error($"Missing {HeaderFile}, can not parse");
		}

		long? vsyncAttoseconds;
		int? vBlankCount;
		string clockRate;
		string core;

		await using (var stream = headerEntry.OpenEntryStream())
		{
			using var reader = new StreamReader(stream);
			var header = (await reader
				.ReadToEndAsync())
				.LineSplit();

			string platform = header.GetValueFor(Keys.Platform);
			if (string.IsNullOrWhiteSpace(platform))
			{
				return Error("Could not determine the System Code");
			}

			string romHash = header.GetValueFor("SHA1");
			if (string.IsNullOrEmpty(romHash))
			{
				romHash = header.GetValueFor("MD5");
			}

			HashType? hashType = romHash.Length switch
			{
				2 * SHA1.HashSizeInBytes => HashType.Sha1,
				2 * MD5.HashSizeInBytes => HashType.Md5,
				8/* 2 * Crc32.HashLengthInBytes w/ System.IO.Hashing */ => HashType.Crc32,
				_ => null
			};
			if (hashType is not null)
			{
				result.Hashes[hashType.Value] = romHash.ToLower();
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

			// Some biz system ids do not match tasvideos, convert if needed
			if (BizToTasvideosSystemIds.TryGetValue(platform, out var systemId))
			{
				platform = systemId;
			}

			// Check various subsystem flags
			if (header.GetBoolFor(Keys.Mode32X))
			{
				platform = SystemCodes.X32;
			}
			else if (header.GetBoolFor(Keys.ModeCgb))
			{
				platform = SystemCodes.Gbc;
			}
			else if (header.GetValueFor(Keys.Board) == SystemCodes.Fds)
			{
				platform = SystemCodes.Fds;
			}
			else if (header.GetBoolFor(Keys.ModeVs))
			{
				platform = SystemCodes.Arcade;
				result.FrameRateOverride = NtscNesFramerate;
			}
			else if (header.GetBoolFor(Keys.ModeStv))
			{
				platform = SystemCodes.Arcade;
				result.FrameRateOverride = NtscSatFramerate;
			}
			else if (header.GetValueFor(Keys.Board) == SystemCodes.Sgb)
			{
				platform = SystemCodes.Sgb;
				result.FrameRateOverride = result.Region == RegionType.Pal
					? PalSnesFramerate
					: NtscSnesFramerate;
			}
			else if (header.GetBoolFor(Keys.ModeSegaCd))
			{
				platform = SystemCodes.SegaCd;
			}
			else if (header.GetBoolFor(Keys.ModeGg))
			{
				platform = SystemCodes.Gg;
			}
			else if (header.GetBoolFor(Keys.ModeSg))
			{
				platform = SystemCodes.Sg;
			}
			else if (header.GetBoolFor(Keys.ModeDsi))
			{
				platform = SystemCodes.Dsi;
			}
			else if (header.GetBoolFor(Keys.ModeDd))
			{
				platform = SystemCodes.N64Dd;
			}
			else if (header.GetBoolFor(Keys.ModeJaguarCd))
			{
				platform = SystemCodes.JaguarCd;
			}

			result.SystemCode = platform;

			if (header.GetBoolFor(Keys.StartsFromSavestate))
			{
				result.StartType = MovieStartType.Savestate;
			}
			else if (header.GetBoolFor(Keys.StartsFromSram))
			{
				result.StartType = MovieStartType.Sram;
			}

			vsyncAttoseconds = header.GetPositiveLongFor(Keys.VsyncAttoseconds);
			vBlankCount = header.GetPositiveIntFor(Keys.VBlankCount);
			result.CycleCount = header.GetPositiveLongFor(Keys.CycleCount);
			clockRate = header.GetValueFor(Keys.ClockRate).Replace(',', '.');
			core = header.GetValueFor(Keys.Core).ToLower();
		}

		var inputLog = archive.Entry(InputFile);
		if (inputLog is null)
		{
			return Error($"Missing {InputFile}, can not parse");
		}

		await using var inputStream = inputLog.OpenEntryStream();
		(_, result.Frames) = await inputStream.PipeBasedMovieHeaderAndFrameCount();

		var commentEntry = archive.Entries.SingleOrDefault(e => e.Key.StartsWith(CommentFile, StringComparison.InvariantCultureIgnoreCase));
		if (commentEntry is not null)
		{
			await using var commentStream = commentEntry.OpenEntryStream();
			using var reader = new StreamReader(commentStream);
			var annotations = await reader.ReadToEndAsync();
			if (!string.IsNullOrWhiteSpace(annotations))
			{
				result.Annotations = annotations;
			}
		}

		// MapParsedResult() implies we only ever have a list of framerates for cores with framerate overrides, but it doesn't distinguish by core. nymashock has cycle count but octoshock has to rely on mednafen framerates for now. so we override with a constant for octoshock, to prevent picking random wrong values from nymashock overrides
		if (core == "octoshock")
		{
			result.FrameRateOverride = result.Region == RegionType.Pal
				? PalPsxFramerate
				: NtscPsxFramerate;
		}

		if (result.CycleCount.HasValue)
		{
			if (ValidClockRates.Contains(clockRate))
			{
				var seconds = result.CycleCount.Value / double.Parse(clockRate, CultureInfo.InvariantCulture);
				result.FrameRateOverride = result.Frames / seconds;
			}
			else if (CycleBasedCores.TryGetValue(core, out int cyclesPerFrame))
			{
				var seconds = result.CycleCount.Value / (double)cyclesPerFrame;
				result.FrameRateOverride = result.Frames / seconds;
			}
			else
			{
				return Error($"Missing or invalid {Keys.ClockRate}, could not parse movie time (is {nameof(ValidClockRates)} up-to-date?)");
			}
		}
		else if (core == "subneshawk")
		{
			if (!vBlankCount.HasValue)
			{
				return Error($"Missing {Keys.VBlankCount}, could not parse movie time");
			}

			result.Frames = vBlankCount.Value;
		}
		else if (core == "mame")
		{
			if (!vsyncAttoseconds.HasValue)
			{
				return Error($"Missing {Keys.VsyncAttoseconds}, could not parse movie time");
			}

			const decimal attosecondsInSecond = 1000000000000000000;
			result.FrameRateOverride = (double)(attosecondsInSecond / vsyncAttoseconds.Value);
		}

		return result;
	}

	// before 2.8, clock rate had to be determined by the core used
	// only SubGbHawk and gambatte used cycle based time at this time
	private static readonly Dictionary<string, int> CycleBasedCores = new()
	{
		["subgbhawk"] = 4194304,
		["gambatte"] = 2097152,
	};

	private static readonly IReadOnlyList<string> ValidClockRates =
	[
		"4194304", // SubGbHawk
		"2097152", // gambatte, SameBoy
		"5369318.18181818", // SubNesHawk (NTSC)
		"5320342.5", // SubNesHawk (PAL/Dendy)
		"33868800", // NymaShock,
		"1000", // DOSBox-x
	];

	private static readonly Dictionary<string, string> BizToTasvideosSystemIds = new()
	{
		["gen"] = SystemCodes.Genesis,
		["sat"] = SystemCodes.Saturn,
		["dgb"] = SystemCodes.GameBoy,
		["gb3x"] = SystemCodes.GameBoy,
		["gb4x"] = SystemCodes.GameBoy,
		["gbl"] = SystemCodes.GameBoy,
		["gbal"] = SystemCodes.Gba,
		["a26"] = SystemCodes.Atari2600,
		["a78"] = SystemCodes.Atari7800,
		["uze"] = SystemCodes.UzeBox,
		["vb"] = SystemCodes.VirtualBoy,
		["zxspectrum"] = SystemCodes.ZxSpectrum,
		["nds"] = SystemCodes.Ds
	};

	private static class Keys
	{
		public const string RerecordCount = "rerecordcount";
		public const string Platform = "platform";
		public const string Board = "boardname";
		public const string Pal = "pal";
		public const string StartsFromSram = "startsfromsaveram";
		public const string StartsFromSavestate = "startsfromsavestate";
		public const string Mode32X = "is32x";
		public const string ModeCgb = "iscgbmode";
		public const string ModeDd = "isdd";
		public const string ModeDsi = "isdsi";
		public const string ModeJaguarCd = "isjaguarcd";
		public const string ModeSegaCd = "issegacdmode";
		public const string ModeGg = "isggmode";
		public const string ModeSg = "issgmode";
		public const string ModeStv = "isstv";
		public const string ModeVs = "isvs";
		public const string VBlankCount = "vblankcount";
		public const string CycleCount = "cyclecount";
		public const string ClockRate = "clockrate";
		public const string VsyncAttoseconds = "vsyncattoseconds";
		public const string Core = "core";
	}
}
