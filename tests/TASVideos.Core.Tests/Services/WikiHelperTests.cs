using TASVideos.Data.Entity;
using TASVideos.Extensions;

namespace TASVideos.Core.Tests.Services;

[TestClass]
public class WikiHelperTests
{
	[TestMethod]
	[DataRow(null, null, PermissionTo.EditWikiPages, false)]
	[DataRow("", null, PermissionTo.EditWikiPages, false)]
	[DataRow(" ", null, PermissionTo.EditWikiPages, false)]
	[DataRow("WikiPage", null, PermissionTo.EditWikiPages, false)]
	[DataRow("WikiPage", "", PermissionTo.EditWikiPages, false)]
	[DataRow("WikiPage", " ", PermissionTo.EditWikiPages, false)]
	[DataRow("WikiPage", "User", PermissionTo.EditWikiPages, true, PermissionTo.EditWikiPages)]
	[DataRow(LinkConstants.PublicationWikiPage + "1", "User", PermissionTo.EditPublicationMetaData, true, PermissionTo.EditPublicationMetaData)]
	[DataRow(LinkConstants.PublicationWikiPage + "1", "User", PermissionTo.EditWikiPages, false, PermissionTo.EditPublicationMetaData)]
	[DataRow(LinkConstants.PublicationWikiPage + "1", "User", PermissionTo.EditGameResources, false, PermissionTo.EditPublicationMetaData)]
	[DataRow(LinkConstants.PublicationWikiPage + "1", "User", PermissionTo.EditHomePage, false, PermissionTo.EditPublicationMetaData)]
	[DataRow(LinkConstants.PublicationWikiPage + "1", "User", PermissionTo.EditSystemPages, false, PermissionTo.EditPublicationMetaData)]
	[DataRow(LinkConstants.PublicationWikiPage + "1", "User", PermissionTo.EditSubmissions, false, PermissionTo.EditPublicationMetaData)]
	[DataRow(LinkConstants.SubmissionWikiPage + "1", "User", PermissionTo.EditSubmissions, true, PermissionTo.EditSubmissions)]
	[DataRow(LinkConstants.SubmissionWikiPage + "1", "User", PermissionTo.EditWikiPages, false, PermissionTo.EditSubmissions)]
	[DataRow(LinkConstants.SubmissionWikiPage + "1", "User", PermissionTo.EditGameResources, false, PermissionTo.EditSubmissions)]
	[DataRow(LinkConstants.SubmissionWikiPage + "1", "User", PermissionTo.EditHomePage, false, PermissionTo.EditSubmissions)]
	[DataRow(LinkConstants.SubmissionWikiPage + "1", "User", PermissionTo.EditSystemPages, false, PermissionTo.EditSubmissions)]
	[DataRow(LinkConstants.SubmissionWikiPage + "1", "User", PermissionTo.EditPublicationMetaData, false, PermissionTo.EditSubmissions)]
	[DataRow("GameResources/NES", "User", PermissionTo.EditGameResources, true, PermissionTo.EditGameResources)]
	[DataRow("GameResources/NES", "User", PermissionTo.EditSubmissions, false, PermissionTo.EditGameResources)]
	[DataRow("GameResources/NES", "User", PermissionTo.EditWikiPages, false, PermissionTo.EditGameResources)]
	[DataRow("GameResources/NES", "User", PermissionTo.EditHomePage, false, PermissionTo.EditGameResources)]
	[DataRow("GameResources/NES", "User", PermissionTo.EditSystemPages, false, PermissionTo.EditGameResources)]
	[DataRow("GameResources/NES", "User", PermissionTo.EditPublicationMetaData, false, PermissionTo.EditGameResources)]
	[DataRow("GameResources/", "User", PermissionTo.EditGameResources, false, PermissionTo.EditWikiPages)]
	[DataRow("GameResources/", "User", PermissionTo.EditWikiPages, true, PermissionTo.EditWikiPages)]
	[DataRow("System/SystemPage", "User", PermissionTo.EditGameResources, false, PermissionTo.EditSystemPages)]
	[DataRow("System/SystemPage", "User", PermissionTo.EditSubmissions, false, PermissionTo.EditSystemPages)]
	[DataRow("System/SystemPage", "User", PermissionTo.EditWikiPages, false, PermissionTo.EditSystemPages)]
	[DataRow("System/SystemPage", "User", PermissionTo.EditHomePage, false, PermissionTo.EditSystemPages)]
	[DataRow("System/SystemPage", "User", PermissionTo.EditSystemPages, true, PermissionTo.EditSystemPages)]
	[DataRow("System/SystemPage", "User", PermissionTo.EditPublicationMetaData, false, PermissionTo.EditSystemPages)]
	[DataRow("System/", "User", PermissionTo.EditSystemPages, false, PermissionTo.EditWikiPages)] // TODO: I'm not sure this is intended, probably System should be a system page permision-wise
	[DataRow("System/", "User", PermissionTo.EditWikiPages, true, PermissionTo.EditWikiPages)]
	[DataRow(LinkConstants.HomePages, "User", PermissionTo.EditHomePage, false, PermissionTo.EditWikiPages)]
	[DataRow(LinkConstants.HomePages + "User", "User", PermissionTo.EditHomePage, true, PermissionTo.EditHomePage)]
	[DataRow(LinkConstants.HomePages + "User/", "User", PermissionTo.EditHomePage, true, PermissionTo.EditHomePage)]
	[DataRow(LinkConstants.HomePages + "User/UserSubPage", "User", PermissionTo.EditHomePage, true, PermissionTo.EditHomePage)]
	[DataRow(LinkConstants.HomePages + "useR/UserSubPage", "User", PermissionTo.EditHomePage, true, PermissionTo.EditHomePage)]
	[DataRow(LinkConstants.HomePages + "User", "AnotherUser", PermissionTo.EditHomePage, false, PermissionTo.EditWikiPages)]
	[DataRow(LinkConstants.HomePages + "User", "AnotherUser", PermissionTo.EditWikiPages, true, PermissionTo.EditWikiPages)]
	[DataRow(LinkConstants.HomePages + "User", "AnotherUser", PermissionTo.EditSystemPages, false, PermissionTo.EditWikiPages)]
	[DataRow(LinkConstants.HomePages + "User", "AnotherUser", PermissionTo.EditGameResources, false, PermissionTo.EditWikiPages)]
	[DataRow(LinkConstants.HomePages + "User", "AnotherUser", PermissionTo.EditSubmissions, false, PermissionTo.EditWikiPages)]
	[DataRow(LinkConstants.HomePages + "User", "AnotherUser", PermissionTo.EditPublicationMetaData, false, PermissionTo.EditWikiPages)]
	public void UserCanEditWikiPage(string pageName, string? userName, PermissionTo userPermissions, bool expected, PermissionTo? expectedRelevantPermission = null)
	{
		var actual = WikiHelper.UserCanEditWikiPage(pageName, userName, [userPermissions], out var relevantPermissions);
		Assert.AreEqual(expected, actual);

		if (expectedRelevantPermission != null)
		{
			Assert.IsTrue(relevantPermissions.Contains(expectedRelevantPermission.Value));
		}
		else
		{
			Assert.AreEqual(0, relevantPermissions.Count);
		}
	}

