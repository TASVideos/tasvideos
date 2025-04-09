using TASVideos.Data.Entity;
using TASVideos.Data.Helpers;
using static TASVideos.Data.Entity.SubmissionStatus;

namespace TASVideos.Data.Tests.Helpers;

[TestClass]
public class SubmissionHelperTests
{
	[TestMethod]
	[DataRow("", null)]
	[DataRow(" ", null)]
	[DataRow("1007", null)]
	[DataRow("NotANumberS", null)]
	[DataRow("1007s", 1007)]
	[DataRow("/1007S", 1007)]
	[DataRow("/1007S/", 1007)]
	[DataRow("1007Sub", null)]
	[DataRow("/1007M", null)]
	public void IsSubmissionLink(string link, int? expected)
	{
		var actual = SubmissionHelper.IsSubmissionLink(link);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow("", null)]
	[DataRow(" ", null)]
	[DataRow("1007", null)]
	[DataRow("NotANumberM", null)]
	[DataRow("1007m", 1007)]
	[DataRow("1007M", 1007)]
	[DataRow("/1007M", 1007)]
	[DataRow("/1007M/", 1007)]
	[DataRow("1007MM", null)]
	[DataRow("/1007S", null)]
	public void IsPublicationLink(string link, int? expected)
	{
		var actual = SubmissionHelper.IsPublicationLink(link);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow("", null)]
	[DataRow(" ", null)]
	[DataRow("1007", null)]
	[DataRow("NotANumberG", null)]
	[DataRow("1007g", 1007)]
	[DataRow("1007G", 1007)]
	[DataRow("/1007G", 1007)]
	[DataRow("/1007G/", 1007)]
	[DataRow("1007Game", null)]
	[DataRow("/1007S", null)]
	public void IsGamePageLink(string link, int? expected)
	{
		var actual = SubmissionHelper.IsGamePageLink(link);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow("", null)]
	[DataRow(" ", null)]
	[DataRow("1007", null)]
	[DataRow("1007S", null)]
	[DataRow("InternalSystem/SubmissionContent/1007", null)]
	[DataRow("InternalSystem/SubmissionContent/S1007", 1007)]
	[DataRow("/InternalSystem/SubmissionContent/S1007", 1007)]
	[DataRow("/InternalSystem/SubmissionContent/S1007/", 1007)]
	public void IsRawSubmissionLink(string link, int? expected)
	{
		var actual = SubmissionHelper.IsRawSubmissionLink(link);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow("", null)]
	[DataRow(" ", null)]
	[DataRow("1007", null)]
	[DataRow("1007M", null)]
	[DataRow("InternalSystem/PublicationContent/1007", null)]
	[DataRow("InternalSystem/PublicationContent/M1007", 1007)]
	[DataRow("/InternalSystem/PublicationContent/M1007", 1007)]
	[DataRow("/InternalSystem/PublicationContent/M1007/", 1007)]
	public void IsRawPublicationLink(string link, int? expected)
	{
		var actual = SubmissionHelper.IsRawPublicationLink(link);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow("", null)]
	[DataRow(" ", null)]
	[DataRow("1007", null)]
	[DataRow("1007M", null)]
	[DataRow("InternalSystem/GameContent/1007", null)]
	[DataRow("InternalSystem/GameContent/G1007", 1007)]
	[DataRow("/InternalSystem/GameContent/G1007", 1007)]
	[DataRow("/InternalSystem/GameContent/G1007/", 1007)]
	public void IsRawGamePageLink(string link, int? expected)
	{
		var actual = SubmissionHelper.IsRawGamePageLink(link);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(JudgingUnderWay, JudgingUnderWay, false)]
	[DataRow(New, PublicationUnderway, false)]
	[DataRow(New, JudgingUnderWay, true)]
	[DataRow(NeedsMoreInfo, JudgingUnderWay, true)]
	[DataRow(Accepted, JudgingUnderWay, true)]
	public void JudgeIsClaiming(SubmissionStatus oldStatus, SubmissionStatus newStatus, bool expected)
	{
		var actual = SubmissionHelper.JudgeIsClaiming(oldStatus, newStatus);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(New, true)]
	[DataRow(JudgingUnderWay, false)]
	[DataRow(NeedsMoreInfo, false)]
	[DataRow(Cancelled, false)]
	[DataRow(Accepted, false)]
	[DataRow(Published, false)]
	public void JudgeIsUnclaiming(SubmissionStatus status, bool expected)
	{
		var actual = SubmissionHelper.JudgeIsUnclaiming(status);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(PublicationUnderway, PublicationUnderway, false)]
	[DataRow(New, JudgingUnderWay, false)]
	[DataRow(New, PublicationUnderway, true)]
	[DataRow(JudgingUnderWay, PublicationUnderway, true)]
	[DataRow(Accepted, PublicationUnderway, true)]
	public void PublisherIsClaiming(SubmissionStatus oldStatus, SubmissionStatus newStatus, bool expected)
	{
		var actual = SubmissionHelper.PublisherIsClaiming(oldStatus, newStatus);
		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	[DataRow(PublicationUnderway, PublicationUnderway, false)]
	[DataRow(PublicationUnderway, Accepted, true)]
	[DataRow(Accepted, Accepted, false)]
	[DataRow(New, Accepted, false)]
	[DataRow(PublicationUnderway, New, false)]
	public void PublisherIsUnclaiming(SubmissionStatus oldStatus, SubmissionStatus newStatus, bool expected)
	{
		var actual = SubmissionHelper.PublisherIsUnclaiming(oldStatus, newStatus);
		Assert.AreEqual(expected, actual);
	}
}
