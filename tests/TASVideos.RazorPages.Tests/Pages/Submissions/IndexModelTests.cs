using TASVideos.Core.Services;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.Submissions;
using TASVideos.Tests.Base;

namespace TASVideos.RazorPages.Tests.Pages.Submissions;

[TestClass]
public class IndexModelTests : TestDbBase
{
	private readonly IGameSystemService _gameSystemService;
	private readonly IndexModel _page;

	public IndexModelTests()
	{
		_gameSystemService = Substitute.For<IGameSystemService>();
		_page = new IndexModel(_db, _gameSystemService);
	}

	[TestMethod]
	public async Task OnGet_PopulatesSystemList()
	{
		_gameSystemService.GetAll().Returns(
		[
			new(1, "NES", "Nintendo Entertainment System", []),
			new(2, "SNES", "Super Nintendo Entertainment System", [])
		]);

		await _page.OnGet();

		Assert.AreEqual(3, _page.SystemList.Count); // 2 systems + default entry
		Assert.AreEqual("", _page.SystemList[0].Value); // Default entry
		Assert.AreEqual("NES", _page.SystemList[1].Value);
		Assert.AreEqual("SNES", _page.SystemList[2].Value);
	}

	[TestMethod]
	public async Task OnGet_NoQueryString_UsesDefaultStatuses()
	{
		_gameSystemService.GetAll().Returns([]);
		_page.Query = null;

		await _page.OnGet();

		Assert.IsTrue(_page.Search.Statuses.Any());
		Assert.AreEqual(IndexModel.SubmissionSearchRequest.Default.Count, _page.Search.Statuses.Count);
		foreach (var status in IndexModel.SubmissionSearchRequest.Default)
		{
			Assert.IsTrue(_page.Search.Statuses.Contains(status));
		}
	}

	[TestMethod]
	public async Task OnGet_WithUser_UsesAllStatuses()
	{
		_gameSystemService.GetAll().Returns([]);
		_page.Search = new IndexModel.SubmissionSearchRequest { User = "TestUser" };

		await _page.OnGet();

		Assert.AreEqual(IndexModel.SubmissionSearchRequest.All.Count, _page.Search.Statuses.Count);
		foreach (var status in IndexModel.SubmissionSearchRequest.All)
		{
			Assert.IsTrue(_page.Search.Statuses.Contains(status));
		}
	}

	[TestMethod]
	public async Task OnGet_WithYears_UsesAllStatuses()
	{
		_gameSystemService.GetAll().Returns([]);
		_page.Search = new IndexModel.SubmissionSearchRequest { Years = [2024, 2025] };

		await _page.OnGet();

		Assert.AreEqual(IndexModel.SubmissionSearchRequest.All.Count, _page.Search.Statuses.Count);
		foreach (var status in IndexModel.SubmissionSearchRequest.All)
		{
			Assert.IsTrue(_page.Search.Statuses.Contains(status));
		}
	}

	[TestMethod]
	public async Task OnGet_WithGameId_DoesNotSetDefaultStatuses()
	{
		_gameSystemService.GetAll().Returns([]);
		_page.Search = new IndexModel.SubmissionSearchRequest { GameId = "123" };

		await _page.OnGet();

		Assert.IsFalse(_page.Search.Statuses.Any());
	}

	[TestMethod]
	public async Task OnGet_WithExistingStatuses_PreservesStatuses()
	{
		_gameSystemService.GetAll().Returns([]);
		var customStatuses = new List<SubmissionStatus> { SubmissionStatus.New, SubmissionStatus.Accepted };
		_page.Search = new IndexModel.SubmissionSearchRequest { Statuses = customStatuses };

		await _page.OnGet();

		Assert.AreEqual(2, _page.Search.Statuses.Count);
		Assert.IsTrue(_page.Search.Statuses.Contains(SubmissionStatus.New));
		Assert.IsTrue(_page.Search.Statuses.Contains(SubmissionStatus.Accepted));
	}

	[TestMethod]
	public async Task OnGet_PopulatesSubmissionsFromDatabase()
	{
		_gameSystemService.GetAll().Returns([]);

		var system = _db.GameSystems.Add(new GameSystem { Code = "NES" }).Entity;
		var game = _db.Games.Add(new Game { DisplayName = "Test Game" }).Entity;
		var user = _db.AddUser("TestUser").Entity;
		_db.Submissions.Add(new Submission
		{
			Title = "Test Submission",
			Game = game,
			System = system,
			Submitter = user,
			Status = SubmissionStatus.New
		});
		await _db.SaveChangesAsync();

		await _page.OnGet();

		Assert.AreEqual(1, _page.Submissions.Count());
		Assert.AreEqual(system.Code, _page.Submissions.Single().System);
	}

