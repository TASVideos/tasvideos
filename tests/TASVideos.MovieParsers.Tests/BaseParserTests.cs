using System.Reflection;
using SharpZipArchive = SharpCompress.Archives.Zip.ZipArchive;

namespace TASVideos.MovieParsers.Tests;

public abstract class BaseParserTests
{
	protected abstract string ResourcesPath { get; }

	protected Stream Embedded(string name)
	{
		var stream = Assembly.GetAssembly(typeof(BaseParserTests))?.GetManifestResourceStream(ResourcesPath + name);
		return stream is null
			? throw new InvalidOperationException($"Unable to find embedded resource {name}")
			: MakeTestStream(stream);
	}

	protected long EmbeddedLength(string name)
	{
		var stream = Assembly.GetAssembly(typeof(BaseParserTests))?.GetManifestResourceStream(ResourcesPath + name);
		return stream?.Length ?? throw new InvalidOperationException($"Unable to find embedded resource {name}");
	}

	private static Stream MakeTestStream(Stream input)
	{
		// Simulates real situations, as the `stream` passed to parse
		// in the real site will always be from within a zip file.
		var ms = new MemoryStream();

		using (var zip = SharpZipArchive.Create())
		{
			zip.AddEntry("foobar", input, input.Length);
			zip.SaveTo(ms);
		}

		ms.Position = 0;

		var zip2 = SharpZipArchive.Open(ms);
		var movieFile = zip2.Entries.First();
		var movieFileStream = movieFile.OpenEntryStream();
		return movieFileStream;
	}

	protected static void AssertNoWarnings(IParseResult result)
	{
		Assert.AreEqual(0, result.Warnings.Count());
	}

	protected static void AssertNoErrors(IParseResult result)
	{
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
