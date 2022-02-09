using TASVideos.Data.Entity;
using TASVideos.Extensions;
using static TASVideos.Data.Entity.PermissionTo;

namespace TASVideos.RazorPages.Tests.Extensions;

[TestClass]
public class WikiHelperTests
{
	[TestMethod]
	[DataRow("Test", null, new[] { EditWikiPages }, false)]
	[DataRow("Test", "", new[] { EditWikiPages }, false)]
	[DataRow("Test", " ", new[] { EditWikiPages }, false)]
	[DataRow(null, "TestUser", new[] { EditWikiPages }, false)]
	[DataRow("", "TestUser", new[] { EditWikiPages }, false)]
	[DataRow(" ", "TestUser", new[] { EditWikiPages }, false)]
	[DataRow("Test", "TestUser", new PermissionTo[0], false)]
	[DataRow("Test", "TestUser", new[] { EditWikiPages }, true)]
	[DataRow("Test", "TestUser", new[] { EditHomePage }, false)]
	[DataRow("Test", "TestUser", new[] { EditGameResources }, false)]
	[DataRow("Test", "TestUser", new[] { EditSystemPages }, false)]
	[DataRow("SandBox", "TestUser", new[] { EditWikiPages }, true)]
	[DataRow("SandBox", "TestUser", new[] { EditHomePage }, true)]
	[DataRow("SandBox", "TestUser", new[] { EditGameResources }, true)]
	[DataRow("SandBox", "TestUser", new[] { EditSystemPages }, true)]
	[DataRow("GameResources", "TestUser", new[] { EditGameResources }, false)]
	[DataRow("GameResources/NES", "TestUser", new[] { EditGameResources }, true)]
	[DataRow("/GameResources/NES", "TestUser", new[] { EditGameResources }, true)]
	[DataRow("GameResources/NES/", "TestUser", new[] { EditGameResources }, true)]
	[DataRow("GameResources/NES", "", new[] { EditGameResources }, false)]
	[DataRow("GameResources/NES/Mario", "TestUser", new[] { EditGameResources }, true)]
	[DataRow("System", "TestUser", new[] { EditSystemPages }, false)]
	[DataRow("System/Test", "TestUser", new[] { EditSystemPages }, true)]
	[DataRow("HomePages", "TestUser", new[] { EditHomePage }, false)]
	[DataRow("HomePages/TestUser", "TestUser", new[] { EditHomePage }, true)]
	[DataRow("HomePages/TestUser/Subpage", "TestUser", new[] { EditHomePage }, true)]
	[DataRow("HomePages/ADifferentUser", "TestUser", new[] { EditHomePage }, false)]
	public void UserCanEditWikiPage(
		string pageName,
		string userName,
		IEnumerable<PermissionTo> userPermissions,
		bool expected)
	{
		var actual = WikiHelper.UserCanEditWikiPage(
			pageName,
			userName,
			userPermissions);

		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(null, false)]
	[DataRow("", false)]
	[DataRow(" ", false)]
	[DataRow("/", false)]
	[DataRow("Test/", false)]
	[DataRow("Test.html", false)]
	[DataRow("/Test", false)]
	[DataRow("Test", true)]
	[DataRow("Test.html", false)]
	[DataRow("Test/Subpage", true)]
	[DataRow("ProperCased", true)]
	[DataRow("camelCased", false)]
	[DataRow("Page With Spaces", false)]
	[DataRow("HomePages/SomeUser", true)]
	[DataRow("HomePages/user with Invalid Page name", true)]
	[DataRow("HomePages/[^_^]", true)]
	[DataRow("HomePages/user with Invalid Page name/Subpage", true)]
	[DataRow("HomePages/user with Invalid Page name/Subpage that is invalid", false)]
	public void IsValidWikiPageName(string pageName, bool expected)
	{
		var actual = WikiHelper.IsValidWikiPageName(pageName);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(null, false)]
	[DataRow("", false)]
	[DataRow(" ", false)]
	[DataRow("/", false)]
	[DataRow("GameResources", false)]
	[DataRow("GameResources/", false)]
	[DataRow("GameResources/NES", true)]
	[DataRow("/GameResources/NES", true)]
	[DataRow("GameResources/NES/", true)]
	[DataRow("GameResources/NES/Mario", false)]
	public void IsSystemGameResourcePath(string pageName, bool expected)
	{
		var actual = pageName.IsSystemGameResourcePath();
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(null, "")]
	[DataRow("", "")]
	[DataRow(" ", "")]
	[DataRow("/", "")]
	[DataRow("GameResources", "")]
	[DataRow("GameResources/", "")]
	[DataRow("GameResources/NES", "NES")]
	[DataRow("/GameResources/NES", "NES")]
	[DataRow("GameResources/NES/", "NES")]
	[DataRow("GameResources/NES/Mario", "")]
	public void SystemGameResourcePath(string pageName, string expected)
	{
		var actual = pageName.SystemGameResourcePath();
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(null, false)]
	[DataRow("", false)]
	[DataRow(" ", false)]
	[DataRow("/", false)]
	[DataRow("/", false)]
	[DataRow("/Test", false)]
	[DataRow("HomePages", false)]
	[DataRow("HomePages/TestUser", true)]
	[DataRow("/HomePages/TestUser", false)]
	[DataRow("HomePages/[^_^]", true)]
	public void IsHomePage(string pageName, bool expected)
	{
		var actual = WikiHelper.IsHomePage(pageName);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(null, "")]
	[DataRow("", "")]
	[DataRow(" ", "")]
	[DataRow("/", "")]
	[DataRow("/Test", "")]
	[DataRow("HomePages", "")]
	[DataRow("HomePages/TestUser", "TestUser")]
	[DataRow("/HomePages/TestUser", "")]
	[DataRow("HomePages/[^_^]", "[^_^]")]
	public void ToUserName(string pageName, string expected)
	{
		var actual = WikiHelper.ToUserName(pageName);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(null, false)]
	[DataRow("", false)]
	[DataRow(" ", false)]
	[DataRow("/", false)]
	[DataRow("/Test", false)]
	[DataRow("System", false)]
	[DataRow("System/Test", true)]
	[DataRow("/System/Test", false)]
	public void IsSystemPage(string pageName, bool expected)
	{
		var actual = WikiHelper.IsSystemPage(pageName);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(null, false)]
	[DataRow("", false)]
	[DataRow(" ", false)]
	[DataRow("/", false)]
	[DataRow("Test", false)]
	[DataRow("GameResources", false)]
	[DataRow("GameResources/Test", true)]
	[DataRow("/GameResources/Test", false)]
	public void IsGameResourcesPage(string pageName, bool expected)
	{
		var actual = WikiHelper.IsGameResourcesPage(pageName);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(null, null)]
	[DataRow("", null)]
	[DataRow(" ", null)]
	[DataRow("/", null)]
	[DataRow("Test", null)]
	[DataRow("InternalSystem/PublicationContent", null)]
	[DataRow("InternalSystem/PublicationContent/M", null)]
	[DataRow("InternalSystem/PublicationContent/M10", 10)]
	[DataRow("InternalSystem/PublicationContent/MNotANumber", null)]
	public void IsPublicationPage(string pageName, int? expected)
	{
		var actual = WikiHelper.IsPublicationPage(pageName);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(null, null)]
	[DataRow("", null)]
	[DataRow(" ", null)]
	[DataRow("/", null)]
	[DataRow("Test", null)]
	[DataRow("InternalSystem/SubmissionContent", null)]
	[DataRow("InternalSystem/SubmissionContent/S", null)]
	[DataRow("InternalSystem/SubmissionContent/S10", 10)]
	[DataRow("InternalSystem/SubmissionContent/SNotANumber", null)]
	public void IsSubmissionPage(string pageName, int? expected)
	{
		var actual = WikiHelper.IsSubmissionPage(pageName);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(null, null)]
	[DataRow("", "")]
	[DataRow("Test", "Test")]
	[DataRow("/Test", "/Test")]
	[DataRow("InternalSystem/SubmissionContent/S", "")]
	[DataRow("InternalSystem/SubmissionContent/SNotANumber", "")]
	[DataRow("InternalSystem/SubmissionContent/S10", "10S")]
	[DataRow("InternalSystem/PublicationContent/M10", "10M")]
	[DataRow("InternalSystem/GameContent/G10", "10G")]
	public void ProcessLink(string pageName, string expected)
	{
		var actual = WikiHelper.ProcessLink(pageName);
		Assert.AreEqual(expected, actual);
	}
}
