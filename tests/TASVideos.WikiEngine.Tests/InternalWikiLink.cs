using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TASVideos.WikiEngine.Tests;

[TestClass]
public class InternalWikiLink
{
	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("\r \n \t")]
	public void GetReferrals_NoContent_ReturnsEmptyList(string content)
	{
		var actual = Util.GetReferrals(content);
		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count());
	}

	[TestMethod]
	[DataRow("RecentChanges", "RecentChanges")]
	[DataRow("1S", "1S")]
	[DataRow("HomePages/adelikat", "HomePages/adelikat")]
	[DataRow("HomePages/Ready Steady Yeti/Outgoing", "HomePages/Ready Steady Yeti/Outgoing")]
	[DataRow("Recent Changes", "RecentChanges")]
	[DataRow("recent Changes", "RecentChanges")]
	[DataRow("ArbitraryCodeExecutionHowTo|ACE How To", "ArbitraryCodeExecutionHowTo")]
	[DataRow("=HomePages/£e Nécroyeur", "HomePages/£e Nécroyeur", DisplayName = "Special characters require =")]
	public void GetReferrals_OneNormalizedLink_ReturnsLink(string link, string expected)
	{
		var actual = Util.GetReferrals($"[{link}]");
		Assert.IsNotNull(actual);
		var actualList = actual.ToList();
		Assert.AreEqual(1, actualList.Count);
		Assert.AreEqual(expected, actualList.Single().Link);
	}

	[TestMethod]
	public void GetReferrals_Anchors_AreNotReturned()
	{
		var link = "MovieRules";
		var anchor = "#GameChoice";
		var content = $"[{link}{anchor}]";

		var actual = Util.GetReferrals(content);

		Assert.IsNotNull(actual);
		var actualList = actual.ToList();
		Assert.AreEqual(1, actualList.Count);
		Assert.AreEqual(link, actualList.Single().Link);
	}

	[TestMethod]
	public void GetReferrals_AnchorOnly_NotReturned()
	{
		// Creates an implicit link to itself that is anchored
		// This is not considered a referral
		var content = "[#GameChoice]";

		var actual = Util.GetReferrals(content);
		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count());
	}

	[TestMethod]
	public void GetReferrals_UserModule_NotConsideredALink()
	{
		var actual = Util.GetReferrals("[user:foo]");
		Assert.IsNotNull(actual);
		Assert.AreEqual(0, actual.Count());
	}

	[TestMethod]
	public void GetReferrals_MultipleLinks_ReturnsAllLinks()
	{
		string link1 = "Link1";
		string link2 = "Link2";

		var actual = Util.GetReferrals($"[{link1}] [{link2}]");

		Assert.IsNotNull(actual);
		var actualList = actual.ToList();
		Assert.AreEqual(2, actualList.Count);
		Assert.IsTrue(actualList.Any(l => l.Link == link1));
		Assert.IsTrue(actualList.Any(l => l.Link == link2));
	}
}
