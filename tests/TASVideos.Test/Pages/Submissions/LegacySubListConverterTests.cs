using TASVideos.Data.Entity;
using TASVideos.Pages.Submissions;
using static TASVideos.Data.Entity.SubmissionStatus;

namespace TASVideos.RazorPages.Tests.Pages.Submissions;

[TestClass]
public class LegacySubListConverterTests
{
	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow(" ")]
	[DataRow("-")]
	public void ReturnsNull_WhenEmptyOrInvalidQuery(string queryString)
	{
		var actual = LegacySubListConverter.ToSearchRequest(queryString);
		Assert.IsNull(actual);
	}

	[TestMethod]
	[DataRow("Acc", new[] { Accepted })]
	[DataRow("acc", new[] { Accepted })]
	[DataRow("Acc-Rej", new[] { Accepted, Rejected })]
	[DataRow("InvalidStatus", new SubmissionStatus[0])]
	[DataRow("new-can-inf-del-jud-acc-und-pub-rej", new[] { New, Cancelled, NeedsMoreInfo, Delayed, JudgingUnderWay, Accepted, PublicationUnderway, Published, Rejected })]
	public void StatusConversions(string queryString, SubmissionStatus[] expected)
	{
		var request = LegacySubListConverter.ToSearchRequest(queryString);
		Assert.IsNotNull(request);
		Assert.IsNotNull(request.StatusFilter);
		var actual = request.StatusFilter.ToList();
		Assert.AreEqual(expected.Length, actual.Count);
		foreach (var status in expected)
		{
			Assert.IsTrue(actual.Contains(status));
		}
	}

	[TestMethod]
	[DataRow("Y1999", new int[0])]
	[DataRow("YNotANumber", new int[0])]
	[DataRow("2000Y", new int[0])]
	[DataRow("y2000", new[] { 2000 })]
	[DataRow("Y2000", new[] { 2000 })]
	[DataRow("Y2000-Y2001", new[] { 2000, 2001 })]
	public void YearConversions(string queryString, int[] expected)
	{
		var request = LegacySubListConverter.ToSearchRequest(queryString);
		Assert.IsNotNull(request);
		Assert.IsNotNull(request.Years);
		var actual = request.Years.ToList();
		Assert.AreEqual(expected.Length, actual.Count);
		foreach (var year in expected)
		{
			Assert.IsTrue(actual.Contains(year));
		}
	}
}
