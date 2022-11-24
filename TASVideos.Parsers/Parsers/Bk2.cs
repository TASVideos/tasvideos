﻿using System.Globalization;
using TASVideos.MovieParsers.Extensions;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("bk2")]
internal class Bk2 : ParserBase, IParser
{
	private const string HeaderFile = "header";
	private const string InputFile = "input log";

	// hacky framerate fields, taken from platform framerates
	private const double NtscNesFramerate = 60.0988138974405;
	private const double NtscSnesFramerate = 60.0988138974405;
	private const double PalSnesFramerate = 50.0069789081886;

	protected virtual string[] InvalidArchiveEntries => new[]
	{
		"greenzonesettings.txt",
		"laglog",
		"markers.txt",
		"clientsettings.json",
		"session.txt",
		"greenzone"
	};

	public override string FileExtension => "bk2";

	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new ParseResult
		{
			Region = RegionType.Ntsc,
			FileExtension = FileExtension
		};

		var archive = new ZipArchive(file);

		foreach (var entry in InvalidArchiveEntries)
		{
			if (archive.HasEntry(entry))
			{
				return Error($"Invalid {FileExtension}, cannot contain a {entry} file");
			}
		}

		// guard against branch header files, which have a number in their name
		var headerEntry = archive.Entries.SingleOrDefault(
			e => e.Name.ToLower().StartsWith(HeaderFile) && !e.Name.Any(c => char.IsDigit(c)));
		if (headerEntry is null)
		{
			return Error($"Missing {HeaderFile}, can not parse");
		}

		int? vBlankCount;
		string clockRate;
		string core;

		await using (var stream = headerEntry.Open())
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

			int? rerecordVal = header.GetValueFor(Keys.RerecordCount).ToPositiveInt();
			if (rerecordVal.HasValue)
			{
				result.RerecordCount = rerecordVal.Value;
			}
			else
			{
				result.WarnNoRerecords();
			}

			if (header.GetValueFor(Keys.Pal).ToBool())
			{
				result.Region = RegionType.Pal;
			}

			// Some biz system ids do not match tasvideos, convert if needed
			if (BizToTasvideosSystemIds.ContainsKey(platform))
			{
				platform = BizToTasvideosSystemIds[platform];
			}

			// Check various subsystem flags
			if (header.GetValueFor(Keys.Mode32X).ToBool())
			{
				platform = SystemCodes.X32;
			}
			else if (header.GetValueFor(Keys.ModeCgb).ToBool())
			{
				platform = SystemCodes.Gbc;
			}
			else if (header.GetValueFor(Keys.Board) == SystemCodes.Fds)
			{
				platform = SystemCodes.Fds;
			}
			else if (header.GetValueFor(Keys.ModeVs).ToBool())
			{
				platform = SystemCodes.Arcade;
				result.FrameRateOverride = NtscNesFramerate;
			}
			else if (header.GetValueFor(Keys.Board) == SystemCodes.Sgb)
			{
				platform = SystemCodes.Sgb;
				if (result.Region == RegionType.Pal)
				{
					result.FrameRateOverride = PalSnesFramerate;
				}
				else
				{
					result.FrameRateOverride = NtscSnesFramerate;
				}
			}
			else if (header.GetValueFor(Keys.ModeSegaCd).ToBool())
			{
				platform = SystemCodes.SegaCd;
			}
			else if (header.GetValueFor(Keys.ModeGg).ToBool())
			{
				platform = SystemCodes.Gg;
			}
			else if (header.GetValueFor(Keys.ModeSg).ToBool())
			{
				platform = SystemCodes.Sg;
			}
			else if (header.GetValueFor(Keys.ModeDsi).ToBool())
			{
				platform = SystemCodes.Dsi;
			}

			result.SystemCode = platform;

			if (header.GetValueFor(Keys.StartsFromSavestate).ToBool())
			{
				result.StartType = MovieStartType.Savestate;
			}
			else if (header.GetValueFor(Keys.StartsFromSram).ToBool())
			{
				result.StartType = MovieStartType.Sram;
			}

			vBlankCount = header.GetValueFor(Keys.VBlankCount).ToPositiveInt();
			result.CycleCount = header.GetValueFor(Keys.CycleCount).ToPositiveLong();
			clockRate = header.GetValueFor(Keys.ClockRate).Replace(',', '.');
			core = header.GetValueFor(Keys.Core).ToLower();
		}

		var inputLog = archive.Entry(InputFile);
		if (inputLog is null)
		{
			return Error($"Missing {InputFile}, can not parse");
		}

		await using var inputStream = inputLog.Open();
		(_, result.Frames) = await inputStream.PipeBasedMovieHeaderAndFrameCount();

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

		return result;
	}

	// before 2.8, clock rate had to be determined by the core used
	// only subgbhawk and gambatte used cycle based time at this time
	private static readonly Dictionary<string, int> CycleBasedCores = new()
	{
		["subgbhawk"] = 4194304,
		["gambatte"] = 2097152,
	};

	private static readonly IReadOnlyList<string> ValidClockRates = new[]
	{
		"4194304", // subgbhawk
		"2097152", // gambatte, sameboy
		"5369318.18181818", // subneshawk (NTSC)
		"5320342.5", // subneshawk (PAL/Dendy)
		"33868800", // nymashock
	};

	private static readonly Dictionary<string, string> BizToTasvideosSystemIds = new()
	{
		["gen"] = SystemCodes.Genesis,
		["sat"] = SystemCodes.Saturn,
		["dgb"] = SystemCodes.GameBoy,
		["gb3x"] = SystemCodes.GameBoy,
		["gb4x"] = SystemCodes.GameBoy,
		["gbl"] = SystemCodes.GameBoy,
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
		public const string ModeDsi = "isdsi";
		public const string ModeSegaCd = "issegacdmode";
		public const string ModeGg = "isggmode";
		public const string ModeSg = "issgmode";
		public const string ModeVs = "isvs";
		public const string VBlankCount = "vblankcount";
		public const string CycleCount = "cyclecount";
		public const string ClockRate = "clockrate";
		public const string Core = "core";
	}
}
