using SharpCompress.Archives.Tar;

namespace TASVideos.MovieParsers.Parsers;

[FileExtension("dft")]
internal class Dft : Parser, IParser
{
	private const double FrameRate = 60.0;

	public async Task<IParseResult> Parse(Stream file, long length)
	{
		var result = new SuccessResult(FileExtension)
		{
			Region = RegionType.Ntsc,
			SystemCode = SystemCodes.Windows,
			FrameRateOverride = FrameRate,
		};

		using var archive = await file.OpenTarGzArchiveRead();
		if (archive == null)
		{
			return InvalidFormat();
		}

		var pendingInputFiles = new Queue<string>();
		var parsedInputFiles = new Dictionary<string, ParsedInputFile>();

		// The first file to look for is main.txt, which includes all other files
		pendingInputFiles.Enqueue("main.txt");

		while (pendingInputFiles.TryDequeue(out var inputFilePath))
		{
			var inputFileEntry = archive.Entries.SingleOrDefault(
				e => e.Key?.EndsWith(inputFilePath, StringComparison.InvariantCulture) == true);
			if (inputFileEntry is null)
			{
				return inputFilePath == "main.txt"
					? InvalidFormat()
					: Error($"Missing included file {inputFilePath}, cannot parse");
			}

			ParsedInputFile parsedInputFile;
			try
			{
				parsedInputFile = await ParseInputFile(inputFileEntry);
			}
			catch (OverflowException)
			{
				return Error($"Overflow occurred when parsing {inputFilePath}");
			}

			parsedInputFiles.Add(inputFilePath, parsedInputFile);

			// If a pending include hasn't been parsed yet, queue it up for parsing
			foreach (var includedFile in parsedInputFile.PendingIncludes.Keys)
			{
				if (parsedInputFiles.ContainsKey(includedFile) || pendingInputFiles.Contains(includedFile))
				{
					continue;
				}

				pendingInputFiles.Enqueue(includedFile);
			}
		}

		// Iterate through all the files with pending includes
		// If a file doesn't have any pending includes, it can be ignored here
		var pendingParsedInputFiles = parsedInputFiles.Values
			.Where(parsedInputFile => parsedInputFile.PendingIncludes.Count > 0)
			.ToList();

		while (pendingParsedInputFiles.Count > 0)
		{
			var removedSomething = false;
			var finishedParsedInputFiles = new List<ParsedInputFile>();
			foreach (var pendingParsedInputFile in pendingParsedInputFiles)
			{
				var includedPathsToRemove = new List<string>();
				foreach (var (includedInputFilePath, numIncludes) in pendingParsedInputFile.PendingIncludes)
				{
					var includedInputFile = parsedInputFiles[includedInputFilePath];

					// If the included file still has pending includes, don't count it yet
					if (includedInputFile.PendingIncludes.Count > 0)
					{
						continue;
					}

					try
					{
						checked
						{
							pendingParsedInputFile.FrameCount += includedInputFile.FrameCount * numIncludes;
						}
					}
					catch (OverflowException)
					{
						var pendingParsedInputFilePath = parsedInputFiles
							.Single(kvp => kvp.Value == pendingParsedInputFile).Key;
						return Error($"Frame count overflow occurred for {pendingParsedInputFilePath}");
					}

					includedPathsToRemove.Add(includedInputFilePath);
					removedSomething = true;
				}

				foreach (var includedPath in includedPathsToRemove)
				{
					pendingParsedInputFile.PendingIncludes.Remove(includedPath);
				}

				if (pendingParsedInputFile.PendingIncludes.Count == 0)
				{
					finishedParsedInputFiles.Add(pendingParsedInputFile);
					removedSomething = true;
				}
			}

			foreach (var finishedParsedInputFile in finishedParsedInputFiles)
			{
				pendingParsedInputFiles.Remove(finishedParsedInputFile);
			}

			// If nothing got removed, we're stuck in recursion!
			if (!removedSomething)
			{
				return Error("Recursive includes detected, cannot parse");
			}
		}

		result.Frames = parsedInputFiles["main.txt"].FrameCount;

		return await Task.FromResult(result);
	}

	private class ParsedInputFile
	{
		public int FrameCount;
		public readonly Dictionary<string, int> PendingIncludes = [];
	}

	/// <summary>
	/// Parses an input file within a .dft file
	/// </summary>
	/// <param name="archiveEntry">The input file, as a tar archive entry</param>
	/// <exception cref="OverflowException">Thrown if more than <see cref="int.MaxValue"/> frames or includes are present</exception>
	/// <returns>Parsed input file results</returns>
	private static async Task<ParsedInputFile> ParseInputFile(TarArchiveEntry archiveEntry)
	{
		var parsedInputFile = new ParsedInputFile();
		await using var stream = archiveEntry.OpenEntryStream();
		using var reader = new StreamReader(stream);
		while (await reader.ReadLineAsync() is { } line)
		{
			// # indicates this line is a comment
			if (line.StartsWith('#'))
			{
				continue;
			}

			// MOUSE indicates this line is a mouse frame
			// Mouse frames do not advance the frame count
			if (line.StartsWith("MOUSE", StringComparison.InvariantCulture))
			{
				continue;
			}

			// If an include is present, those inputs get included into this file
			// Includes are deferred until later (so all includes are known)
			if (line.StartsWith("INCLUDE:", StringComparison.InvariantCulture))
			{
				var includedFile = line[8..];
				if (!parsedInputFile.PendingIncludes.TryAdd(includedFile, 1))
				{
					checked
					{
						parsedInputFile.PendingIncludes[includedFile]++;
					}
				}

				continue;
			}

			// Otherwise, this is an input frame
			checked
			{
				parsedInputFile.FrameCount++;
			}
		}

		return parsedInputFile;
	}
}
