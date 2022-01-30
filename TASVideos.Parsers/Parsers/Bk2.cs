using TASVideos.MovieParsers.Extensions;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("bk2")]
internal class Bk2 : ParserBase, IParser
{
	private const string HeaderFile = "header";
	private const string InputFile = "input log";

	public override string FileExtension => "bk2";

	public async Task<IParseResult> Parse(Stream file)
	{
		var result = new ParseResult
		{
			Region = RegionType.Ntsc,
			FileExtension = FileExtension
		};

		var archive = new ZipArchive(file);

		var headerEntry = archive.Entry(HeaderFile);
		if (headerEntry == null)
		{
			return Error($"Missing {HeaderFile}, can not parse");
		}

		int? vBlankCount;
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
			else if (header.GetValueFor(Keys.ModeVs).ToBool())
			{
				platform = SystemCodes.Arcade;
				result.FrameRateOverride = 60.0988138974405;
			}

			result.SystemCode = platform;

			if (header.GetValueFor(Keys.Pal).ToBool())
			{
				result.Region = RegionType.Pal;
			}

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
			core = header.GetValueFor(Keys.Core).ToLower();
		}

		var inputLog = archive.Entry(InputFile);
		if (inputLog == null)
		{
			return Error($"Missing {InputFile}, can not parse");
		}

		await using var inputStream = inputLog.Open();
		using var inputReader = new StreamReader(inputStream);
		result.Frames = (await inputReader.ReadToEndAsync())
			.LineSplit()
			.PipeCount();

		if (result.CycleCount.HasValue && CycleBasedCores.TryGetValue(core, out int cyclesPerFrame))
		{
			var seconds = result.CycleCount.Value / (double)cyclesPerFrame;
			result.FrameRateOverride = result.Frames / seconds;
		}

		if (core == "subneshawk")
		{
			if (!vBlankCount.HasValue)
			{
				return Error($"Missing {Keys.VBlankCount}, could not parse movie time");
			}

			result.Frames = vBlankCount.Value;
		}

		return result;
	}

	private static readonly Dictionary<string, int> CycleBasedCores = new()
	{
		["subgbhawk"] = 4194304,
		["gambatte"] = 2097152
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
		public const string ModeSegaCd = "issegacdmode";
		public const string ModeGg = "isggmode";
		public const string ModeSg = "issgmode";
		public const string ModeVs = "isvs";
		public const string VBlankCount = "vblankcount";
		public const string CycleCount = "cyclecount";
		public const string Core = "core";
	}
}
