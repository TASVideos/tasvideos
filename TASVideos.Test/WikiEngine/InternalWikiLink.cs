using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TASVideos.WikiEngine;

// ReSharper disable InconsistentNaming
namespace TASVideos.Test.WikiEngine
{
	[TestClass]
	public class InternalWikiLink
	{
		[TestMethod]
		[DataRow(null)]
		[DataRow("")]
		[DataRow("\r \n \t")]
		public void GetAllInternalLinks_NoContent_ReturnsEmptyList(string content)
		{
			var actual = Util.GetAllInternalLinks(content);
			Assert.IsNotNull(actual);
			Assert.AreEqual(0, actual.Count());
		}

		[TestMethod]
		[DataRow("RecentChanges")]
		[DataRow("1S")]
		// TODO:
		////[DataRow("HomePages/adelikat")]
		////[DataRow("HomePages/Ready Steady Yeti/Outgoing")]
		////[DataRow("HomePages/[^_^]")]
		public void GetAllInternalLinks_OneNormalizedLink_ReturnsLink(string link)
		{
			var actual = Util.GetAllInternalLinks($"[{link}]");
			Assert.IsNotNull(actual);
			var actualList = actual.ToList();
			Assert.AreEqual(1, actualList.Count);
			Assert.AreEqual(link, actualList.Single().Link);
		}

		[TestMethod]
		public void GetAllInternalLinks_MultipleLinks_ReturnsAllLinks()
		{
			string link1 = "Link1";
			string link2 = "Link2";
			
			var actual = Util.GetAllInternalLinks($"[{link1}] [{link2}]");

			Assert.IsNotNull(actual);
			var actualList = actual.ToList();
			Assert.AreEqual(2, actualList.Count);
			Assert.IsTrue(actualList.Any(l => l.Link == link1));
			Assert.IsTrue(actualList.Any(l => l.Link == link2));
		}
	}
}
