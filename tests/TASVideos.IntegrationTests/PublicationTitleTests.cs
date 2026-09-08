using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.IntegrationTests;

[TestClass]
#pragma warning disable CA1001
public class PublicationTitleTests
#pragma warning restore CA1001
{
	private TASVideosWebApplicationFactory _factory = null!;
	private HttpClient _client = null!;

	[TestInitialize]
	public void Setup()
	{
		_factory = new TASVideosWebApplicationFactory(true);
		_client = _factory.CreateClientWithFollowRedirects();

		_factory.SeedDatabase(db =>
		{
			var author1 = db.Users.Add(new User
			{
				UserName = "TestUser1",
				NormalizedUserName = "TESTUSER1",
				Email = "test1@example.com",
				NormalizedEmail = "TEST1@EXAMPLE.COM",
			}).Entity;
			var author2 = db.Users.Add(new User
			{
				UserName = "TestUser2",
				NormalizedUserName = "TESTUSER2",
				Email = "test2@example.com",
				NormalizedEmail = "TEST2@EXAMPLE.COM",
			}).Entity;

			var game = db.Games.Add(new Game
			{
				DisplayName = "TestGame",
				GameGoals = [new() { DisplayName = "TestGoal" }],
				GameVersions = [new() { Name = "TestGame" }]
			}).Entity;

			var pubClass = db.PublicationClasses.Add(new PublicationClass { Name = "Standard" }).Entity;
			var gameSystem = db.GameSystems.Add(new GameSystem { Code = "NES" }).Entity;

			var publicationMetricTas = new Publication
			{
				Id = 1,
				System = gameSystem,
				Game = game,
				GameVersion = game.GameVersions.First(),
				GameGoal = game.GameGoals.First(),
				Authors = [new() { Author = author1, Ordinal = 1 }, new() { Author = author2, Ordinal = 2 }],
				AdditionalAuthors = "AdditionalDude,NiceDude",
				Frames = 1234,
				SystemFrameRate = new() { FrameRate = 60 },
				PublicationClass = pubClass,
				Submission = new() { Submitter = author1 },
				Files = [new() { Type = FileType.Screenshot }],
				Metric = OptimizationMetric.TASTiming,
				MovieFileName = "id1"
			};
			db.Publications.Add(publicationMetricTas);

			var publicationMetricIgt = new Publication
			{
				Id = 2,
				System = gameSystem,
				Game = game,
				GameVersion = game.GameVersions.First(),
				GameGoal = game.GameGoals.First(),
				Authors = [new() { Author = author1, Ordinal = 1 }, new() { Author = author2, Ordinal = 2 }],
				AdditionalAuthors = "AdditionalDude,NiceDude",
				Frames = 1234,
				SystemFrameRate = new() { FrameRate = 60 },
				PublicationClass = pubClass,
				Submission = new() { Submitter = author1 },
				Files = [new() { Type = FileType.Screenshot }],
				Metric = OptimizationMetric.InGameTiming,
				MetricValue = "123:99",
				MovieFileName = "id2"
			};
			db.Publications.Add(publicationMetricIgt);

			var publicationMetricScore = new Publication
			{
				Id = 3,
				System = gameSystem,
				Game = game,
				GameVersion = game.GameVersions.First(),
				GameGoal = game.GameGoals.First(),
				Authors = [new() { Author = author1, Ordinal = 1 }, new() { Author = author2, Ordinal = 2 }],
				AdditionalAuthors = "AdditionalDude,NiceDude",
				Frames = 1234,
				SystemFrameRate = new() { FrameRate = 60 },
				PublicationClass = pubClass,
				Submission = new() { Submitter = author1 },
				Files = [new() { Type = FileType.Screenshot }],
				Metric = OptimizationMetric.HighScore,
				MetricValue = "99745",
				MovieFileName = "id3"
			};
			db.Publications.Add(publicationMetricScore);
			db.SaveChanges();

			publicationMetricTas.Title = publicationMetricTas.GenerateTitle();
			publicationMetricIgt.Title = publicationMetricIgt.GenerateTitle();
			publicationMetricScore.Title = publicationMetricScore.GenerateTitle();
			db.SaveChanges();
		});
	}

	[TestCleanup]
	public void Cleanup()
	{
		_client.Dispose();
		_factory.Dispose();
	}

	private async Task GetPublicationTitleAndAssert(int publicationId)
	{
		var response = await _client.GetAsync($"/{publicationId}M");
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

		var pubTitleElement = await response.QuerySelectorAsync(".bg-publication h4");
		Assert.IsNotNull(pubTitleElement);

		var actual = pubTitleElement.TextContent.Trim();
		Assert.IsNotNull(actual);
		Assert.IsNotEmpty(actual);

		using var db = _factory.Services.GetRequiredService<ApplicationDbContext>();
		string expected = db.Publications.First(p => p.Id == publicationId).Title;

		Assert.AreEqual(expected, actual);
	}

	[TestMethod]
	public async Task PublicationPage_MetricTas_TitleSameAsGenerated()
	{
		await GetPublicationTitleAndAssert(publicationId: 1);
	}

	[TestMethod]
	public async Task PublicationPage_MetricIgt_TitleSameAsGenerated()
	{
		await GetPublicationTitleAndAssert(publicationId: 2);
	}

	[TestMethod]
	public async Task PublicationPage_MetricScore_TitleSameAsGenerated()
	{
		await GetPublicationTitleAndAssert(publicationId: 3);
	}
}
