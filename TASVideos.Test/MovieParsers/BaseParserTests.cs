using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.MovieParsers.Result;

namespace TASVideos.Test.MovieParsers
{
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

		protected void AssertNoWarnings(IParseResult result)
		{
			Assert.IsNotNull(result.Warnings);
			Assert.AreEqual(0, result.Warnings.Count());
		}

		protected void AssertNoErrors(IParseResult result)
		{
			Assert.IsNotNull(result.Errors);
			Assert.AreEqual(0, result.Errors.Count());
		}

		protected void AssertNoWarningsOrErrors(IParseResult result)
		{
			AssertNoWarnings(result);
			AssertNoErrors(result);
		}

		protected bool FrameRatesAreEqual(double expected, double actual)
		{
			return Math.Abs(expected - actual) < 0.0001;
		}
	}
}
