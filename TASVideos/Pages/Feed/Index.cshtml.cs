using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;

namespace TASVideos.Pages.Feed;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class IndexModel(FeedDbContext db, UserNameCache userNameCache) : BasePageModel
{
	private readonly AchievementService _achievementService = new(db);

	[FromRoute]
	public string? PageRoute { get; set; }
	[FromRoute]
	public int? PostId { get; set; }
	[FromRoute]
	public int? CommentId { get; set; }

	public PageState PageState { get; set; } = new();

	private readonly HashSet<string> _allowedEmojis = ["👍", "👎", "😄", "😕", "❤️", "🎉", "🚀", "👀"];

	public async Task<IActionResult> OnGet()
	{
		var userId = User.GetUserId();

		PageState = new PageState
		{
			UserName = userId == -1 ? null : User.Name(),
			CanModEdit = User.Has(PermissionTo.EditUsersForumPosts),
			CanModDelete = User.Has(PermissionTo.DeleteForumPosts),
			Type = PageRoute == "My" ? "My" : PostId is null ? "Home" : CommentId is null ? "Post" : "Comment",
		};

		if (PageState.Type == "My" && userId == -1)
		{
			var returnUrl = Request.Path + Request.QueryString;
			return new RedirectToPageResult("/Account/Login", new { returnUrl });
		}

		if (PageState.Type == "Post")
		{
			PageState.Post = await GetSinglePost(PostId!.Value, userId);
		}
		else if (PageState.Type == "Comment")
		{
			PageState.CommentId = CommentId;
			var post = await GetSinglePost(PostId!.Value, userId);
			PageState.Post = post;

			if (PageState.Post is not null)
			{
				static bool Trim(CommentModel comment, int commentId)
				{
					if (comment.Id == commentId)
					{
						return true;
					}

					foreach (var reply in comment.Replies.ToList())
					{
						if (Trim(reply, commentId))
						{
							comment.Replies.Clear();
							comment.Replies.Add(reply);
							return true;
						}
					}

					comment.Replies.Clear();
					return false;
				}

				var commentList = PageState.Post.Comments.ToList();
				PageState.Post.Comments.Clear();
				foreach (var comment in commentList)
				{
					if (Trim(comment, PageState.CommentId!.Value))
					{
						PageState.Post.Comments = [comment];
						break;
					}
				}

				if (PageState.Post.Comments.Count == 0)
				{
					PageState.Post = null;
				}
			}
		}

		return Page();
	}

	public async Task<IActionResult> OnGetPosts(string filter, int? afterId)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		List<int> postIds;
		const int PageSize = 20;

		if (filter == "Hot")
		{
			PostOrderHot? afterPostScoreHot = null;
			if (afterId is not null)
			{
				afterPostScoreHot = await db.Posts
				   .Where(p => p.Id == afterId)
				   .SelectOrderHot()
				   .FirstOrDefaultAsync();
			}

			var query = db.Posts
				.Where(p => !p.IsDeleted)
				.SelectOrderHot();

			if (afterPostScoreHot is not null)
			{
				query = query.Where(p =>
					(p.IsSticky ? 1 : 0) < (afterPostScoreHot.IsSticky ? 1 : 0)
					|| ((p.IsSticky ? 1 : 0) == (afterPostScoreHot.IsSticky ? 1 : 0) && p.ScoreHot < afterPostScoreHot.ScoreHot)
					|| ((p.IsSticky ? 1 : 0) == (afterPostScoreHot.IsSticky ? 1 : 0) && p.ScoreHot == afterPostScoreHot.ScoreHot && p.Id < afterId));
			}

			postIds = await query
				.OrderByDescending(p => p.IsSticky)
				.ThenByDescending(p => p.ScoreHot)
				.ThenByDescending(p => p.Id)
				.Select(p => p.Id)
				.Take(PageSize)
				.ToListAsync();
		}
		else if (filter == "New")
		{
			DateTime? afterDate = null;
			if (afterId is not null)
			{
				afterDate = await db.Posts
				   .Where(p => p.Id == afterId)
				   .Select(p => p.Date)
				   .FirstOrDefaultAsync();
			}

			postIds = await db.Posts
				.Where(p => afterDate == null || p.Date < afterDate || (p.Date == afterDate && p.Id < afterId))
				.OrderByDescending(p => p.Date)
				.ThenByDescending(p => p.Id)
				.Select(p => p.Id)
				.Take(PageSize)
				.ToListAsync();
		}
		else if (filter == "Top")
		{
			int? afterScore = null;
			if (afterId is not null)
			{
				afterScore = await db.Posts
				   .Where(p => p.Id == afterId)
				   .Select(p => p.Votes.Sum(v => v.Value))
				   .FirstOrDefaultAsync();
			}

			postIds = await db.Posts
				.Where(p => afterScore == null || p.Votes.Sum(v => v.Value) < afterScore || (p.Votes.Sum(v => v.Value) == afterScore && p.Id < afterId))
				.OrderByDescending(p => p.Votes.Sum(v => v.Value))
				.ThenByDescending(p => p.Id)
				.Select(p => p.Id)
				.Take(PageSize)
				.ToListAsync();
		}
		else
		{
			return BadRequest("Invalid filter.");
		}

		var userId = User.GetUserId();

