using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TASVideos.MovieParsers.Result;

namespace TASVideos.Test.MovieParsers
{
	public abstract class BaseParserTests
	{
		public abstract string ResourcesPath { get; }

		protected Stream Embedded(string name)
		{
			return Assembly.GetAssembly(typeof(BaseParserTests)).GetManifestResourceStream(ResourcesPath + name);
		}

		protected void AssertNoWarnings(IParseResult result)
		{
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Warnings);
			Assert.AreEqual(0, result.Warnings.Count());
		}

		protected void AssertNoErrors(IParseResult result)
		{
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Errors);
			Assert.AreEqual(0, result.Errors.Count());
		}

		protected void AssertNoWarningsOrErrors(IParseResult result)
		{
			AssertNoWarnings(result);
			AssertNoErrors(result);
		}
	}
}
