using TASVideos.MovieParsers.Extensions;
using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("lsmv")]
internal class Lsmv : ParserBase, IParser
{
	private const string InputFile = "input";
	private const string RerecordFile = "rerecords";
	private const string GameType = "gametype";
	private const string Savestate = "savestate";
	private const string SavestateAnchor = "savestate.anchor";
	private const string Sram = "moviesram";

	public override string FileExtension => "lsmv";

	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new ParseResult
		{
			Region = RegionType.Ntsc,
			FileExtension = FileExtension
		};

		var archive = new ZipArchive(file);

		// a .lsmv is actually a savestate if a savestate file is present
		if (archive.Entries.Any(e => e.Name.ToLower() == Savestate))
		{
			return Error("This is a savestate file, not a movie file");
		}

		if (archive.Entry(SavestateAnchor) is not null)
		{
			result.StartType = MovieStartType.Savestate;
		}
		else if (archive.Entry(Sram)?.Length > 0)
		{
			result.StartType = MovieStartType.Sram;
		}

		var gameTypeFile = archive.Entry(GameType);
		if (gameTypeFile is null)
		{
			return Error("Could not determine the System Code");
		}

		await using (var stream = gameTypeFile.Open())
		{
			using var reader = new StreamReader(stream);
			var line = (await reader
				.ReadToEndAsync())
				.LineSplit()
				.FirstOrDefault();

			if (line is not null)
			{
				switch (line.ToLower())
				{
					default:
						DefaultGameType(result);
						break;
					case "snes_ntsc":
					case "bsx":
					case "bsxslotted":
					case "sufamiturbo":
						result.SystemCode = SystemCodes.Snes;
						result.Region = RegionType.Ntsc;
						break;
					case "snes_pal":
						result.SystemCode = SystemCodes.Snes;
						result.Region = RegionType.Pal;
						break;
					case "sgb_ntsc":
						result.SystemCode = SystemCodes.Sgb;
						result.Region = RegionType.Ntsc;
						break;
					case "sgb_pal":
						result.SystemCode = SystemCodes.Sgb;
						result.Region = RegionType.Pal;
						break;
					case "gdmg":
						result.SystemCode = SystemCodes.GameBoy;
						result.Region = RegionType.Ntsc;
						break;
					case "ggbc":
					case "ggbca":
						result.SystemCode = SystemCodes.Gbc;
						result.Region = RegionType.Ntsc;
						break;
				}
			}
			else
			{
				DefaultGameType(result);
			}
		}

		var rerecordCountFile = archive.Entry(RerecordFile);
		if (rerecordCountFile is not null)
		{
			await using var stream = rerecordCountFile.Open();
			using var reader = new StreamReader(stream);
			var line = (await reader
				.ReadToEndAsync())
				.LineSplit()
				.FirstOrDefault();

			if (line is null)
			{
				result.WarnNoRerecords();
			}
			else
			{
				var parseResult = int.TryParse(line, out int rerecords);
				if (parseResult)
				{
					result.RerecordCount = rerecords;
				}
				else
				{
					result.WarnNoRerecords();
				}
			}
		}
		else
		{
			result.WarnNoRerecords();
		}

		// guard against extra branch input files, which have a number in their name
		var inputLog = archive.Entries.SingleOrDefault(
			e => e.Name.ToLower().StartsWith(InputFile) && !e.Name.Any(char.IsDigit));
		if (inputLog is null)
		{
			return Error($"Missing {InputFile}, can not parse");
		}

		await using (var stream = inputLog.Open())
		{
			using var reader = new StreamReader(stream);
			result.Frames = (await reader
				.ReadToEndAsync())
				.LineSplit()
				.Count(i => i.StartsWith("F"));
		}

		return result;
	}

	private static void DefaultGameType(ParseResult result)
	{
		result.SystemCode = SystemCodes.Snes;
		result.Region = RegionType.Ntsc;
		result.WarningList.Add(ParseWarnings.SystemIdInferred);
		result.WarningList.Add(ParseWarnings.RegionInferred);
	}
}
