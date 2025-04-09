using TASVideos.Data.Helpers;
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
}