		var order = postIds
			.Select((id, index) => new { id, index })
			.ToDictionary(x => x.id, x => x.index);

		List<PostModel> posts = (await db.Posts
			.Include(p => p.Votes)
			.Include(p => p.Reactions)
			.Include(p => p.Comments)
				.ThenInclude(c => c.Votes)
			.Include(p => p.Comments)
				.ThenInclude(p => p.Reactions)
			.Where(p => postIds.Contains(p.Id))
			.ToListAsync())
			.OrderBy(obj => order[obj.Id])
			.SelectPostModel(userId)
			.ToList();

		foreach (var post in posts)
		{
			await AdjustPost(post, userId);
		}

		return new JsonResult(posts);
	}

	public async Task<IActionResult> OnGetMy()
	{
		var userId = User.GetUserId();
		if (userId == -1)
		{
			return new UnauthorizedObjectResult("Please log in.");
		}

		var posts = (await db.Posts
			.Include(p => p.Votes)
			.Include(p => p.Reactions)
			.Include(p => p.Comments)
				.ThenInclude(c => c.Votes)
			.Include(p => p.Comments)
				.ThenInclude(p => p.Reactions)
			.Where(p => p.UserId == userId)
			.Where(p => !p.IsDeleted)
			.ToListAsync())
			.SelectPostModel(userId)
			.ToList();

		foreach (var post in posts)
		{
			await AdjustPost(post, userId);
		}

		var commentPosts = (await db.Posts
			.Include(p => p.Votes)
			.Include(p => p.Reactions)
			.Include(p => p.Comments)
				.ThenInclude(c => c.Votes)
			.Include(p => p.Comments)
				.ThenInclude(p => p.Reactions)
			.Where(p => p.Comments.Any(c => c.UserId == userId && !c.IsDeleted))
			.ToListAsync())
			.SelectPostModel(userId)
			.ToList();

		List<CommentModel> comments = [];
		void AddComments(CommentModel comment)
		{
			if (comment.AuthorId == userId)
			{
				comments.Add(comment);
			}

			foreach (var reply in comment.Replies)
			{
				AddComments(reply);
			}
		}

		foreach (var commentPost in commentPosts)
		{
			await AdjustPost(commentPost, userId);

			foreach (var comment in commentPost.Comments)
			{
				AddComments(comment);
			}
		}

		var achievements = await db.Achievements
			.Where(a => a.UserId == userId)
			.OrderBy(a => a.Date)
			.ToListAsync();

		var unseenAchievements = achievements.Where(a => !a.HasSeen).ToList();
		foreach (var achievement in unseenAchievements)
		{
			achievement.HasSeen = true;
		}

		await db.SaveChangesAsync();

		foreach (var achievement in unseenAchievements)
		{
			achievement.HasSeen = false;
		}

		return new JsonResult(new
		{
			Posts = posts,
			Comments = comments,
			Achievements = achievements
		});
	}

	private async Task AdjustPost(PostModel post, int userId)
	{
		foreach (var comment in post.Comments)
		{
			AdjustComment(comment, post.IsDeleted);
		}

		if (post.Date > DateTime.UtcNow.AddHours(-1) && post.AuthorId != userId)
		{
			post.Score = null;
			post.ScoreHot = null;
		}

		if (post.IsDeleted)
		{
			post.Title = null;
			post.AuthorId = null;
			post.Author = null;
			post.AuthorAvatar = null;
			post.Content = "";
		}

		List<int> neededUserNames = post.Comments
			.Select(c => c.AuthorId)
			.Append(post.AuthorId)
			.Where(id => id != null)
			.Select(id => id!.Value)
			.ToList();

		var userDict = await userNameCache.ResolveUserIds(neededUserNames);

		if (post.AuthorId != null)
		{
			if (userDict.TryGetValue(post.AuthorId.Value, out var entry))
			{
				post.Author = entry.UserName;
				post.AuthorAvatar = entry.Avatar;
			}
			else
			{
				post.Author = "Unknown";
				post.AuthorAvatar = null;
			}
		}

		foreach (var comment in post.Comments)
		{
			if (comment.AuthorId != null)
			{
				if (userDict.TryGetValue(comment.AuthorId.Value, out var entry))
				{
					comment.Author = entry.UserName;
					comment.AuthorAvatar = entry.Avatar;
				}
				else
				{
					comment.Author = "Unknown";
					comment.AuthorAvatar = null;
				}
			}
		}

		post.Comments = post.Comments
			.ToNestedList()
			.RootComments()
			.ToList();
	}

	private void AdjustComment(CommentModel comment, bool isPostDeleted)
	{
		comment.ScoreWilson = null;

		if (isPostDeleted)
		{
			comment.PostTitle = null;
		}

		if (comment.IsDeleted)
		{
			comment.AuthorId = null;
			comment.Author = null;
			comment.AuthorAvatar = null;
			comment.Content = "";
		}
	}

	private async Task<PostModel?> GetSinglePost(int id, int userId)
	{
		var post = (await db.Posts
			.Include(p => p.Votes)
			.Include(p => p.Reactions)
			.Include(p => p.Comments)
				.ThenInclude(c => c.Votes)
			.Include(p => p.Comments)
				.ThenInclude(p => p.Reactions)
			.Where(p => p.Id == id)
			.ToListAsync())
			.SelectPostModel(userId)
			.FirstOrDefault();

		if (post is not null)
		{
			await AdjustPost(post, userId);
		}

		return post;
	}

	private async Task<CommentModel?> GetSingleComment(int commentId, int postId, int userId)
	{
		var commentPost = (await db.Posts
			.Include(p => p.Votes)
			.Include(p => p.Reactions)
			.Include(p => p.Comments)
				.ThenInclude(c => c.Votes)
			.Include(p => p.Comments)
				.ThenInclude(p => p.Reactions)
			.Where(p => p.Id == postId)
			.ToListAsync())
			.SelectPostModel(userId)
			.FirstOrDefault();

		if (commentPost is null)
		{
			return null;
		}

		await AdjustPost(commentPost, userId);

		CommentModel? FindComment(CommentModel comment)
		{
			if (comment.Id == commentId)
			{
				return comment;
			}

			foreach (var reply in comment.Replies)
			{
				var found = FindComment(reply);
				if (found is not null)
				{
					return found;
				}
			}

			return null;
		}

		foreach (var comment in commentPost.Comments)
		{
			var foundComment = FindComment(comment);
			if (foundComment is not null)
			{
				return foundComment;
			}
		}

		return null;
	}

	public async Task<IActionResult> OnPostPostCreate(string title, string contentType, string? content)
	{
		var userId = User.GetUserId();
		if (userId == -1)
		{
			return new UnauthorizedObjectResult("Please log in.");
		}

		if (!User.Has(PermissionTo.CreateForumPosts))
		{
			return Forbid("You do not have permission to create posts.");
		}

		if (contentType != "Text" && contentType != "Image")
		{
			ModelState.AddModelError(nameof(contentType), "Invalid content type.");
		}

		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var post = new Post
		{
			Title = title,
			Date = DateTime.UtcNow,
			ContentType = contentType,
			Content = content ?? "",
			UserId = userId,
			Votes = [new()
				{
					UserId = userId,
					Value = 1
				}
			],
		};

		db.Posts.Add(post);
		await db.SaveChangesAsync();

		var newAchievements = await _achievementService.CheckAndGrantRegular(userId, AchievementData.PostsCreated);
		if ((content?.Length ?? 0) >= AchievementData.LongContentLength)
		{
			newAchievements.AddRange(await _achievementService.GrantSpecial(userId, AchievementData.LongContent));
		}

		var postModel = await GetSinglePost(post.Id, userId);
		return new JsonResult(new AchievementResult<PostModel?>(postModel, newAchievements));
	}

	public async Task<IActionResult> OnPostCommentCreate(int postId, int? parentId, string content)
	{
		var userId = User.GetUserId();
		if (userId == -1)
		{
			return new UnauthorizedObjectResult("Please log in.");
		}

		if (!User.Has(PermissionTo.CreateForumPosts))
		{
			return Forbid("You do not have permission to create comments.");
		}

		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var existingPost = await db.Posts
			.Include(p => p.Comments)
			.Where(p => p.Id == postId)
			.Select(p => new
			{
				p.Id,
				p.IsLocked,
				Comments = p.Comments.Select(p => p.Id)
			})
			.FirstOrDefaultAsync();

		if (existingPost is null)
		{
			return NotFound("Post not found.");
		}

		if (existingPost.IsLocked)
		{
			return Forbid("This post is locked.");
		}

		if (parentId != null && !existingPost.Comments.Contains(parentId.Value))
		{
			return NotFound("Parent comment not found.");
		}

		Comment comment = new()
		{
			Date = DateTime.UtcNow,
			PostId = postId,
			ParentCommentId = parentId,
			Content = content,
			UserId = userId,
			Votes = [new()
				{
					UserId = userId,
					Value = 1
				}
			],
		};
		db.Comments.Add(comment);

		await db.SaveChangesAsync();

		var commentModel = await GetSingleComment(comment.Id, comment.PostId, userId);
		var newAchievements = await _achievementService.CheckAndGrantRegular(userId, AchievementData.CommentsCreated);

		if (content.Length >= AchievementData.LongContentLength)
		{
			newAchievements.AddRange(await _achievementService.GrantSpecial(userId, AchievementData.LongContent));
		}

		if (parentId is not null)
		{
			var parentAuthorId = await db.Comments
				.Where(c => c.Id == parentId)
				.Select(c => c.UserId)
				.FirstOrDefaultAsync();
			if (parentAuthorId == userId)
			{
				newAchievements.AddRange(await _achievementService.GrantSpecial(userId, AchievementData.ReplyToOwnComment));
			}
		}

		newAchievements.AddRange(await _achievementService.GrantSpecial(userId, AchievementData.ThreadDepth5));

		return new JsonResult(new AchievementResult<CommentModel?>(commentModel, newAchievements));
	}

	public async Task<IActionResult> OnPostPostVote(int id, int value)
	{
		var userId = User.GetUserId();
		if (userId == -1)
		{
			return new UnauthorizedObjectResult("Please log in.");
		}

		if (!(value == -1 || value == 1))
		{
			ModelState.AddModelError(nameof(value), "Vote value must be -1 or 1.");
		}

		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var post = await db.Posts.FindAsync(id);
		if (post is null || post.IsDeleted)
		{
			return new JsonResult(null);
		}

		var existing = await db.PostVotes.FirstOrDefaultAsync(v => v.UserId == userId && v.PostId == id);

		if (existing != null)
		{
			if (existing.Value == value)
			{
				db.PostVotes.Remove(existing);
			}
			else
			{
				existing.Value = value;
			}
		}
		else
		{
			db.PostVotes.Add(new PostVote
			{
				PostId = id,
				Value = value,
				UserId = userId
			});
		}

		await db.SaveChangesAsync();

		var newAchievements = new List<Achievement>();

		if (value == 1)
		{
			newAchievements.AddRange(await _achievementService.CheckAndGrantRegular(userId, AchievementData.UpvotesMade));
		}

		if (value == -1)
		{
			newAchievements.AddRange(await _achievementService.CheckAndGrantRegular(userId, AchievementData.DownvotesMade));
			if (post.UserId == userId)
			{
				newAchievements.AddRange(await _achievementService.GrantSpecial(userId, AchievementData.DownvoteSelf));
			}
		}

		await _achievementService.CheckAndGrantRegular(post.UserId, AchievementData.HighScoreReached);

		return new JsonResult(new AchievementResult<object?>(null, newAchievements));
	}

	public async Task<IActionResult> OnPostCommentVote(int id, int value)
	{
		var userId = User.GetUserId();
		if (userId == -1)
		{
			return new UnauthorizedObjectResult("Please log in.");
		}

		if (!(value == -1 || value == 1))
		{
			ModelState.AddModelError(nameof(value), "Vote value must be -1 or 1.");
		}

		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var comment = await db.Comments.FindAsync(id);
		if (comment is null || comment.IsDeleted)
		{
			return new JsonResult(null);
		}

		var existing = await db.CommentVotes.FirstOrDefaultAsync(v => v.UserId == userId && v.CommentId == id);
		if (existing != null)
		{
			if (existing.Value == value)
			{
				db.CommentVotes.Remove(existing);
			}
			else
			{
				existing.Value = value;
			}
		}
		else
		{
			db.CommentVotes.Add(new CommentVote
			{
				CommentId = id,
				Value = value,
				UserId = userId
			});
		}

		await db.SaveChangesAsync();

		var newAchievements = new List<Achievement>();

		if (value == 1)
		{
			newAchievements.AddRange(await _achievementService.CheckAndGrantRegular(userId, AchievementData.UpvotesMade));
		}

		if (value == -1)
		{
			newAchievements.AddRange(await _achievementService.CheckAndGrantRegular(userId, AchievementData.DownvotesMade));
			if (comment.UserId == userId)
			{
				newAchievements.AddRange(await _achievementService.GrantSpecial(userId, AchievementData.DownvoteSelf));
			}
		}

		await _achievementService.CheckAndGrantRegular(comment.UserId, AchievementData.HighScoreReached);

		return new JsonResult(new AchievementResult<object?>(null, newAchievements));
	}

	public async Task<IActionResult> OnPostPostReact(int id, string emoji)
	{
		var userId = User.GetUserId();
		if (userId == -1)
		{
			return new UnauthorizedObjectResult("Please log in.");
		}

		if (!_allowedEmojis.Contains(emoji))
		{
			ModelState.AddModelError(nameof(emoji), "Emoji not allowed.");
		}

		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var post = await db.Posts.FindAsync(id);
		if (post is null || post.IsDeleted)
		{
			return new JsonResult(null);
		}

		var existing = await db.PostReactions.FirstOrDefaultAsync(r => r.PostId == id && r.UserId == userId);
		var newAchievements = new List<Achievement>();

		if (existing != null)
		{
			db.PostReactions.Remove(existing);
			await db.SaveChangesAsync();
			newAchievements.AddRange(await _achievementService.GrantSpecial(userId, AchievementData.UndoReaction));
		}
		else
		{
			db.PostReactions.Add(new PostReaction
			{
				PostId = id,
				Emoji = emoji,
				UserId = userId
			});
			await db.SaveChangesAsync();
			newAchievements.AddRange(await _achievementService.CheckAndGrantRegular(userId, AchievementData.ReactionsMade));
		}

		return new JsonResult(new AchievementResult<object?>(null, newAchievements));
	}

	public async Task<IActionResult> OnPostCommentReact(int id, string emoji)
	{
		var userId = User.GetUserId();
		if (userId == -1)
		{
			return new UnauthorizedObjectResult("Please log in.");
		}

		if (!_allowedEmojis.Contains(emoji))
		{
			ModelState.AddModelError(nameof(emoji), "Emoji not allowed.");
		}

		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var comment = await db.Comments.FindAsync(id);
		if (comment is null || comment.IsDeleted)
		{
			return new JsonResult(null);
		}

		var existing = await db.CommentReactions.FirstOrDefaultAsync(r => r.CommentId == id && r.UserId == userId);
		var newAchievements = new List<Achievement>();

		if (existing != null)
		{
			db.CommentReactions.Remove(existing);
			await db.SaveChangesAsync();
			newAchievements.AddRange(await _achievementService.GrantSpecial(userId, AchievementData.UndoReaction));
		}
		else
		{
			db.CommentReactions.Add(new CommentReaction
			{
				CommentId = id,
				Emoji = emoji,
				UserId = userId
			});
			await db.SaveChangesAsync();
			newAchievements.AddRange(await _achievementService.CheckAndGrantRegular(userId, AchievementData.ReactionsMade));
		}

		return new JsonResult(null);
	}

	public async Task<IActionResult> OnPostPostEdit(int id, string content)
	{
		var userId = User.GetUserId();
		if (userId == -1)
		{
			return new UnauthorizedObjectResult("Please log in.");
		}

		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var post = await db.Posts.FindAsync(id);
		if (post is null || post.IsDeleted)
		{
			return NotFound("Post not found.");
		}

		if (post.UserId != userId && !User.Has(PermissionTo.EditUsersForumPosts))
		{
			return Forbid("Only authors and moderators can edit posts.");
		}

		post.Content = content;
		post.LastEdited = DateTime.UtcNow;
		await db.SaveChangesAsync();

		var newAchievements = new List<Achievement>();
		if (content.Length >= AchievementData.LongContentLength)
		{
			newAchievements.AddRange(await _achievementService.GrantSpecial(userId, AchievementData.LongContent));
		}

		return new JsonResult(new AchievementResult<PostModel?>(await GetSinglePost(id, userId), newAchievements));
	}

	public async Task<IActionResult> OnPostCommentEdit(int id, string content)
	{
		var userId = User.GetUserId();
		if (userId == -1)
		{
			return new UnauthorizedObjectResult("Please log in.");
		}

		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var comment = await db.Comments.FindAsync(id);
		if (comment is null || comment.IsDeleted)
		{
			return NotFound("Comment not found.");
		}

		if (comment.UserId != userId && !User.Has(PermissionTo.EditUsersForumPosts))
		{
			return Forbid("Only authors and moderators can edit comments.");
		}

		comment.Content = content;
		comment.LastEdited = DateTime.UtcNow;
		await db.SaveChangesAsync();

		var newAchievements = new List<Achievement>();
		if (content.Length >= AchievementData.LongContentLength)
		{
			newAchievements.AddRange(await _achievementService.GrantSpecial(userId, AchievementData.LongContent));
		}

		return new JsonResult(new AchievementResult<CommentModel?>(await GetSingleComment(comment.Id, comment.PostId, userId), newAchievements));
	}

	public async Task<IActionResult> OnPostPostDelete(int id)
	{
		var userId = User.GetUserId();
		if (userId == -1)
		{
			return new UnauthorizedObjectResult("Please log in.");
		}

		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var post = await db.Posts.FindAsync(id);
		if (post is null || post.IsDeleted)
		{
			return NotFound("Post not found.");
		}

		if (post.UserId != userId && !User.Has(PermissionTo.DeleteForumPosts))
		{
			return Forbid("Only authors and moderators can delete posts.");
		}

		post.IsDeleted = true;
		post.LastEdited = DateTime.UtcNow;
		await db.SaveChangesAsync();

		return new JsonResult(null);
	}

	public async Task<IActionResult> OnPostCommentDelete(int id)
	{
		var userId = User.GetUserId();
		if (userId == -1)
		{
			return new UnauthorizedObjectResult("Please log in.");
		}

		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var comment = await db.Comments.FindAsync(id);
		if (comment is null || comment.IsDeleted)
		{
			return NotFound("Comment not found.");
		}

		if (comment.UserId != userId && !User.Has(PermissionTo.DeleteForumPosts))
		{
			return Forbid("Only authors and moderators can delete comments.");
		}

		comment.IsDeleted = true;
		await db.SaveChangesAsync();

		return new JsonResult(null);
	}

	public async Task<IActionResult> OnPostQuery(string query)
	{
		if (User.Name() != "Masterjun")
		{
			return Forbid("No.");
		}

		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var conn = db.Database.GetDbConnection();
		try
		{
			await conn.OpenAsync();
			using var cmd = conn.CreateCommand();
#pragma warning disable CA2100
			cmd.CommandText = query;
#pragma warning restore CA2100
			using var reader = await cmd.ExecuteReaderAsync();
			var results = new List<Dictionary<string, object>>();
			while (await reader.ReadAsync())
			{
				var row = new Dictionary<string, object>();
				for (int i = 0; i < reader.FieldCount; i++)
				{
					row[reader.GetName(i)] = reader.GetValue(i);
				}

				results.Add(row);
			}

			return new JsonResult(results);
		}
		catch (Exception ex)
		{
			return BadRequest(ex.ToString());
		}
		finally
		{
			await conn.CloseAsync();
		}
	}
}

