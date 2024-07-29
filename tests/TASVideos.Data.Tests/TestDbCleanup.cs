using TASVideos.Tests.Base;

namespace TASVideos.Data.Tests;
public class TestDbCleanup : TestDbBase
{
	[AssemblyCleanup]
	public static new void AssemblyCleanup() => TestDbBase.AssemblyCleanup();
}
