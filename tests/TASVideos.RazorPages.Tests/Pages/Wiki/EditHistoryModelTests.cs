using TASVideos.Pages.Wiki;

namespace TASVideos.RazorPages.Tests.Pages.Wiki;

[TestClass]
public class EditHistoryModelTests : BasePageModelTests
{
	private readonly EditHistoryModel _model;

	public EditHistoryModelTests()
	{
		_model = new EditHistoryModel(_db)
		{
			PageContext = TestPageContext()
		};
	}

	[TestMethod]
	public async Task OnGet_NoUserName_ReturnsEmptyHistory()
	{
		_model.UserName = "";

		await _model.OnGet();

		Assert.AreEqual(0, _model.History.Count());
		Assert.AreEqual(0, _model.History.RowCount);
	}

	[TestMethod]
	public async Task OnGet_UserDoesNotExist_ReturnsEmptyHistory()
	{
		_model.UserName = "NonExistentUser";

		await _model.OnGet();

		Assert.AreEqual(0, _model.History.Count());
		Assert.AreEqual(0, _model.History.RowCount);
	}

	[TestMethod]
	public async Task OnGet_UserHasNonDeletedPages_ReturnsHistory()
	{
		const string userName = "TestUser";
		const string pageName = "TestPage";
		var author = _db.AddUser(userName).Entity;
		_db.WikiPages.Add(new WikiPage
		{
			Id = 1,
			PageName = pageName,
			Markup = "Test content",
			Revision = 1,
			IsDeleted = false,
			Author = author,
			ChildId = null,
			MinorEdit = false,
			RevisionMessage = "Initial revision"
		});
		await _db.SaveChangesAsync();

		_model.UserName = userName;

		await _model.OnGet();

		Assert.AreEqual(1, _model.History.Count());
		Assert.AreEqual(1, _model.History.RowCount);
		var historyEntry = _model.History.First();

		Assert.AreEqual(1, historyEntry.Revision);
		Assert.AreEqual(pageName, historyEntry.PageName);
		Assert.IsFalse(historyEntry.MinorEdit);
		Assert.AreEqual("Initial revision", historyEntry.RevisionMessage);
	}

	[TestMethod]
	public async Task OnGet_UserHasDeletedPages_ExcludesFromHistory()
	{
		const string userName = "TestUser";
		var author = _db.AddUser(userName).Entity;
		_db.WikiPages.Add(new WikiPage
		{
			Id = 1,
			PageName = "DeletedPage",
			Markup = "Deleted content",
			Revision = 1,
			IsDeleted = true,
			AuthorId = author.Id,
			Author = author,
			ChildId = null
		});
		await _db.SaveChangesAsync();

		_model.UserName = userName;

		await _model.OnGet();

		Assert.AreEqual(0, _model.History.Count());
		Assert.AreEqual(0, _model.History.RowCount);
	}

	[TestMethod]
	public async Task OnGet_MultipleUsersWithPages_ReturnsOnlySpecificUserHistory()
	{
		const string targetUser = "TargetUser";
		const string otherUser = "OtherUser";

		var targetAuthor = _db.AddUser(targetUser).Entity;
		var otherAuthor = _db.AddUser(otherUser).Entity;

		_db.WikiPages.AddRange(
			new WikiPage
			{
				Id = 1,
				PageName = "TargetPage1",
				Markup = "Target content 1",
				Revision = 1,
				IsDeleted = false,
				AuthorId = targetAuthor.Id,
				Author = targetAuthor
			},
			new WikiPage
			{
				Id = 2,
				PageName = "OtherPage1",
				Markup = "Other content 1",
				Revision = 1,
				IsDeleted = false,
				AuthorId = otherAuthor.Id,
				Author = otherAuthor
			},
			new WikiPage
			{
				Id = 3,
				PageName = "TargetPage2",
				Markup = "Target content 2",
				Revision = 1,
				IsDeleted = false,
				AuthorId = targetAuthor.Id,
				Author = targetAuthor
			});
		await _db.SaveChangesAsync();

		_model.UserName = targetUser;

		await _model.OnGet();

		Assert.AreEqual(2, _model.History.Count());
		Assert.AreEqual(2, _model.History.RowCount);

		var pageNames = _model.History.Select(h => h.PageName).ToList();
		Assert.IsTrue(pageNames.Contains("TargetPage1"));
		Assert.IsTrue(pageNames.Contains("TargetPage2"));
		Assert.IsFalse(pageNames.Contains("OtherPage1"));
	}