public class AchievementService(FeedDbContext db)
{
	public async Task<List<Achievement>> CheckAndGrantRegular(int userId, params string[] keys)
	{
		var newAchievements = new List<Achievement>();
		var existing = await db.Achievements
			.Where(a => a.UserId == userId)
			.ToListAsync();

		foreach (var key in keys.Distinct())
		{
			int[]? tiers = key switch
			{
				AchievementData.UpvotesMade => AchievementData.UpvotesMadeTiers,
				AchievementData.ReactionsMade => AchievementData.ReactionsMadeTiers,
				AchievementData.CommentsCreated => AchievementData.CommentsCreatedTiers,
				AchievementData.PostsCreated => AchievementData.PostsCreatedTiers,
				AchievementData.HighScoreReached => AchievementData.HighScoreReachedTiers,
				AchievementData.DownvotesMade => AchievementData.DownvotesMadeTiers,
				_ => null
			};

			if (tiers is null)
			{
				continue;
			}

			int value = await GetValue(userId, key);
			int highestEarned = existing
				.Where(a => a.Key == key)
				.Select(a => a.Tier)
				.DefaultIfEmpty(0)
				.Max();

			for (int i = 0; i < tiers.Length; i++)
			{
				int tier = i + 1;
				if (value >= tiers[i] && tier > highestEarned)
				{
					newAchievements.Add(new Achievement
					{
						UserId = userId,
						Key = key,
						Tier = tier,
						Date = DateTime.UtcNow
					});
					highestEarned = tier;
				}
			}
		}

		if (newAchievements.Count > 0)
		{
			db.Achievements.AddRange(newAchievements);
			await db.SaveChangesAsync();
		}

		return newAchievements;
	}

