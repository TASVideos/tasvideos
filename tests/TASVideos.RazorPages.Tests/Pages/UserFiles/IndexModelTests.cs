using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Pages.UserFiles;
using TASVideos.Services;
using TASVideos.Tests.Base;
using static TASVideos.RazorPages.Tests.RazorTestHelpers;

namespace TASVideos.RazorPages.Tests.Pages.UserFiles;

[TestClass]
public class IndexModelTests : TestDbBase
{
	private readonly IExternalMediaPublisher _publisher;
	private readonly IndexModel _page;

	public IndexModelTests()
	{
		_publisher = Substitute.For<IExternalMediaPublisher>();
		_page = new IndexModel(_db, _publisher);
	}

	[TestMethod]
	public async Task OnGet_LoadsUsersWithMovies()
	{
		var user1 = _db.AddUser("User1").Entity;
		var user2 = _db.AddUser("User2").Entity;

		var file1 = new UserFile
		{
			Id = 1,
			FileName = "user1-movie.bk2",
			Title = "User1 Movie",
			Author = user1,
			Hidden = false,
			Class = UserFileClass.Movie,
			UploadTimestamp = DateTime.UtcNow.AddDays(-1)
		};

		var file2 = new UserFile
		{
			Id = 2,
			FileName = "user2-movie.bk2",
			Title = "User2 Movie",
			Author = user2,
			Hidden = false,
			Class = UserFileClass.Movie,
			UploadTimestamp = DateTime.UtcNow.AddDays(-2)
		};

		_db.UserFiles.AddRange(file1, file2);
		await _db.SaveChangesAsync();

		await _page.OnGet();

		Assert.AreEqual(2, _page.UsersWithMovies.Count);
		var userNames = _page.UsersWithMovies.Select(u => u.UserName).ToList();
		Assert.IsTrue(userNames.Contains("User1"));
		Assert.IsTrue(userNames.Contains("User2"));
	}

	[TestMethod]
	public async Task OnGet_LoadsLatestMovies()
	{
		var user = _db.AddUser("TestUser").Entity;

		// Create 12 files to test the limit of 10
		for (int i = 1; i <= 12; i++)
		{
			_db.UserFiles.Add(new UserFile
			{
				Id = i,
				FileName = $"movie-{i}.bk2",
				Title = $"Movie {i}",
				Author = user,
				Hidden = false,
				Class = UserFileClass.Movie,
				UploadTimestamp = DateTime.UtcNow.AddDays(-i)
			});
		}

		await _db.SaveChangesAsync();

		await _page.OnGet();

		Assert.AreEqual(10, _page.LatestMovies.Count); // Should limit to 10
		Assert.AreEqual("Movie 1", _page.LatestMovies.First().Title); // Most recent first
	}

	[TestMethod]
	public async Task OnGet_LoadsGamesWithMovies()
	{
		var game1 = new Game { Id = 1, DisplayName = "Game A" };
		var game2 = new Game { Id = 2, DisplayName = "Game B" };
		_db.Games.AddRange(game1, game2);
		var user = _db.AddUser("TestUser").Entity;
		var file1 = new UserFile
		{
			Id = 1,
			FileName = "game1-movie.bk2",
			Title = "Game 1 Movie",
			Author = user,
			GameId = game1.Id,
			Hidden = false,
			Class = UserFileClass.Movie
		};

		var file2 = new UserFile
		{
			Id = 2,
			FileName = "game2-movie.bk2",
			Title = "Game 2 Movie",
			Author = user,
			GameId = game2.Id,
			Hidden = false,
			Class = UserFileClass.Movie
		};

		_db.UserFiles.AddRange(file1, file2);
		await _db.SaveChangesAsync();

		await _page.OnGet();

		Assert.AreEqual(2, _page.GamesWithMovies.Count);

		// Should be ordered by game name
		Assert.AreEqual("Game A", _page.GamesWithMovies.First().GameName);
		Assert.AreEqual("Game B", _page.GamesWithMovies.Last().GameName);
	}

