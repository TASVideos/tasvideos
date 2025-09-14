using System.Text.Json;
using TASVideos.Api.Responses;
using TASVideos.E2E.Tests.Base;

namespace TASVideos.E2E.Tests.Tests;

[TestClass]
public class PublicationsApiTests : BaseE2ETest
{
	[TestMethod]
	public async Task GetPublicationById_ValidId_ReturnsPublication()
	{
		AssertEnabled();

		var response = await ApiGetAsync("publications/1");
		AssertApiOk(response);
		var pub = await Deserialize<PublicationsResponse>(response);

		Assert.AreEqual(1, pub.Id);
		Assert.IsTrue(pub.Authors.Any(a => a == "Bisqwit"));
		Assert.AreEqual("nes", pub.SystemCode.ToLower());
		Assert.AreEqual(4007, pub.ObsoletedById);
	}

	[TestMethod]
	public async Task GetPublicationById_InvalidId_Returns404()
	{
		AssertEnabled();
		var response = await ApiGetAsync("publications/999999");
		AssertApiNotFound(response);
	}

	[TestMethod]
	public async Task GetPublications_NoFilters_ReturnsList()
	{
		AssertEnabled();

		var response = await ApiGetAsync("publications");
		AssertApiOk(response);

		var publications = await Deserialize<PublicationsResponse[]>(response);

		Assert.IsTrue(publications.Length > 0);

		var pub = publications[0];
		Assert.IsTrue(pub.Id > 0);
		Assert.IsFalse(string.IsNullOrEmpty(pub.Title));
	}

	[TestMethod]
	public async Task GetPublications_WithSystemFilter_ReturnsFilteredList()
	{
		AssertEnabled();

		const string system = "NES";
		var response = await ApiGetAsync($"publications?systems={system}&pageSize=10");
		AssertApiOk(response);

		var publications = await Deserialize<PublicationsResponse[]>(response);

		Assert.IsTrue(publications.Length > 0);
		Assert.IsTrue(publications.All(p => p.SystemCode == system));
	}

	[TestMethod]
	public async Task GetPublications_WithClassFilter_ReturnsFilteredList()
	{
		AssertEnabled();

		const string className = "Standard";
		var response = await ApiGetAsync($"publications?classNames={className}&pageSize=10");
		AssertApiOk(response);

		var publications = await Deserialize<PublicationsResponse[]>(response);

		Assert.IsTrue(publications.Length > 0);
		Assert.IsTrue(publications.All(p => p.Class == className));
	}

	[TestMethod]
	public async Task GetPublications_WithYearFilter_ReturnsFilteredList()
	{
		AssertEnabled();

		const int year = 2005;
		var response = await ApiGetAsync($"publications?startYear={year}&endYear={year}&pageSize=10");
		AssertApiOk(response);

		var publications = await Deserialize<PublicationsResponse[]>(response);

		Assert.IsTrue(publications.Length > 0);
		Assert.IsTrue(publications.All(p => p.CreateTimestamp.Year == year));
	}

	[TestMethod]
	public async Task GetPublications_WithPaginationParams_ReturnsValidResponse()
	{
		AssertEnabled();

		var response = await ApiGetAsync("publications?pageSize=5");
		AssertApiOk(response);

		var publications = await Deserialize<PublicationsResponse[]>(response);
		Assert.AreEqual(5, publications.Length);
	}

	[TestMethod]
	public async Task GetPublications_WithSorting_ReturnsSortedResults()
	{
		AssertEnabled();

		var response = await ApiGetAsync("publications?sortBy=id&sortDir=asc&pageSize=10");
		AssertApiOk(response);

		var publications = await Deserialize<PublicationsResponse[]>(response);

		Assert.IsTrue(publications.Length > 1);

		var previousId = 0;
		foreach (var publication in publications)
		{
			Assert.IsTrue(publication.Id >= previousId, $"IDs should be in ascending order. Previous: {previousId}, Current: {publication.Id}");
			previousId = publication.Id;
		}
	}

	[TestMethod]
	public async Task GetPublications_WithFieldSelection_ReturnsSelectedFields()
	{
		AssertEnabled();

		var response = await ApiGetAsync("publications?fields=id,title,systemCode&pageSize=1");
		AssertApiOk(response);

		var content = await response.TextAsync();
		Assert.IsFalse(string.IsNullOrEmpty(content));

		var json = JsonDocument.Parse(content);
		var root = json.RootElement;

		Assert.AreEqual(JsonValueKind.Array, root.ValueKind);
		Assert.IsTrue(root.GetArrayLength() > 0);

		var publication = root[0];

		Assert.IsTrue(publication.TryGetProperty("id", out _));
		Assert.IsTrue(publication.TryGetProperty("title", out _));
		Assert.IsTrue(publication.TryGetProperty("systemCode", out _));

		Assert.IsFalse(publication.TryGetProperty("emulatorVersion", out _));
		Assert.IsFalse(publication.TryGetProperty("movieFileName", out _));
		Assert.IsFalse(publication.TryGetProperty("frames", out _));
	}
}