	public async Task<List<Achievement>> GrantSpecial(int userId, string key)
	{
		bool alreadyExists = await db.Achievements.AnyAsync(a => a.UserId == userId && a.Key == key);
		if (alreadyExists)
		{
			return [];
		}

		if (key == AchievementData.ThreadDepth5 && !await HasThreadDepth5(userId))
		{
			return [];
		}

		var achievement = new Achievement
		{
			UserId = userId,
			Key = key,
			Tier = 0,
			Date = DateTime.UtcNow
		};
		db.Achievements.Add(achievement);
		await db.SaveChangesAsync();
		return [achievement];
	}

	private async Task<int> GetValue(int userId, string key) => key switch
	{
		AchievementData.UpvotesMade =>
			await db.PostVotes.CountAsync(v => v.UserId == userId && v.Value == 1 && v.Post.UserId != userId)
			+ await db.CommentVotes.CountAsync(v => v.UserId == userId && v.Value == 1 && v.Comment.UserId != userId),

		AchievementData.DownvotesMade =>
			await db.PostVotes.CountAsync(v => v.UserId == userId && v.Value == -1 && v.Post.UserId != userId)
			+ await db.CommentVotes.CountAsync(v => v.UserId == userId && v.Value == -1 && v.Comment.UserId != userId),

		AchievementData.ReactionsMade =>
			await db.PostReactions.CountAsync(r => r.UserId == userId && r.Post.UserId != userId)
			+ await db.CommentReactions.CountAsync(r => r.UserId == userId && r.Comment.UserId != userId),

		AchievementData.CommentsCreated =>
			await db.Comments.CountAsync(c => c.UserId == userId && !c.IsDeleted),

		AchievementData.PostsCreated =>
			await db.Posts.CountAsync(p => p.UserId == userId && !p.IsDeleted),

		AchievementData.HighScoreReached =>
			Math.Max(
				(await db.Posts
					.Where(p => p.UserId == userId)
					.Select(p => p.Votes.Sum(v => v.Value))
					.ToListAsync())
					.DefaultIfEmpty(0)
					.Max(),
				(await db.Comments
					.Where(p => p.UserId == userId)
					.Select(p => p.Votes.Sum(v => v.Value))
					.ToListAsync())
					.DefaultIfEmpty(0)
					.Max()),

		_ => 0
	};