	[TestMethod]
	public async Task OnGet_LoadsUncatalogedFiles()
	{
		var game = new Game { Id = 1, DisplayName = "Test Game" };
		_db.Games.Add(game);
		var user = _db.AddUser("TestUser").Entity;
		var catalogedFile = new UserFile
		{
			Id = 1,
			FileName = "cataloged.bk2",
			Title = "Cataloged Movie",
			Author = user,
			GameId = game.Id, // Has a game ID
			Hidden = false,
			Class = UserFileClass.Movie
		};

		var uncatalogedFile = new UserFile
		{
			Id = 2,
			FileName = "uncataloged.bk2",
			Title = "Uncataloged Movie",
			Author = user,
			GameId = null, // No game ID
			Hidden = false,
			Class = UserFileClass.Movie
		};

		_db.UserFiles.AddRange(catalogedFile, uncatalogedFile);
		await _db.SaveChangesAsync();

		await _page.OnGet();

		Assert.AreEqual(1, _page.UncatalogedFiles.Count);
		Assert.AreEqual("uncataloged.bk2", _page.UncatalogedFiles.First().FileName);
	}

	[TestMethod]
	public async Task OnGet_OnlyIncludesPublicFiles()
	{
		var user = _db.AddUser("TestUser").Entity;
		var publicFile = new UserFile
		{
			Id = 1,
			FileName = "public.bk2",
			Title = "Public Movie",
			Author = user,
			Hidden = false,
			Class = UserFileClass.Movie
		};

		var hiddenFile = new UserFile
		{
			Id = 2,
			FileName = "hidden.bk2",
			Title = "Hidden Movie",
			Author = user,
			Hidden = true,
			Class = UserFileClass.Movie
		};

		_db.UserFiles.AddRange(publicFile, hiddenFile);
		await _db.SaveChangesAsync();

		await _page.OnGet();

		Assert.AreEqual(1, _page.UsersWithMovies.Count);
		Assert.AreEqual(1, _page.LatestMovies.Count);
		Assert.AreEqual("Public Movie", _page.LatestMovies.First().Title);
	}

	[TestMethod]
	public async Task OnPostDelete_AsAuthor_DeletesFile()
	{
		var author = _db.AddUser("Author").Entity;
		var userFile = new UserFile
		{
			Id = 1,
			FileName = "test.bk2",
			Title = "Test Movie",
			Author = author,
			Class = UserFileClass.Movie
		};

		_db.UserFiles.Add(userFile);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, author, []);