	[TestMethod]
	public async Task OnGet_EmptyDatabase_HandlesGracefully()
	{
		_gameSystemService.GetAll().Returns([]);

		await _page.OnGet();

		Assert.IsFalse(_page.Submissions.Any());
		Assert.AreEqual(1, _page.SystemList.Count); // Just the default entry
	}

	[TestMethod]
	public void AvailableStatuses_ContainsAllSubmissionStatuses()
	{
		var allStatuses = Enum.GetValues<SubmissionStatus>().ToList();
		var availableStatuses = IndexModel.AvailableStatuses;

		Assert.AreEqual(allStatuses.Count, availableStatuses.Count);
		foreach (var status in allStatuses)
		{
			Assert.IsTrue(availableStatuses.Any(s => s.Value == ((int)status).ToString()));
		}
	}

	[TestMethod]
	public void AvailableYears_ContainsValidRange()
	{
		var years = _page.AvailableYears.ToList();
		var currentYear = DateTime.UtcNow.Year;

		Assert.IsTrue(years.Any(y => y.Value == "2000"));
		Assert.IsTrue(years.Any(y => y.Value == currentYear.ToString()));

		// Should be in descending order
		var yearValues = years.Select(y => int.Parse(y.Value!)).ToList();
		for (var i = 1; i < yearValues.Count; i++)
		{
			Assert.IsTrue(yearValues[i - 1] > yearValues[i]);
		}
	}

	[TestMethod]
	public void SubmissionSearchRequest_Default_ContainsExpectedStatuses()
	{
		var expectedStatuses = new[]
		{
			SubmissionStatus.New,
			SubmissionStatus.JudgingUnderWay,
			SubmissionStatus.Accepted,
			SubmissionStatus.PublicationUnderway,
			SubmissionStatus.NeedsMoreInfo,
			SubmissionStatus.Delayed
		};

		var defaultStatuses = IndexModel.SubmissionSearchRequest.Default;

		Assert.AreEqual(expectedStatuses.Length, defaultStatuses.Count);
		foreach (var status in expectedStatuses)
		{
			Assert.IsTrue(defaultStatuses.Contains(status));
		}
	}

	[TestMethod]
	public void SubmissionSearchRequest_All_ContainsAllStatuses()
	{
		var allStatuses = Enum.GetValues<SubmissionStatus>().ToList();
		var allFromSearchRequest = IndexModel.SubmissionSearchRequest.All;

		Assert.AreEqual(allStatuses.Count, allFromSearchRequest.Count);
		foreach (var status in allStatuses)
		{
			Assert.IsTrue(allFromSearchRequest.Contains(status));
		}
	}

	[TestMethod]
	public void SubmissionSearchRequest_Systems_ReturnsCorrectCollection()
	{
		// Empty when System is null or empty
		ISubmissionFilter filter = new IndexModel.SubmissionSearchRequest { System = null };
		Assert.IsFalse(filter.Systems.Any());

		filter = new IndexModel.SubmissionSearchRequest { System = "" };
		Assert.IsFalse(filter.Systems.Any());

		filter = new IndexModel.SubmissionSearchRequest { System = "   " };
		Assert.IsFalse(filter.Systems.Any());

		// Contains the system when set
		filter = new IndexModel.SubmissionSearchRequest { System = "NES" };
		Assert.AreEqual(1, filter.Systems.Count);
		Assert.AreEqual("NES", filter.Systems.First());
	}

	[TestMethod]
	public void SubmissionSearchRequest_GameIds_ReturnsCorrectCollection()
	{
		// Empty when GameId is null or empty
		ISubmissionFilter filter = new IndexModel.SubmissionSearchRequest { GameId = null };
		Assert.IsFalse(filter.GameIds.Any());

		filter = new IndexModel.SubmissionSearchRequest { GameId = "" };
		Assert.IsFalse(filter.GameIds.Any());

		filter = new IndexModel.SubmissionSearchRequest { GameId = "   " };
		Assert.IsFalse(filter.GameIds.Any());

		// Empty when GameId is not a valid integer
		filter = new IndexModel.SubmissionSearchRequest { GameId = "abc" };
		Assert.IsFalse(filter.GameIds.Any());

		filter = new IndexModel.SubmissionSearchRequest { GameId = "12.34" };
		Assert.IsFalse(filter.GameIds.Any());

		// Contains game ID when valid integer
		filter = new IndexModel.SubmissionSearchRequest { GameId = "123" };
		Assert.AreEqual(1, filter.GameIds.Count);
		Assert.AreEqual(123, filter.GameIds.First());
	}
}
