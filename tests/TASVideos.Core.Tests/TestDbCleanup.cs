namespace TASVideos.Core.Tests;

public class TestDbCleanup : TestDbBase
{
	[AssemblyCleanup]
	public static new void AssemblyCleanup() => TestDbBase.AssemblyCleanup();
}
