using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests;
public class TestDbCleanup : TestDbBase
{
	[AssemblyCleanup]
	public static new void AssemblyCleanup() => TestDbBase.AssemblyCleanup();
}