	[TestMethod]
	[DataRow(PermissionTo.EditGameResources, true)]
	[DataRow(PermissionTo.EditHomePage, true)]
	[DataRow(PermissionTo.EditWikiPages, true)]
	[DataRow(PermissionTo.EditSystemPages, true)]
	[DataRow(PermissionTo.EditSubmissions, true)]
	[DataRow(PermissionTo.EditPublicationMetaData, true)]
	public void UserCanEditWikiPageSandbox(PermissionTo userPermission, bool expected)
	{
		var actual = WikiHelper.UserCanEditWikiPage("SandBox", "User", [userPermission], out var relevantPermissions);
		Assert.AreEqual(expected, actual);

		Assert.IsTrue(relevantPermissions.Contains(PermissionTo.EditGameResources));
		Assert.IsTrue(relevantPermissions.Contains(PermissionTo.EditHomePage));
		Assert.IsTrue(relevantPermissions.Contains(PermissionTo.EditWikiPages));
		Assert.IsTrue(relevantPermissions.Contains(PermissionTo.EditSystemPages));
		Assert.IsTrue(relevantPermissions.Contains(PermissionTo.EditSubmissions));
		Assert.IsTrue(relevantPermissions.Contains(PermissionTo.EditPublicationMetaData));
	}

	[TestMethod]
	[DataRow(null, false, false)]
	[DataRow("", false, false)]
	[DataRow(" ", false, false)]
	[DataRow("/", false, false)]
	[DataRow("WikiPage/", false, false)]
	[DataRow("WikiPage.html", false, false)]
	[DataRow("/WikiPage", false, false)]
	[DataRow("WikiPage", true, true)]
	[DataRow("wikipage", false, true)]
	[DataRow("WikiPage.html", false, false)]
	[DataRow("WikiPage/Subpage", true, true)]
	[DataRow("ProperCased", true, true)]
	[DataRow("camelCased", false, true)]
	[DataRow("Page With Spaces", false, false)]
	[DataRow("HomePages/SomeUser", true, true)]
	[DataRow("HomePages/user with Invalid Page name", true, true)]
	[DataRow("HomePages/[^_^]", true, true)]
	[DataRow("HomePages/user with Invalid Page name/Subpage", true, true)]
	[DataRow("HomePages/user with Invalid Page name/Subpage that is invalid", false, false)]
	public void IsValidWikiPageName(string pageName, bool expectedStrict, bool expectedLoose)
	{
		var actualStrict = WikiHelper.IsValidWikiPageName(pageName, validateLoosely: false);
		var actualLoose = WikiHelper.IsValidWikiPageName(pageName, validateLoosely: true);
		Assert.AreEqual(expectedStrict, actualStrict);
		Assert.AreEqual(expectedLoose, actualLoose);
	}

