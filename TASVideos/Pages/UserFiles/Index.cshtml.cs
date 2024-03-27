using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Pages.UserFiles.Models;

namespace TASVideos.Pages.UserFiles;

[AllowAnonymous]
public class IndexModel(
	ApplicationDbContext db,
	ExternalMediaPublisher publisher)
	: BasePageModel
{
	public List<UserWithMovie> UsersWithMovies { get; set; } = [];
	public List<UserMovieListModel> LatestMovies { get; set; } = [];
	public List<GameWithMovie> GamesWithMovies { get; set; } = [];
	public List<UncatalogedViewModel> UncatalogedFiles { get; set; } = [];

	public async Task OnGet()
	{
		UsersWithMovies = await db.UserFiles
			.ThatArePublic()
			.GroupBy(gkey => gkey.Author!.UserName, gvalue => gvalue.UploadTimestamp)
			.Select(uf => new UserWithMovie(uf.Key, uf.Max()))
			.ToListAsync();
		LatestMovies = await db.UserFiles
			.ThatArePublic()
			.ByRecentlyUploaded()
			.ToUserMovieListModel()
			.Take(10)
			.ToListAsync();
		GamesWithMovies = await db.Games
			.Where(g => g.UserFiles.Any(uf => !uf.Hidden))
			.OrderBy(g => g.DisplayName)
			.Select(g => new GameWithMovie(
				g.Id,
				g.DisplayName,
				g.UserFiles.Select(uf => uf.UploadTimestamp).ToList()))
			.ToListAsync();
		UncatalogedFiles = await db.UserFiles
			.Where(uf => uf.GameId == null)
			.Where(uf => !uf.Hidden)
			.ToUnCatalogedModel()
			.Take(25)
			.ToListAsync();
	}

	public async Task<IActionResult> OnPostDelete(long fileId)
	{
		var userFile = await db.UserFiles.SingleOrDefaultAsync(u => u.Id == fileId);
		if (userFile is not null)
		{
			if (User.GetUserId() == userFile.AuthorId
				|| User.Has(PermissionTo.EditUserFiles))
			{
				db.UserFiles.Remove(userFile);

				await ConcurrentSave(db, $"{userFile.FileName} deleted", $"Unable to delete {userFile.FileName}");
			}
		}

		return BaseReturnUrlRedirect();
	}

	public async Task<IActionResult> OnPostComment(long fileId, string comment)
	{
		if (User.Has(PermissionTo.CreateForumPosts)
			&& !string.IsNullOrWhiteSpace(comment))
		{
			var userFile = await db.UserFiles.SingleOrDefaultAsync(u => u.Id == fileId);
			if (userFile is not null)
			{
				db.UserFileComments.Add(new UserFileComment
				{
					UserFileId = fileId,
					Text = comment,
					UserId = User.GetUserId(),
					Ip = IpAddress,
					CreationTimeStamp = DateTime.UtcNow
				});

				await db.SaveChangesAsync();
				await publisher.SendUserFile(
					userFile.Hidden,
					$"New user file comment by {User.Name()}",
					$"New [user file]({{0}}) comment by {User.Name()}",
					$"UserFiles/Info/{fileId}",
					$"{userFile.Title}");
			}
		}

		return BaseReturnUrlRedirect();
	}

	public async Task<IActionResult> OnPostEditComment(long commentId, string comment)
	{
		if (User.Has(PermissionTo.CreateForumPosts)
			&& !string.IsNullOrWhiteSpace(comment))
		{
			var fileComment = await db.UserFileComments
				.Include(c => c.UserFile)
				.SingleOrDefaultAsync(u => u.Id == commentId);

			if (fileComment is not null)
			{
				fileComment.Text = comment;

				var result = await ConcurrentSave(db, "Comment edited", "Unable to edit comment");
				if (result)
				{
					await publisher.SendUserFile(
						fileComment.UserFile!.Hidden,
						$"User file comment edited by {User.Name()}",
						$"[User file]({{0}}) comment edited by {User.Name()}",
						$"UserFiles/Info/{fileComment.UserFile.Id}",
						$"{fileComment.UserFile!.Title}");
				}
			}
		}

		return BaseReturnUrlRedirect();
	}

	public async Task<IActionResult> OnPostDeleteComment(long commentId)
	{
		if (User.Has(PermissionTo.CreateForumPosts))
		{
			var fileComment = await db.UserFileComments
				.Include(c => c.UserFile)
				.Include(c => c.User)
				.SingleOrDefaultAsync(u => u.Id == commentId);

			if (fileComment is not null)
			{
				db.UserFileComments.Remove(fileComment);
				var result = await ConcurrentSave(db, "Comment deleted", "Unable to delete comment");
				if (result)
				{
					await publisher.SendUserFile(
						fileComment.UserFile!.Hidden,
						$"User file comment DELETED by {User.Name()}",
						$"[User file]({{0}}) comment DELETED by {User.Name()}",
						$"UserFiles/Info/{fileComment.UserFile.Id}",
						$"{fileComment.UserFile!.Title}");
				}
			}
		}

		return BaseReturnUrlRedirect();
	}

	public record UserWithMovie(string UserName, DateTime Latest);

	public record GameWithMovie(int GameId, string GameName, List<DateTime> Dates)
	{
		public DateTime Latest => Dates.Max();
	}
}