	private async Task<bool> HasThreadDepth5(int userId)
	{
		var userComments = await db.Comments
			.Where(c => c.UserId == userId && !c.IsDeleted)
			.Select(c => new { c.Id, c.ParentCommentId, c.PostId })
			.ToListAsync();

		if (userComments.Count == 0)
		{
			return false;
		}

		var postIds = userComments.Select(c => c.PostId).Distinct().ToList();
		var allComments = await db.Comments
			.Where(c => postIds.Contains(c.PostId))
			.Select(c => new { c.Id, c.ParentCommentId })
			.ToListAsync();

		var parentMap = allComments.ToDictionary(c => c.Id, c => c.ParentCommentId);

		foreach (var uc in userComments)
		{
			int depth = 1;
			int? current = uc.ParentCommentId;
			while (current is not null && parentMap.TryGetValue(current.Value, out var parent))
			{
				depth++;
				if (depth >= 5)
				{
					return true;
				}

				current = parent;
			}
		}

		return false;
	}
}

public static class AchievementData
{
	public const string UpvotesMade = "UpvotesMade";
	public const string ReactionsMade = "ReactionsMade";
	public const string CommentsCreated = "CommentsCreated";
	public const string PostsCreated = "PostsCreated";
	public const string HighScoreReached = "HighScoreReached";
	public const string DownvotesMade = "DownvotesMade";

