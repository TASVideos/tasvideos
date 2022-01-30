using System.Reflection;

namespace TASVideos.MovieParsers.Tests;

public abstract class BaseParserTests
{
	public abstract string ResourcesPath { get; }

	protected Stream Embedded(string name)
	{
		var stream = Assembly.GetAssembly(typeof(BaseParserTests))?.GetManifestResourceStream(ResourcesPath + name);
		if (stream == null)
		{
			throw new InvalidOperationException($"Unable to find embedded resource {name}");
		}

		return stream;
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