	[TestMethod]
	[DataRow("", false)]
	[DataRow(" ", false)]
	[DataRow("/", false)]
	[DataRow("GameResources", false)]
	[DataRow("GameResources/", false)]
	[DataRow("GameResources/NES", true)]
	[DataRow("GameResources/nes", true)]
	[DataRow("gameResources/nes", false)]
	[DataRow("GameResources/NES/", true)]
	[DataRow("GameResources/NES/Mario", false)]
	public void IsSystemGameResourcePath(string path, bool expected)
	{
		var actual = path.IsSystemGameResourcePath();
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow("", "")]
	[DataRow(" ", "")]
	[DataRow("/", "")]
	[DataRow("GameResources", "")]
	[DataRow("GameResources/", "")]
	[DataRow("GameResources/NES", "NES")]
	[DataRow("/GameResources/NES/", "NES")]
	[DataRow("GameResources/NES/", "NES")]
	[DataRow("GameResources/nEs", "nEs")]
	[DataRow("GameResources/NES/Mario", "")]
	public void SystemGameResourcePath(string path, string expected)
	{
		var actual = path.SystemGameResourcePath();
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(null, false)]
	[DataRow("", false)]
	[DataRow(" ", false)]
	[DataRow("/", false)]
	[DataRow("User", false)]
	[DataRow("HomePages", false)]
	[DataRow("HomePages/", false)]
	[DataRow("HomePages/User", true)]
	[DataRow("/HomePages/User", false)]
	[DataRow("HomePages/User/", true)]
	[DataRow("HomePages/User/SubPage", true)]
	[DataRow("HomePages/[^_^]", true)]
	public void IsHomePage(string? pageName, bool expected)
	{
		var actual = WikiHelper.IsHomePage(pageName);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow("", "")]
	[DataRow(" ", "")]
	[DataRow("/", "")]
	[DataRow("WikiPage", "")]
	[DataRow("HomePages", "")]
	[DataRow("HomePages/", "")]
	[DataRow("HomePages/User", "User")]
	[DataRow("/HomePages/User", "")]
	[DataRow("HomePages/User/SubPage", "User")]
	[DataRow("WikiPage/User", "")]
	[DataRow("HomePages/[^_^]", "[^_^]")]
	public void ToUserName(string pageName, string expected)
	{
		var actual = WikiHelper.ToUserName(pageName);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow("", "")]
	[DataRow(" ", " ")]
	[DataRow("/", "/")]
	[DataRow("WikiPage", "WikiPage")]
	[DataRow("/WikiPage", "/WikiPage")]
	[DataRow("HomePages", "HomePages")]
	[DataRow("HomePages/", "HomePages/")]
	[DataRow("HomePages/User", "HomePages/User")]
	[DataRow("HomePages/User/SubPage", "HomePages/User/SubPage")]
	[DataRow("HomePages/User With Spaces", "HomePages/User%20With%20Spaces")]
	[DataRow("HomePages/User With Spaces/Sub Page", "HomePages/User%20With%20Spaces/Sub Page")]
	[DataRow("HomePages/[^_^]", "HomePages/%5B%5E_%5E%5D")]
	public void EscapeUserName(string pageName, string expected)
	{
		var actual = WikiHelper.EscapeUserName(pageName);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(null, false)]
	[DataRow("", false)]
	[DataRow(" ", false)]
	[DataRow("/", false)]
	[DataRow("WikiPage", false)]
	[DataRow("System", false)]
	[DataRow("System/", false)]
	[DataRow("System/A", true)]
	[DataRow("System/A/", true)]
	[DataRow("System/A/B", true)]
	public void IsSystemPage(string? pageName, bool expected)
	{
		var actual = WikiHelper.IsSystemPage(pageName);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(null, false)]
	[DataRow("", false)]
	[DataRow(" ", false)]
	[DataRow("/", false)]
	[DataRow("WikiPage", false)]
	[DataRow("GameResources", false)]
	[DataRow("GameResources/", false)]
	[DataRow("GameResources/NES", true)]
	[DataRow("/GameResources/NES", false)]
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
	[DataRow(LinkConstants.PublicationWikiPage, null)]
	[DataRow(LinkConstants.PublicationWikiPage + "10", 10)]
	[DataRow(LinkConstants.PublicationWikiPage + "NotANumber", null)]
	public void IsPublicationPage(string pageName, int? expected)
	{
		if (expected is null)
		{
			Assert.IsFalse(WikiHelper.IsPublicationPage(pageName, out _), "success");
		}
		else
		{
			Assert.IsTrue(WikiHelper.IsPublicationPage(pageName, out var actual), "success");
			Assert.AreEqual(expected, actual, "match");
		}
	}