	public const string DownvoteSelf = "DownvoteSelf";
	public const string ReplyToOwnComment = "ReplyToOwnComment";
	public const string ThreadDepth5 = "ThreadDepth5";
	public const string UndoReaction = "UndoReaction";
	public const string LongContent = "LongContent";

	public static readonly int[] UpvotesMadeTiers = [1, 20, 50];
	public static readonly int[] ReactionsMadeTiers = [1, 7, 30];
	public static readonly int[] CommentsCreatedTiers = [1, 4, 10];
	public static readonly int[] PostsCreatedTiers = [1, 3, 5];
	public static readonly int[] HighScoreReachedTiers = [2, 4, 6];
	public static readonly int[] DownvotesMadeTiers = [1, 2, 3];
	public static readonly int LongContentLength = 500;
}

public class Achievement
{
	public int Id { get; set; }
	public int UserId { get; set; }
	public bool HasSeen { get; set; }
	public string Key { get; set; } = "";
	public int Tier { get; set; }
	public DateTime Date { get; set; }
}

public class PageState
{
	public string? UserName { get; set; }
	public bool CanModEdit { get; set; }
	public bool CanModDelete { get; set; }
	public string Type { get; set; } = "Home";
	public PostModel? Post { get; set; }
	public int? CommentId { get; set; }
}