	[TestMethod]
	public async Task OnGet_MultipleRevisions_OrdersByMostRecent()
	{
		const string userName = "TestUser";
		var author = _db.AddUser(userName).Entity;
		var baseTime = DateTime.UtcNow.AddHours(-10);

		_db.WikiPages.AddRange(
			new WikiPage
			{
				Id = 1,
				PageName = "OlderPage",
				Markup = "Older content",
				Revision = 1,
				IsDeleted = false,
				AuthorId = author.Id,
				Author = author,
				ChildId = null,
				CreateTimestamp = baseTime.AddHours(-2)
			},
			new WikiPage
			{
				Id = 2,
				PageName = "NewerPage",
				Markup = "Newer content",
				Revision = 1,
				IsDeleted = false,
				AuthorId = author.Id,
				Author = author,
				ChildId = null,
				CreateTimestamp = baseTime
			});
		await _db.SaveChangesAsync();

		_model.UserName = userName;

		await _model.OnGet();

		Assert.AreEqual(2, _model.History.Count());

		var historyList = _model.History.ToList();

		// Should be ordered by most recent first
		Assert.AreEqual("NewerPage", historyList[0].PageName);
		Assert.AreEqual("OlderPage", historyList[1].PageName);
	}

	[TestMethod]
	public async Task OnGet_MinorEditFlag_PreservesMinorEditStatus()
	{
		const string userName = "TestUser";
		var author = _db.AddUser(userName).Entity;

		_db.WikiPages.AddRange(
			new WikiPage
			{
				Id = 1,
				PageName = "MajorEdit",
				Markup = "Major content",
				Revision = 1,
				IsDeleted = false,
				AuthorId = author.Id,
				Author = author,
				ChildId = null,
				MinorEdit = false
			},
			new WikiPage
			{
				Id = 2,
				PageName = "MinorEdit",
				Markup = "Minor content",
				Revision = 1,
				IsDeleted = false,
				AuthorId = author.Id,
				Author = author,
				ChildId = null,
				MinorEdit = true
			});
		await _db.SaveChangesAsync();

		_model.UserName = userName;

		await _model.OnGet();

		Assert.AreEqual(2, _model.History.Count());

		var majorEdit = _model.History.FirstOrDefault(h => h.PageName == "MajorEdit");
		var minorEdit = _model.History.FirstOrDefault(h => h.PageName == "MinorEdit");

		Assert.IsNotNull(majorEdit);
		Assert.IsNotNull(minorEdit);
		Assert.IsFalse(majorEdit.MinorEdit);
		Assert.IsTrue(minorEdit.MinorEdit);
	}

	[TestMethod]
	public async Task OnGet_RevisionMessages_PreservesRevisionMessages()
	{
		const string userName = "TestUser";
		var author = _db.AddUser(userName).Entity;

		_db.WikiPages.AddRange(
			new WikiPage
			{
				Id = 1,
				PageName = "PageWithMessage",
				Markup = "Content with message",
				Revision = 1,
				IsDeleted = false,
				AuthorId = author.Id,
				Author = author,
				ChildId = null,
				RevisionMessage = "Added new content"
			},
			new WikiPage
			{
				Id = 2,
				PageName = "PageWithoutMessage",
				Markup = "Content without message",
				Revision = 1,
				IsDeleted = false,
				AuthorId = author.Id,
				Author = author,
				ChildId = null,
				RevisionMessage = null
			});
		await _db.SaveChangesAsync();

		_model.UserName = userName;

		await _model.OnGet();

		Assert.AreEqual(2, _model.History.Count());

		var pageWithMessage = _model.History.FirstOrDefault(h => h.PageName == "PageWithMessage");
		var pageWithoutMessage = _model.History.FirstOrDefault(h => h.PageName == "PageWithoutMessage");

		Assert.IsNotNull(pageWithMessage);
		Assert.IsNotNull(pageWithoutMessage);
		Assert.AreEqual("Added new content", pageWithMessage.RevisionMessage);
		Assert.IsNull(pageWithoutMessage.RevisionMessage);
	}

	[TestMethod]
	public async Task OnGet_PagingModel_DefaultsToCorrectValues()
	{
		const string userName = "TestUser";
		var author = _db.AddUser(userName).Entity;

		for (int i = 1; i <= 30; i++)
		{
			_db.WikiPages.Add(new WikiPage
			{
				Id = i,
				PageName = $"Page{i}",
				Markup = $"Content {i}",
				Revision = 1,
				IsDeleted = false,
				AuthorId = author.Id,
				Author = author,
				ChildId = null
			});
		}

		await _db.SaveChangesAsync();

		_model.UserName = userName;
		_model.Paging = new() { CurrentPage = 1, PageSize = 25 };

		await _model.OnGet();

		Assert.AreEqual(25, _model.History.Count());
		Assert.AreEqual(30, _model.History.RowCount);
	}
}
