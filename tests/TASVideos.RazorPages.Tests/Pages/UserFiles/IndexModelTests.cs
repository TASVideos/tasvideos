using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Pages.UserFiles;
using TASVideos.Services;
using TASVideos.Tests.Base;

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
			Class = UserFileClass.Movie,
			UploadTimestamp = DateTime.UtcNow.AddDays(-1)
		};

		var file2 = new UserFile
		{
			Id = 2,
			FileName = "user2-movie.bk2",
			Title = "User2 Movie",
			Author = user2,
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
		var game1 = _db.AddGame("Game A").Entity;
		var game2 = _db.AddGame("Game B").Entity;
		var user = _db.AddUser("TestUser").Entity;
		_db.UserFiles.AddRange(
			new UserFile
			{
				Id = 1,
				FileName = "game1-movie.bk2",
				Title = "Game 1 Movie",
				Author = user,
				Game = game1,
				Class = UserFileClass.Movie
			},
			new UserFile
			{
				Id = 2,
				FileName = "game2-movie.bk2",
				Title = "Game 2 Movie",
				Author = user,
				Game = game2,
				Class = UserFileClass.Movie
			});
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
		var game = _db.AddGame("Test Game").Entity;
		var user = _db.AddUser("TestUser").Entity;
		var catalogedFile = new UserFile
		{
			Id = 1,
			FileName = "cataloged.bk2",
			Title = "Cataloged Movie",
			Author = user,
			Game = game, // Has a game
			Hidden = false,
			Class = UserFileClass.Movie
		};

		var uncatalogedFile = new UserFile
		{
			Id = 2,
			FileName = "uncataloged.bk2",
			Title = "Uncataloged Movie",
			Author = user,
			GameId = null, // No game
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
		var userFile = _db.AddPublicUserFile(author).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, author, []);

		var result = await _page.OnPostDelete(userFile.Id);

		AssertRedirectHome(result);
		var deletedFile = await _db.UserFiles.FindAsync(userFile.Id);
		Assert.IsNull(deletedFile);
	}

	[TestMethod]
	public async Task OnPostDelete_WithEditPermission_DeletesFile()
	{
		var author = _db.AddUser("Author").Entity;
		var editor = _db.AddUser("Editor").Entity;
		var userFile = _db.AddPublicUserFile(author).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, editor, [PermissionTo.EditUserFiles]);

		var result = await _page.OnPostDelete(userFile.Id);

		AssertRedirectHome(result);
		var deletedFile = await _db.UserFiles.FindAsync(userFile.Id);
		Assert.IsNull(deletedFile);
	}

	[TestMethod]
	public async Task OnPostDelete_WithoutPermission_DoesNotDelete()
	{
		var author = _db.AddUser("Author").Entity;
		var otherUser = _db.AddUser("OtherUser").Entity;
		var userFile = _db.AddPublicUserFile(author).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, otherUser, []);

		var result = await _page.OnPostDelete(userFile.Id);

		AssertRedirectHome(result);
		var fileStillExists = await _db.UserFiles.FindAsync(userFile.Id);
		Assert.IsNotNull(fileStillExists);
	}

	[TestMethod]
	public async Task OnPostDelete_NonExistentFile_ReturnsRedirect()
	{
		var result = await _page.OnPostDelete(999); // Non-existent ID
		AssertRedirectHome(result);
	}

	[TestMethod]
	public async Task OnPostComment_WithPermission_CreatesComment()
	{
		var author = _db.AddUser("Author").Entity;
		var commenter = _db.AddUser("Commenter").Entity;
		var userFile = _db.AddPublicUserFile(author).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, commenter, [PermissionTo.CreateForumPosts]);
		_page.PageContext.HttpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

		var result = await _page.OnPostComment(userFile.Id, "Great movie!");

		AssertRedirectHome(result);
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
		var userFile = _db.AddPublicUserFile(author).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, user, []); // No CreateForumPosts permission

		var result = await _page.OnPostComment(userFile.Id, "Great movie!");

		AssertRedirectHome(result);
		var commentCount = await _db.UserFileComments.CountAsync(c => c.UserFileId == userFile.Id);
		Assert.AreEqual(0, commentCount);
	}

	[TestMethod]
	public async Task OnPostComment_EmptyComment_DoesNotCreateComment()
	{
		var user = _db.AddUser("User").Entity;
		var userFile = _db.AddPublicUserFile(user).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, user, [PermissionTo.CreateForumPosts]);

		var result = await _page.OnPostComment(userFile.Id, "");

		AssertRedirectHome(result);
		var commentCount = await _db.UserFileComments.CountAsync(c => c.UserFileId == userFile.Id);
		Assert.AreEqual(0, commentCount);
	}

	[TestMethod]
	public async Task OnPostEditComment_AsOwner_UpdatesComment()
	{
		var user = _db.AddUser("User").Entity;
		var userFile = _db.AddPublicUserFile(user).Entity;
		var comment = _db.UserFileComments.Add(new UserFileComment
		{
			UserFile = userFile,
			Text = "Original comment",
			User = user
		}).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, user, [PermissionTo.CreateForumPosts]);

		var result = await _page.OnPostEditComment(comment.Id, "Updated comment");

		AssertRedirectHome(result);
		var updatedComment = await _db.UserFileComments.FindAsync(comment.Id);
		Assert.IsNotNull(updatedComment);
		Assert.AreEqual("Updated comment", updatedComment.Text);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPostEditComment_WithEditPermission_UpdatesComment()
	{
		var author = _db.AddUser("Author").Entity;
		var editor = _db.AddUser("Editor").Entity;
		var userFile = _db.AddPublicUserFile(author).Entity;
		var comment = _db.UserFileComments.Add(new UserFileComment
		{
			UserFile = userFile,
			Text = "Original comment",
			User = author
		}).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, editor, [PermissionTo.CreateForumPosts, PermissionTo.EditUsersForumPosts]);

		var result = await _page.OnPostEditComment(comment.Id, "Updated by editor");

		AssertRedirectHome(result);
		var updatedComment = await _db.UserFileComments.FindAsync(comment.Id);
		Assert.IsNotNull(updatedComment);
		Assert.AreEqual("Updated by editor", updatedComment.Text);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPostEditComment_WithoutPermission_DoesNotUpdate()
	{
		var author = _db.AddUser("Author").Entity;
		var otherUser = _db.AddUser("OtherUser").Entity;
		var userFile = _db.AddPublicUserFile(author).Entity;
		var comment = _db.UserFileComments.Add(new UserFileComment
		{
			UserFile = userFile,
			Text = "Original comment",
			User = author
		}).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, otherUser, [PermissionTo.CreateForumPosts]); // Has CreateForumPosts but not edit permission

		var result = await _page.OnPostEditComment(comment.Id, "Malicious edit attempt");

		AssertRedirectHome(result);
		var unchangedComment = await _db.UserFileComments.FindAsync(comment.Id);
		Assert.IsNotNull(unchangedComment);
		Assert.AreEqual("Original comment", unchangedComment.Text); // Should remain unchanged
		await _publisher.DidNotReceive().Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPostEditComment_NonExistentComment_DoesNotThrow()
	{
		var user = _db.AddUser("User").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.CreateForumPosts]);

		var result = await _page.OnPostEditComment(999, "Some comment"); // Non-existent ID

		AssertRedirectHome(result);
		await _publisher.DidNotReceive().Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPostEditComment_EmptyComment_DoesNotUpdate()
	{
		var user = _db.AddUser("User").Entity;
		var userFile = _db.UserFiles.Add(new UserFile
		{
			FileName = "test.bk2",
			Title = "Test Movie",
			Author = user
		}).Entity;

		var comment = _db.UserFileComments.Add(new UserFileComment
		{
			UserFileId = userFile.Id,
			Text = "Original comment",
			User = user
		}).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, user, [PermissionTo.CreateForumPosts]);

		var result = await _page.OnPostEditComment(comment.Id, "");

		AssertRedirectHome(result);
		var unchangedComment = await _db.UserFileComments.FindAsync(comment.Id);
		Assert.IsNotNull(unchangedComment);
		Assert.AreEqual("Original comment", unchangedComment.Text); // Should remain unchanged
		await _publisher.DidNotReceive().Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPostDeleteComment_AsOwner_DeletesComment()
	{
		var user = _db.AddUser("User").Entity;
		var userFile = _db.UserFiles.Add(new UserFile
		{
			FileName = "test.bk2",
			Title = "Test Movie",
			Author = user
		}).Entity;

		var comment = _db.UserFileComments.Add(new UserFileComment
		{
			UserFile = userFile,
			Text = "Comment to delete",
			User = user
		}).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, user, [PermissionTo.CreateForumPosts]);

		var result = await _page.OnPostDeleteComment(comment.Id);

		AssertRedirectHome(result);
		var deletedComment = await _db.UserFileComments.FindAsync(comment.Id);
		Assert.IsNull(deletedComment);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPostDeleteComment_WithDeletePermission_DeletesComment()
	{
		var author = _db.AddUser("Author").Entity;
		var moderator = _db.AddUser("Moderator").Entity;
		var userFile = _db.UserFiles.Add(new UserFile
		{
			FileName = "test.bk2",
			Title = "Test Movie",
			Author = author
		}).Entity;

		var comment = _db.UserFileComments.Add(new UserFileComment
		{
			UserFile = userFile,
			Text = "Comment to delete",
			User = author
		}).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, moderator, [PermissionTo.CreateForumPosts, PermissionTo.DeleteForumPosts]);

		var result = await _page.OnPostDeleteComment(comment.Id);

		AssertRedirectHome(result);
		var deletedComment = await _db.UserFileComments.FindAsync(comment.Id);
		Assert.IsNull(deletedComment);
		await _publisher.Received(1).Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPostDeleteComment_WithoutPermission_DoesNotDelete()
	{
		var author = _db.AddUser("Author").Entity;
		var otherUser = _db.AddUser("OtherUser").Entity;
		var userFile = _db.AddPublicUserFile(author).Entity;

		var comment = _db.UserFileComments.Add(new UserFileComment
		{
			UserFile = userFile,
			Text = "Comment to delete",
			User = author
		}).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, otherUser, [PermissionTo.CreateForumPosts]); // Has CreateForumPosts but not delete permission

		var result = await _page.OnPostDeleteComment(comment.Id);

		AssertRedirectHome(result);
		var commentStillExists = await _db.UserFileComments.FindAsync(comment.Id);
		Assert.IsNotNull(commentStillExists);
		Assert.AreEqual("Comment to delete", commentStillExists.Text);
		await _publisher.DidNotReceive().Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPostDeleteComment_NonExistentComment_DoesNotThrow()
	{
		var user = _db.AddUser("User").Entity;
		AddAuthenticatedUser(_page, user, [PermissionTo.CreateForumPosts]);

		var result = await _page.OnPostDeleteComment(999); // Non-existent ID

		AssertRedirectHome(result);
		await _publisher.DidNotReceive().Send(Arg.Any<Post>());
	}

	[TestMethod]
	public async Task OnPostDeleteComment_WithoutCreateForumPostsPermission_DoesNotDelete()
	{
		var user = _db.AddUser("User").Entity;
		var userFile = _db.AddPublicUserFile(user).Entity;

		var comment = _db.UserFileComments.Add(new UserFileComment
		{
			UserFile = userFile,
			Text = "Comment to delete",
			User = user
		}).Entity;
		await _db.SaveChangesAsync();

		AddAuthenticatedUser(_page, user, []); // No permissions at all

		var result = await _page.OnPostDeleteComment(comment.Id);

		AssertRedirectHome(result);
		var commentStillExists = await _db.UserFileComments.FindAsync(comment.Id);
		Assert.IsNotNull(commentStillExists);
		await _publisher.DidNotReceive().Send(Arg.Any<Post>());
	}

	[TestMethod]
	public void AllowsAnonymousUsers() => AssertAllowsAnonymousUsers(typeof(IndexModel));
}
