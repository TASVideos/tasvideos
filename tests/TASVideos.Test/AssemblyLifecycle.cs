using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests;

[TestClass]
public static class AssemblyLifecycle
{
	[AssemblyInitialize]
	public static void AssemblyInit(TestContext context)
	{
		TestDbBase.AssemblyInit(context);
	}

	[AssemblyCleanup]
	public static void AssemblyCleanup()
	{
		TestDbBase.AssemblyCleanup();
	}
}
