using System.Text;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("lmp")]
internal class Lmp : Parser, IParser
{
	private delegate bool TryParseLmp(byte[] movie, ref int frames);
	private const int MAXPLAYERS = 4;
	private const int TERMINATOR = 0x80;
	private const int INVALID = -1;
	private int FooterPointer { get; set; } = INVALID;

	// order is important here to minimize false detections
	// especially the last 3, which are impossible to always detect correctly
	private TryParseLmp[] LmpParsers =>
	[
		TryParseDoomClassic,
		TryParseStrife,
		TryParseNewDoom,
		TryParseOldHexen,
		TryParseNewHexen,
		TryParseHeretic,
		TryParseOldDoom,
	];

	private static bool CheckSizeSanity(int len, int headerLen, int inputLen)
	{
		// header size + 1 single-player input frame + terminator byte
		if (len < headerLen + inputLen + 1)
		{
			return false;
		}

		return true;
	}

	private int CalcFrames(byte[] movie, int headerLen, int inputLen, int playerCount)
	{
		var frameCount = 0;
		FooterPointer = INVALID;

		for (var pointer = headerLen; pointer < movie.Length; pointer += inputLen * playerCount)
		{
			if (movie[pointer] == TERMINATOR)
			{
				if (pointer + 1 < movie.Length)
				{
					FooterPointer = pointer + 1;
				}

				return frameCount;
			}

			frameCount++;
		}

		return INVALID;
	}

	private bool TryParseOldDoom(byte[] movie, ref int frames)
	{
		// Pre-1.4 Doom has a 7 byte header, and 4 bytes per input
		if (!CheckSizeSanity(movie.Length, 7, 4))
		{
			return false;
		}

		var players = 0;
		for (int i = 0; i < MAXPLAYERS; i++)
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
			frames = CalcFrames(movie, 7, 4, players);
			return frames > 0;
		}

		return false;
	}

	private bool TryParseNewDoom(byte[] movie, ref int frames)
	{
		// Regular Doom and Doom II has a 13 byte header, and 4 bytes per input
		if (!CheckSizeSanity(movie.Length, 13, 4))
		{
			return false;
		}

		if (movie[0] < 104 || movie[0] > 110) // version
		{
			return false;
		}

		var players = 0;
		for (int i = 0; i < MAXPLAYERS; i++)
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

		if (players == 0)
		{
			return false;
		}

		frames = CalcFrames(movie, 13, 4, players);
		return frames > 0;
	}

	private bool TryParseDoomClassic(byte[] movie, ref int frames)
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

			frames = CalcFrames(movie, 14 + (84 * players), 4, players);
			return frames > 0;
		}

		return false;
	}

	private bool TryParseHeretic(byte[] movie, ref int frames)
	{
		// Heretic has a 7 byte header, and 6 bytes per input
		if (!CheckSizeSanity(movie.Length, 7, 6))
		{
			return false;
		}

		var players = 0;
		for (int i = 0; i < MAXPLAYERS; i++)
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
			frames = CalcFrames(movie, 7, 6, players);
			return frames > 0;
		}

		return false;
	}

	private bool TryParseOldHexen(byte[] movie, ref int frames)
	{
		// Hexen demo and Hexen 1.0 has an 11 byte header, and 6 bytes per input
		if (!CheckSizeSanity(movie.Length, 11, 6))
		{
			return false;
		}

		var players = 0;
		for (int i = 0; i < MAXPLAYERS; i++)
		{
			if (movie[3 + (i * 2)] == 1)
			{
				players++;
			}

			if (movie[3 + (i * 2)] is not (0 or 1) || movie[3 + (i * 2) + 1] > 2) // invalid values
			{
				return false;
			}
		}

		if (players > 0)
		{
			frames = CalcFrames(movie, 11, 6, players);
			return frames > 0;
		}

		return false;
	}

	private bool TryParseNewHexen(byte[] movie, ref int frames)
	{
		// Hexen 1.1 has a 19 byte header, and 6 bytes per input
		if (!CheckSizeSanity(movie.Length, 19, 6))
		{
			return false;
		}

		var players = 0;
		for (int i = 0; i < MAXPLAYERS * 2; i++)
		{
			if (movie[3 + (i * 2)] == 1)
			{
				players++;
			}

			if (movie[3 + (i * 2)] is not (0 or 1) || movie[3 + (i * 2) + 1] > 2) // invalid values
			{
				return false;
			}
		}

		if (players > 0)
		{
			frames = CalcFrames(movie, 19, 6, players);
			return frames > 0;
		}

		return false;
	}

	private bool TryParseStrife(byte[] movie, ref int frames)
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
		for (int i = 0; i < MAXPLAYERS * 2; i++)
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
			frames = CalcFrames(movie, 16, 6, players);
			return frames > 0;
		}

		return false;
	}

	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc,
			SystemCode = SystemCodes.Doom
		};

		/* A lmp consists of a header, inputs, and a terminator byte
		 * the size of the header and each input depends on the game used
		 * the terminator byte is always 0x80 (note: source ports might have a footer after)
		 * a bit of heuristics is needed here to detect the variant used
		 * as the header doesn't give an easy answer to the variant used
		 */

		using var br = new BinaryReader(file);
		var movie = br.ReadBytes((int)length);

		int frames = INVALID;
		foreach (var tryParseLmp in LmpParsers)
		{
			if (tryParseLmp(movie, ref frames))
			{
				break;
			}
		}

		if (frames <= 0)
		{
			return InvalidFormat();
		}

		result.Frames = frames;

		if (FooterPointer != INVALID)
		{
			result.Annotations = Encoding.UTF8.GetString(movie.AsSpan(FooterPointer).ToArray())
				.Replace('\0', ' ');
		}

		return await Task.FromResult(result);
	}
}
