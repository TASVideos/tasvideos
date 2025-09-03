using System.Text.Json;
using TASVideos.Api.Responses;
using TASVideos.E2E.Tests.Base;

namespace TASVideos.E2E.Tests.Tests;

[TestClass]
public class GamesApiTests : BaseE2ETest
{
	[TestMethod]
	public async Task GetGameById_ValidId_ReturnsGame()
	{
		AssertEnabled();

		var response = await ApiGetAsync("games/1");
		AssertApiOk(response);
		var game = await Deserialize<GamesResponse>(response);

		Assert.AreEqual(1, game.Id);
		Assert.IsFalse(string.IsNullOrEmpty(game.DisplayName));
		Assert.AreEqual("smb", game.Abbreviation);
		Assert.IsTrue(game.Versions.Any());
	}

	[TestMethod]
	public async Task GetGameById_InvalidId_Returns404()
	{
		AssertEnabled();
		var response = await ApiGetAsync("games/999999");
		AssertApiNotFound(response);
	}

	[TestMethod]
	public async Task GetGames_NoFilters_ReturnsList()
	{
		AssertEnabled();

		var response = await ApiGetAsync("games");
		AssertApiOk(response);

		var games = await Deserialize<GamesResponse[]>(response);

		Assert.IsTrue(games.Length > 0);

		var firstGame = games[0];
		Assert.IsFalse(string.IsNullOrEmpty(firstGame.DisplayName));
	}

	[TestMethod]
	public async Task GetGames_WithSystemFilter_ReturnsFilteredList()
	{
		AssertEnabled();

		const string system = "NES";
		var response = await ApiGetAsync($"games?systems={system}&pageSize=10");
		AssertApiOk(response);

		var games = await Deserialize<GamesResponse[]>(response);

		Assert.IsTrue(games.Length > 0);
		Assert.IsTrue(games.All(g => g.Versions.Any(v => v.SystemCode == system)));
	}

	[TestMethod]
	public async Task GetGames_WithPaginationParams_ReturnsValidResponse()
	{
		AssertEnabled();

		var response = await ApiGetAsync("games?pageSize=5");
		AssertApiOk(response);

		var games = await Deserialize<GamesResponse[]>(response);
		Assert.AreEqual(5, games.Length);
	}

	[TestMethod]
	public async Task GetGames_WithSorting_ReturnsSortedResults()
	{
		AssertEnabled();

		// Page 2 to avoid game with id -1
		var response = await ApiGetAsync("games?sortBy=id&sortDir=asc&currentPage=2&pageSize=10");
		AssertApiOk(response);

		var games = await Deserialize<GamesResponse[]>(response);

		Assert.IsTrue(games.Length > 1);

		// Verify ascending order
		var previousId = 0;
		foreach (var game in games)
		{
			Assert.IsTrue(game.Id >= previousId, $"IDs should be in ascending order. Previous: {previousId}, Current: {game.Id}");
			previousId = game.Id;
		}
	}

	[TestMethod]
	public async Task GetGames_WithFieldSelection_ReturnsSelectedFields()
	{
		AssertEnabled();

		var response = await ApiGetAsync("games?fields=id,displayName&pageSize=1");
		AssertApiOk(response);

		var content = await response.TextAsync();
		Assert.IsFalse(string.IsNullOrEmpty(content));

		var json = JsonDocument.Parse(content);
		var root = json.RootElement;

		Assert.AreEqual(JsonValueKind.Array, root.ValueKind);
		Assert.IsTrue(root.GetArrayLength() > 0);

		var game = root[0];

		Assert.IsTrue(game.TryGetProperty("id", out _));
		Assert.IsTrue(game.TryGetProperty("displayName", out _));

		Assert.IsFalse(game.TryGetProperty("abbreviation", out _));
		Assert.IsFalse(game.TryGetProperty("screenshotUrl", out _));
		Assert.IsFalse(game.TryGetProperty("versions", out _));
	}
}