	[TestMethod]
	[DataRow(null, null)]
	[DataRow("", null)]
	[DataRow(" ", null)]
	[DataRow("/", null)]
	[DataRow("Test", null)]
	[DataRow("InternalSystem/SubmissionContent", null)]
	[DataRow(LinkConstants.SubmissionWikiPage, null)]
	[DataRow(LinkConstants.SubmissionWikiPage + "10", 10)]
	[DataRow(LinkConstants.SubmissionWikiPage + "NotANumber", null)]
	public void IsSubmissionPage(string pageName, int? expected)
	{
		if (expected is null)
		{
			Assert.IsFalse(WikiHelper.IsSubmissionPage(pageName, out _), "success");
		}
		else
		{
			Assert.IsTrue(WikiHelper.IsSubmissionPage(pageName, out var actual), "success");
			Assert.AreEqual(expected, actual, "match");
		}
	}

	[TestMethod]
	[DataRow("", "")]
	[DataRow("Test", "Test")]
	[DataRow("/Test", "/Test")]
	[DataRow(LinkConstants.SubmissionWikiPage, "")]
	[DataRow(LinkConstants.SubmissionWikiPage + "NotANumber", "")]
	[DataRow(LinkConstants.SubmissionWikiPage + "10", "10S")]
	[DataRow(LinkConstants.PublicationWikiPage + "10", "10M")]
	[DataRow(LinkConstants.GameWikiPage + "10", "10G")]
	public void ProcessLink(string pageName, string expected)
	{
		var actual = WikiHelper.ProcessLink(pageName);
		Assert.AreEqual(expected, actual);
	}
}