public class UserNameCache(IMemoryCache cache, IServiceProvider serviceProvider)
{
	public class UserNameCacheEntry
	{
		public string UserName { get; set; } = "";
		public string? Avatar { get; set; }
	}

	private readonly TimeSpan _expireAfter = TimeSpan.FromHours(1);

	private string GetCacheKey(int userId) => $"Feed_User{userId}";

	public async Task<Dictionary<int, UserNameCacheEntry>> ResolveUserIds(List<int> userIds)
	{
		var resultUserNames = new Dictionary<int, UserNameCacheEntry>();
		foreach (var userId in userIds)
		{
			if (cache.TryGetValue(GetCacheKey(userId), out UserNameCacheEntry? entry))
			{
				resultUserNames[userId] = entry ?? new UserNameCacheEntry { UserName = "Unknown" };
			}
		}

		var missingUserIds = userIds.Where(id => !resultUserNames.ContainsKey(id)).ToList();
		if (missingUserIds.Count != 0)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

				var missingUserNames = await db.Users
					.Where(u => missingUserIds.Contains(u.Id))
					.Select(u => new
					{
						u.Id,
						Entry = new UserNameCacheEntry
						{
							UserName = u.UserName,
							Avatar = u.Avatar
						}
					})
					.ToListAsync();

				missingUserNames.ForEach(user =>
				{
					cache.Set(GetCacheKey(user.Id), user.Entry, _expireAfter);
					resultUserNames[user.Id] = user.Entry;
				});
			}
		}

		return resultUserNames;
	}
}

public class PostModel
{
	public int Id { get; set; }
	public int? AuthorId { get; set; }
	public string? Author { get; set; }
	public string? AuthorAvatar { get; set; }
	public DateTime Date { get; set; }
	public DateTime? LastEdited { get; set; }
	public string? Title { get; set; } = "";
	public string ContentType { get; set; } = "";
	public string Content { get; set; } = "";
	public string ExtraVideoContent { get; set; } = "";
	public string ExtraOverrideLink { get; set; } = "";
	public int? Score { get; set; }
	public double? ScoreHot { get; set; }
	public int MyVote { get; set; }
	public Dictionary<string, ReactionModel> Reactions { get; set; } = [];
	public bool IsDeleted { get; set; }
	public bool IsSticky { get; set; }
	public bool IsLocked { get; set; }
	public List<CommentModel> Comments { get; set; } = [];
}

public static class PostModelExtensions
{
	private static readonly long _startOffset = new DateTime(2026, 3, 22).Ticks / TimeSpan.TicksPerHour;

	extension(IEnumerable<Post> posts)
	{
		public IEnumerable<PostModel> SelectPostModel(int userId)
		{
			return posts.Select(p => new PostModel
			{
				Id = p.Id,
				AuthorId = p.UserId,
				Date = p.Date,
				LastEdited = p.LastEdited,
				Title = p.Title,
				ContentType = p.ContentType,
				Content = p.Content,
				ExtraVideoContent = p.ExtraVideoContent,
				ExtraOverrideLink = p.ExtraOverrideLink,
				Score = p.Votes.Sum(v => v.Value),
				MyVote = p.Votes.Where(v => v.UserId == userId).Select(v => v.Value).FirstOrDefault(0),
				IsDeleted = p.IsDeleted,
				IsSticky = p.IsSticky,
				IsLocked = p.IsLocked,
				Comments = p.Comments
					.Select(c =>
					{
						var upvotes = c.Votes.Count(v => v.Value == 1);
						var downvotes = c.Votes.Count(v => v.Value == -1);
						var totalVotes = upvotes + downvotes;
						var comment = new CommentModel
						{
							Id = c.Id,
							PostId = c.PostId,
							PostTitle = p.IsDeleted ? null : p.Title,
							ParentId = c.ParentCommentId,
							AuthorId = c.UserId,
							Date = c.Date,
							LastEdited = c.LastEdited,
							Content = c.Content,
							Score = c.Votes.Sum(v => v.Value),
							ScoreWilson = totalVotes == 0 ? 0
								: (((upvotes + 1.9208) / totalVotes)
									- (1.96 * Math.Sqrt(((upvotes * downvotes) / totalVotes) + 0.9604)
										/ totalVotes))
									/ (1 + (3.8416 / totalVotes)),
							MyVote = c.Votes.Where(v => v.UserId == userId).Select(v => v.Value).FirstOrDefault(0),
							IsDeleted = c.IsDeleted,
							Reactions = c.Reactions.GroupBy(r => r.Emoji).ToDictionary(
								g => g.Key,
								g => new ReactionModel
								{
									Count = g.Count(),
									MyReaction = g.Any(r => r.UserId == userId)
								})
						};
						return comment;
					})
					.OrderByDescending(c => c.ScoreWilson)
					.ThenByDescending(c => c.Score)
					.ToList(),
				Reactions = p.Reactions.GroupBy(r => r.Emoji).ToDictionary(
					g => g.Key,
					g => new ReactionModel
					{
						Count = g.Count(),
						MyReaction = g.Any(r => r.UserId == userId)
					})
			});
		}
	}

