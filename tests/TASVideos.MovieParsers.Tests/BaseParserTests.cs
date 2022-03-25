using System.IO.Compression;
using System.Reflection;

namespace TASVideos.MovieParsers.Tests;

public abstract class BaseParserTests
{
	public abstract string ResourcesPath { get; }

	protected Stream Embedded(string name)
	{
		var stream = Assembly.GetAssembly(typeof(BaseParserTests))?.GetManifestResourceStream(ResourcesPath + name);
		if (stream is null)
		{
			throw new InvalidOperationException($"Unable to find embedded resource {name}");
		}

		return MakeTestStream(stream);
	}

	protected long EmbeddedLength(string name)
	{
		var stream = Assembly.GetAssembly(typeof(BaseParserTests))?.GetManifestResourceStream(ResourcesPath + name);
		if (stream is null)
		{
			throw new InvalidOperationException($"Unable to find embedded resource {name}");
		}

		return stream.Length;
	}

	private static Stream MakeTestStream(Stream input)
	{
		// Simulates real situations, as the `stream` passed to parse
		// in the real site will always be from within a zip file.

		var ms = new MemoryStream();

		using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
		{
			var entry = zip.CreateEntry("foobar");
			using var dest = entry.Open();
			input.CopyTo(dest);
		}

		ms.Position = 0;

		var zip2 = new ZipArchive(ms);
		var movieFile = zip2.Entries[0];
		var movieFileStream = movieFile.Open();
		return movieFileStream;
	}

	protected static void AssertNoWarnings(IParseResult result)
	{
		Assert.IsNotNull(result.Warnings);
		Assert.AreEqual(0, result.Warnings.Count());
	}

	protected static void AssertNoErrors(IParseResult result)
	{
		Assert.IsNotNull(result.Errors);
		Assert.AreEqual(0, result.Errors.Count());
	}

	protected static void AssertNoWarningsOrErrors(IParseResult result)
	{
		AssertNoWarnings(result);
		AssertNoErrors(result);
	}

	protected static bool FrameRatesAreEqual(double expected, double actual)
	{
		return Math.Abs(expected - actual) < 0.0001;
	}
}
