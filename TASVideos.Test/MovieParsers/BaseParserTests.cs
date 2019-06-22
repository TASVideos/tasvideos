using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace TASVideos.Test.MovieParsers
{
	public abstract class BaseParserTests
	{
		public abstract string ResourcesPath { get; }

		protected Stream Embedded(string name)
		{
			return Assembly.GetAssembly(typeof(Bk2ParserTests)).GetManifestResourceStream(ResourcesPath + name);
		}
	}
}