	extension(IQueryable<Post> posts)
	{
		public IQueryable<PostOrderHot> SelectOrderHot()
		{
			return posts.Select(p => new
			{
				p.Id,
				p.IsSticky,
				Score = p.Votes.Sum(v => v.Value),
				Freshness = ((p.Date.Ticks / TimeSpan.TicksPerHour) - _startOffset) / 4,
			})
			.Select(p => new PostOrderHot
			{
				Id = p.Id,
				IsSticky = p.IsSticky,
				ScoreHot = p.Freshness + (p.Score < 0 ? -Math.Log2(Math.Abs(p.Score) + 1) : Math.Log2(Math.Abs(p.Score) + 1)),
			});
		}
	}
}

public class PostOrderHot
{
	public int Id { get; set; }
	public bool IsSticky { get; set; }
	public double ScoreHot { get; set; }
}

public class CommentModel
{
	public int Id { get; set; }
	public int PostId { get; set; }
	public string? PostTitle { get; set; }
	public int? ParentId { get; set; }
	public int? AuthorId { get; set; }
	public string? Author { get; set; }
	public string? AuthorAvatar { get; set; }
	public DateTime Date { get; set; }
	public DateTime? LastEdited { get; set; }
	public string Content { get; set; } = "";
	public int? Score { get; set; }
	public double? ScoreWilson { get; set; }
	public int MyVote { get; set; }
	public Dictionary<string, ReactionModel> Reactions { get; set; } = [];
	public bool IsDeleted { get; set; }
	public List<CommentModel> Replies { get; set; } = [];
}

public static class CommentModelExtensions
{
	extension(IEnumerable<CommentModel> comments)
	{
		public List<CommentModel> ToNestedList()
		{
			var commentsList = comments.ToList();
			var commentsDict = commentsList.ToDictionary(c => c.Id);
			foreach (var comment in commentsList)
			{
				if (comment.ParentId is not null)
				{
					commentsDict[comment.ParentId.Value].Replies.Add(comment);
				}
			}

			return commentsList;
		}

		public IEnumerable<CommentModel> RootComments()
		{
			return comments.Where(c => c.ParentId == null);
		}
	}
}

public class ReactionModel
{
	public int Count { get; set; }
	public bool MyReaction { get; set; }
}

public class FeedDbContext(DbContextOptions<FeedDbContext> options) : DbContext(options)
{
	public DbSet<Post> Posts { get; set; }
	public DbSet<PostVote> PostVotes { get; set; }
	public DbSet<PostReaction> PostReactions { get; set; }
	public DbSet<Comment> Comments { get; set; }
	public DbSet<CommentVote> CommentVotes { get; set; }
	public DbSet<CommentReaction> CommentReactions { get; set; }
	public DbSet<Achievement> Achievements { get; set; }
}

public class Post
{
	public int Id { get; set; }
	public int UserId { get; set; }
	public DateTime Date { get; set; }
	public DateTime? LastEdited { get; set; }
	public string Title { get; set; } = "";
	public string ContentType { get; set; } = "";
	public string Content { get; set; } = "";
	public string ExtraVideoContent { get; set; } = "";
	public string ExtraOverrideLink { get; set; } = "";
	public bool IsDeleted { get; set; }
	public bool IsSticky { get; set; }
	public bool IsLocked { get; set; }

	public ICollection<Comment> Comments { get; set; } = [];
	public ICollection<PostVote> Votes { get; set; } = [];
	public ICollection<PostReaction> Reactions { get; set; } = [];
}

public class PostVote
{
	public int Id { get; set; }
	public int PostId { get; set; }
	public Post Post { get; set; } = null!;
	public int Value { get; set; }
	public int UserId { get; set; }
}

public class Comment
{
	public int Id { get; set; }
	public int PostId { get; set; }
	public Post Post { get; set; } = null!;
	public int? ParentCommentId { get; set; }
	public Comment? ParentComment { get; set; }

	public int UserId { get; set; }
	public DateTime Date { get; set; }
	public DateTime? LastEdited { get; set; }
	public string Content { get; set; } = "";
	public bool IsDeleted { get; set; }

	public ICollection<CommentVote> Votes { get; set; } = [];
	public ICollection<CommentReaction> Reactions { get; set; } = [];
	public ICollection<Comment> Replies { get; set; } = [];
}

public class CommentVote
{
	public int Id { get; set; }
	public int CommentId { get; set; }
	public Comment Comment { get; set; } = null!;
	public int Value { get; set; }
	public int UserId { get; set; }
}

public class PostReaction
{
	public int Id { get; set; }

	public int PostId { get; set; }
	public Post Post { get; set; } = null!;

	public string Emoji { get; set; } = "";
	public int UserId { get; set; }
}

public class CommentReaction
{
	public int Id { get; set; }

	public int CommentId { get; set; }
	public Comment Comment { get; set; } = null!;

	public string Emoji { get; set; } = "";
	public int UserId { get; set; }
}

public class AchievementResult<T>(T data, List<Achievement> newAchievements)
{
	public T Data { get; } = data;
	public List<Achievement> NewAchievements { get; } = newAchievements;
}
