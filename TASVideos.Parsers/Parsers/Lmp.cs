using TASVideos.MovieParsers.Result;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("lmp")]
internal class Lmp : ParserBase, IParser
{
	public override string FileExtension => "lmp";

	private delegate bool TryParseLmp(byte[] movie, ref int frames);

	private static readonly TryParseLmp[] _lmpParsers = new TryParseLmp[]
	{
		// order is important here to minimize false detections
		// especially the last 3, which are impossible to always detect correctly
		TryParseDoomClassic,
		TryParseStrife,
		TryParseNewDoom,
		TryParseOldHexen,
		TryParseNewHexen,
		TryParseHeretic,
		TryParseOldDoom,
	};

	private static bool CheckSizeSanity(int len, int headerLen, int inputLen)
	{
		if (len < headerLen + 1)
		{
			return false;
		}

		if ((len - headerLen - 1) % inputLen != 0)
		{
			return false;
		}

		return true;
	}

	private static int CalcFrames(int len, int headerLen, int inputLen, int playerCount)
	{
		return (int)Math.Ceiling((len - headerLen - 1) / (double)(inputLen * playerCount));
	}

	private static bool TryParseOldDoom(byte[] movie, ref int frames)
	{
		// "Old" Doom has a 7 byte header, and 4 bytes per input
		if (!CheckSizeSanity(movie.Length, 7, 4))
		{
			return false;
		}

		var players = 0;
		for (int i = 0; i < 4; i++)
		{
			if (movie[3 + i] == 1)
			{
				players++;
			}
			else if (movie[3 + i] != 0) // invalid value
			{
				return false;
			}
		}

		if (players > 0)
		{
			frames = CalcFrames(movie.Length, 7, 4, players);
			return true;
		}

		return false;
	}

	private static bool TryParseNewDoom(byte[] movie, ref int frames)
	{
		// "New" Doom and Doom II has a 13 byte header, and 4 bytes per input
		if (!CheckSizeSanity(movie.Length, 13, 4))
		{
			return false;
		}

		if (movie[0] < 104 || movie[0] > 110) // version
		{
			return false;
		}

		var players = 0;
		for (int i = 0; i < 4; i++)
		{
			if (movie[9 + i] == 1)
			{
				players++;
			}
			else if (movie[9 + i] != 0) // invalid value
			{
				return false;
			}
		}

		if (players > 0)
		{
			frames = CalcFrames(movie.Length, 13, 4, players);
			return true;
		}

		return false;
	}

	private static bool TryParseDoomClassic(byte[] movie, ref int frames)
	{
		// Doom Classic has a 14 + 84 * player count byte header, and 4 bytes per input
		if (!CheckSizeSanity(movie.Length, 14, 4))
		{
			return false;
		}

		if (movie[0] != 111) // version
		{
			return false;
		}

		var players = 0;
		for (int i = 0; i < 4; i++)
		{
			if (movie[10 + i] == 1)
			{
				players++;
			}
			else if (movie[10 + i] != 0) // invalid value
			{
				return false;
			}
		}

		if (players > 0)
		{
			if (movie.Length < 14 + (84 * players) + 1)
			{
				return false;
			}

			frames = CalcFrames(movie.Length, 14 + (84 * players), 4, players);
			return true;
		}

		return false;
	}

	private static bool TryParseHeretic(byte[] movie, ref int frames)
	{
		// Heretic has a 7 byte header, and 6 bytes per input
		if (!CheckSizeSanity(movie.Length, 7, 6))
		{
			return false;
		}

		var players = 0;
		for (int i = 0; i < 4; i++)
		{
			if (movie[3 + i] == 1)
			{
				players++;
			}
			else if (movie[3 + i] != 0) // invalid value
			{
				return false;
			}
		}

		if (players > 0)
		{
			frames = CalcFrames(movie.Length, 7, 6, players);
			return true;
		}

		return false;
	}

	private static bool TryParseOldHexen(byte[] movie, ref int frames)
	{
		// Hexen demo and Hexen 1.0 has a 11 byte header, and 6 bytes per input
		if (!CheckSizeSanity(movie.Length, 11, 6))
		{
			return false;
		}

		var players = 0;
		for (int i = 0; i < 4; i++)
		{
			if (movie[3 + (i * 2)] == 1)
			{
				players++;
			}

			if (movie[3 + (i * 2)] is not 0 or 1 || movie[3 + (i * 2) + 1] > 2) // invalid values
			{
				return false;
			}
		}

		if (players > 0)
		{
			frames = CalcFrames(movie.Length, 11, 6, players);
			return true;
		}

		return false;
	}

	private static bool TryParseNewHexen(byte[] movie, ref int frames)
	{
		// Hexen 1.1 has a 19 byte header, and 6 bytes per input
		if (!CheckSizeSanity(movie.Length, 19, 6))
		{
			return false;
		}

		var players = 0;
		for (int i = 0; i < 8; i++)
		{
			if (movie[3 + (i * 2)] == 1)
			{
				players++;
			}

			if (movie[3 + (i * 2)] is not 0 or 1 || movie[3 + (i * 2) + 1] > 2) // invalid values
			{
				return false;
			}
		}

		if (players > 0)
		{
			frames = CalcFrames(movie.Length, 19, 6, players);
			return true;
		}

		return false;
	}

	private static bool TryParseStrife(byte[] movie, ref int frames)
	{
		// Strife has a 16 byte header, and 6 bytes per input
		if (!CheckSizeSanity(movie.Length, 16, 6))
		{
			return false;
		}

		if (movie[0] != 101) // version
		{
			return false;
		}

		var players = 0;
		for (int i = 0; i < 8; i++)
		{
			if (movie[8 + i] == 1)
			{
				players++;
			}
			else if (movie[8 + i] != 0) // invalid value
			{
				return false;
			}
		}

		if (players > 0)
		{
			frames = CalcFrames(movie.Length, 16, 6, players);
			return true;
		}

		return false;
	}

	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new ParseResult
		{
			Region = RegionType.Ntsc,
			FileExtension = FileExtension,
			SystemCode = SystemCodes.Doom
		};

		/* A lmp consists of a header, inputs, and a terminator byte
		 * the size of the header and each input depends on the game used
		 * the terminator byte is always 0x80 (note: source ports might have a footer after)
		 * a bit of heuristics are needed here to detect the variant used
		 * as the header doesn't give an easy answer to the variant used
		 */

		var movie = new byte[length];
		file.Read(movie, 0, (int)length);

		if (movie[length - 1] != 0x80) // fixme: this might be ok if there is a source port footer (not easy to detect however)
		{
			return new ErrorResult("Invalid file format, does not seem to be a .lmp");
		}

		int frames = -1;
		foreach (var tryParseLmp in _lmpParsers)
		{
			if (tryParseLmp(movie, ref frames))
			{
				break;
			}
		}

		if (frames < 0)
		{
			return new ErrorResult("Invalid file format, does not seem to be a .lmp");
		}

		result.Frames = frames;

		return await Task.FromResult(result);
	}
}