		var result = await _page.OnPostDelete(userFile.Id);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var deletedFile = await _db.UserFiles.FindAsync(userFile.Id);
		Assert.IsNull(deletedFile);
	}

	[TestMethod]
	public async Task OnPostDelete_WithEditPermission_DeletesFile()
	{
		var author = _db.AddUser("Author").Entity;
		var editor = _db.AddUser("Editor").Entity;
		var userFile = new UserFile
		{
			Id = 1,
			FileName = "test.bk2",
			Title = "Test Movie",
			Author = author,
			Class = UserFileClass.Movie
		};

		_db.UserFiles.Add(userFile);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, editor, [PermissionTo.EditUserFiles]);

		var result = await _page.OnPostDelete(userFile.Id);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var deletedFile = await _db.UserFiles.FindAsync(userFile.Id);
		Assert.IsNull(deletedFile);
	}

	[TestMethod]
	public async Task OnPostDelete_WithoutPermission_DoesNotDelete()
	{
		var author = _db.AddUser("Author").Entity;
		var otherUser = _db.AddUser("OtherUser").Entity;
		var userFile = _db.UserFiles.Add(new UserFile
		{
			Id = 1,
			FileName = "test.bk2",
			Title = "Test Movie",
			Author = author,
			Class = UserFileClass.Movie
		}).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, otherUser, []);

		var result = await _page.OnPostDelete(userFile.Id);

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var fileStillExists = await _db.UserFiles.FindAsync(userFile.Id);
		Assert.IsNotNull(fileStillExists);
	}

	[TestMethod]
	public async Task OnPostDelete_NonExistentFile_ReturnsRedirect()
	{
		var result = await _page.OnPostDelete(999); // Non-existent ID

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
	}

	[TestMethod]
	public async Task OnPostComment_WithPermission_CreatesComment()
	{
		var author = _db.AddUser("Author").Entity;
		var commenter = _db.AddUser("Commenter").Entity;
		var userFile = new UserFile
		{
			Id = 1,
			FileName = "test.bk2",
			Title = "Test Movie",
			Author = author,
			Hidden = false,
			Class = UserFileClass.Movie
		};

		_db.UserFiles.Add(userFile);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, commenter, [PermissionTo.CreateForumPosts]);
		_page.PageContext.HttpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

		var result = await _page.OnPostComment(userFile.Id, "Great movie!");

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var comment = await _db.UserFileComments.FirstOrDefaultAsync(c => c.UserFileId == userFile.Id);
		Assert.IsNotNull(comment);
		Assert.AreEqual("Great movie!", comment.Text);
		Assert.AreEqual(commenter.Id, comment.UserId);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPostComment_WithoutPermission_DoesNotCreateComment()
	{
		var author = _db.AddUser("Author").Entity;
		var user = _db.AddUser("User").Entity;
		var userFile = new UserFile
		{
			Id = 1,
			FileName = "test.bk2",
			Title = "Test Movie",
			Author = author,
			Class = UserFileClass.Movie
		};

		_db.UserFiles.Add(userFile);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, user, []); // No CreateForumPosts permission

		var result = await _page.OnPostComment(userFile.Id, "Great movie!");

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var commentCount = await _db.UserFileComments.CountAsync(c => c.UserFileId == userFile.Id);
		Assert.AreEqual(0, commentCount);
	}

	[TestMethod]
	public async Task OnPostComment_EmptyComment_DoesNotCreateComment()
	{
		var user = _db.AddUser("User").Entity;
		var userFile = new UserFile
		{
			Id = 1,
			FileName = "test.bk2",
			Title = "Test Movie",
			Author = user,
			Class = UserFileClass.Movie
		};

		_db.UserFiles.Add(userFile);
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, user, [PermissionTo.CreateForumPosts]);

		var result = await _page.OnPostComment(userFile.Id, "");

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var commentCount = await _db.UserFileComments.CountAsync(c => c.UserFileId == userFile.Id);
		Assert.AreEqual(0, commentCount);
	}

	[TestMethod]
	public async Task OnPostEditComment_WithPermission_UpdatesComment()
	{
		var user = _db.AddUser("User").Entity;
		var userFile = _db.UserFiles.Add(new UserFile
		{
			Id = 1,
			FileName = "test.bk2",
			Title = "Test Movie",
			Author = user,
			Hidden = false,
			Class = UserFileClass.Movie
		}).Entity;

		var comment = _db.UserFileComments.Add(new UserFileComment
		{
			Id = 1,
			UserFileId = userFile.Id,
			Text = "Original comment",
			User = user,
			CreationTimeStamp = DateTime.UtcNow
		}).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, user, [PermissionTo.CreateForumPosts]);

		var result = await _page.OnPostEditComment(comment.Id, "Updated comment");

		Assert.IsInstanceOfType<RedirectToPageResult>(result);
		var updatedComment = await _db.UserFileComments.FindAsync(comment.Id);
		Assert.IsNotNull(updatedComment);
		Assert.AreEqual("Updated comment", updatedComment.Text);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public void IndexModel_AllowsAnonymousUsers()
	{
		var indexModelType = typeof(IndexModel);
		var allowAnonymousAttribute = indexModelType.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: false);

		Assert.IsTrue(allowAnonymousAttribute.Length > 0);
	}
}
