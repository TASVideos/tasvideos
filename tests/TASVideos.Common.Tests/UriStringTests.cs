namespace TASVideos.Common.Tests;

[TestClass]
public sealed class UriStringTests
{
	[TestMethod]
#if false // throws
	[DataRow(false, ""/*empty string*/)]
#endif
	[DataRow(false, "#fragment")]
#if false // misclassified as external (probably a good idea, though the UI should prepend `//` before these get anywhere near the DB or SSR)
	[DataRow(false, "resource.txt")]
	[DataRow(false, "./resource.txt")]
	[DataRow(false, "../resource.txt")]
	[DataRow(false, "path/resource.txt")]
#endif
	[DataRow(false, "/path/resource.txt")]
#if false // misclassified as external, see method's docs
	[DataRow(false, "https://tasvideos.org/Forum")]
#endif
	[DataRow(true, "//example.com/path/resource.txt")]
	[DataRow(true, "https://example.com/path/resource.txt#fragment")] // yes I pulled this list straight from the English Wikipedia --yoshi
	public void TestIsToExternalDomain(bool expected, string uri)
		=> Assert.AreEqual(expected: expected, actual: UriString.IsToExternalDomain(uri));
}
