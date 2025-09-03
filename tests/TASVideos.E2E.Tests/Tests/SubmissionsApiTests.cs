using System.Net;
using System.Text.Json;
using TASVideos.Api.Responses;
using TASVideos.Data.Entity;
using TASVideos.E2E.Tests.Base;

namespace TASVideos.E2E.Tests.Tests;

[TestClass]
public class SubmissionsApiTests : BaseE2ETest
{
	[TestMethod]
	public async Task GetSubmissionById_ValidId_ReturnsSubmission()
	{
		AssertEnabled();

		var response = await ApiGetAsync("submissions/1140");

		AssertApiOk(response);
		var sub = await Deserialize<SubmissionsResponse>(response);

		Assert.AreEqual(1140, sub.Id);
		Assert.IsTrue(sub.Authors.Any());
		Assert.AreEqual(72297, sub.RerecordCount);
		Assert.AreEqual(576, sub.PublicationId);
		Assert.AreEqual("m64", sub.MovieExtension);
	}

	[TestMethod]
	public async Task GetSubmissionById_InvalidId_Returns404()
	{
		AssertEnabled();
		var response = await ApiGetAsync("submissions/999999");
		AssertApiNotFound(response);
	}

	[TestMethod]
	public async Task GetSubmissions_NoFilters_ReturnsList()
	{
		AssertEnabled();

		var response = await ApiGetAsync("submissions");
		AssertApiOk(response);

		var submissions = await Deserialize<SubmissionsResponse[]>(response);

		Assert.IsTrue(submissions.Length > 0);

		var firstSubmission = submissions[0];
		Assert.IsTrue(firstSubmission.Id > 0);
		Assert.IsFalse(string.IsNullOrEmpty(firstSubmission.Title));
	}

	[TestMethod]
	public async Task GetSubmissions_WithUserFilter_ReturnsFilteredList()
	{
		AssertEnabled();

		const string author = "adelikat";
		var response = await ApiGetAsync($"submissions?user={author}&pageSize=10");
		AssertApiOk(response);

		var submissions = await Deserialize<SubmissionsResponse[]>(response);

		Assert.IsTrue(submissions.Length > 0);

		var hasAuthor = submissions.Any(s => s.Authors.Contains(author)); // author could be submitter
		Assert.IsTrue(hasAuthor, "submissions author list should contain author");
	}

	[TestMethod]
	public async Task GetSubmissions_WithStatusFilter_ReturnsFilteredList()
	{
		AssertEnabled();

		var response = await ApiGetAsync($"submissions?statuses={(int)SubmissionStatus.Published}&pageSize=10");
		AssertApiOk(response);

		var submissions = await Deserialize<SubmissionsResponse[]>(response);
		Assert.IsTrue(submissions.All(s => s.Status == nameof(SubmissionStatus.Published)));
	}

	[TestMethod]
	public async Task GetSubmissions_WithSystemFilter_ReturnsFilteredList()
	{
		AssertEnabled();

		const string system = "NES";
		var response = await ApiGetAsync($"submissions?systems={system}&pageSize=10");
		AssertApiOk(response);

		var submissions = await Deserialize<SubmissionsResponse[]>(response);

		Assert.IsTrue(submissions.Length > 0);
		Assert.IsTrue(submissions.All(s => s.SystemCode == system));
	}

	[TestMethod]
	public async Task GetSubmissions_WithYearFilter_ReturnsFilteredList()
	{
		AssertEnabled();

		const int year = 2007;
		var response = await ApiGetAsync($"submissions?startYear=2007&endYear={year}");
		AssertApiOk(response);

		var submissions = await Deserialize<SubmissionsResponse[]>(response);

		Assert.IsTrue(submissions.Length > 0);
		Assert.IsTrue(submissions.All(s => s.CreateTimestamp.Year == year));
	}

	[TestMethod]
	public async Task GetSubmissions_WithPaginationParams_ReturnsValidResponse()
	{
		AssertEnabled();

		var response = await ApiGetAsync("submissions?pageSize=3");
		AssertApiOk(response);

		var submissions = await Deserialize<SubmissionsResponse[]>(response);
		Assert.AreEqual(3, submissions.Length);
	}

	[TestMethod]
	public async Task GetSubmissions_WithSorting_ReturnsSortedResults()
	{
		AssertEnabled();

		// Sort by ID ascending
		var response = await ApiGetAsync("submissions?sortBy=id&sortDir=asc&pageSize=10");
		AssertApiOk(response);

		var submissions = await Deserialize<SubmissionsResponse[]>(response);

		Assert.IsTrue(submissions.Length > 1);

		// Verify ascending order
		var previousId = 0;
		foreach (var submission in submissions)
		{
			Assert.IsTrue(submission.Id >= previousId, $"IDs should be in ascending order. Previous: {previousId}, Current: {submission.Id}");
			previousId = submission.Id;
		}
	}

	[TestMethod]
	public async Task GetSubmissions_WithInvalidStatus_ReturnsBadRequest()
	{
		AssertEnabled();
		var response = await ApiGetAsync("submissions?statuses=invalid");
		AssertApiBadRequest(response);
	}

	[TestMethod]
	public async Task GetSubmissions_WithFieldSelection_ReturnsSelectedFields()
	{
		AssertEnabled();

		var response = await ApiGetAsync("submissions?fields=id,title,status&limit=1");
		AssertApiOk(response);

		Assert.AreEqual((int)HttpStatusCode.OK, response.Status);

		var content = await response.TextAsync();
		Assert.IsFalse(string.IsNullOrEmpty(content));

		var json = JsonDocument.Parse(content);
		var root = json.RootElement;

		Assert.AreEqual(JsonValueKind.Array, root.ValueKind);
		Assert.IsTrue(root.GetArrayLength() > 0);

		var submission = root[0];

		Assert.IsTrue(submission.TryGetProperty("id", out _));
		Assert.IsTrue(submission.TryGetProperty("title", out _));
		Assert.IsTrue(submission.TryGetProperty("status", out _));

		Assert.IsFalse(submission.TryGetProperty("movieExtension", out _));
		Assert.IsFalse(submission.TryGetProperty("gameId", out _));
		Assert.IsFalse(submission.TryGetProperty("gameVersion", out _));
	}
}
