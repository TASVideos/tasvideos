using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Models;
using TASVideos.ViewComponents;

namespace TASVideos.Tasks
{
	public class ForumTasks
	{
		private readonly ApplicationDbContext _db;
		private readonly AwardTasks _awardTasks;

		public ForumTasks(
			ApplicationDbContext db,
			AwardTasks awardTasks)
		{
			_db = db;
			_awardTasks = awardTasks;
		}

		/// <summary>
		/// Returns data necessary for the Forum/Index page
		/// </summary>
		public async Task<ForumIndexModel> GetForumIndex()
		{
			return new ForumIndexModel
			{
				Categories = await _db.ForumCategories
					.Include(c => c.Forums)
					.ToListAsync()
			};
		}

		/// <summary>
		/// Returns a forum and topics for the given id
		/// For the purpose of display
		/// </summary>
		public async Task<ForumModel> GetForumForDisplay(ForumRequest paging, bool allowRestricted)
		{
			using (await _db.Database.BeginTransactionAsync())
			{
				var model = await _db.Forums
					.Where(f => allowRestricted || !f.Restricted)
					.Select(f => new ForumModel
					{
						Id = f.Id,
						Name = f.Name,
						Description = f.Description
					})
					.SingleOrDefaultAsync(f => f.Id == paging.Id);

				if (model == null)
				{
					return null;
				}

				var rowsToSkip = paging.GetRowsToSkip();
				var rowCount = await _db.ForumTopics
					.Where(ft => ft.ForumId == paging.Id)
					.CountAsync();

				var results = await _db.ForumTopics
					.Where(ft => ft.ForumId == paging.Id)
					.Select(ft => new ForumModel.ForumTopicEntry
					{
						Id = ft.Id,
						Title = ft.Title,
						CreateUserName = ft.CreateUserName,
						CreateTimestamp = ft.CreateTimeStamp,
						Type = ft.Type,
						Views = ft.Views,
						PostCount = ft.ForumPosts.Count,
						LastPost = ft.ForumPosts.Max(fp => (DateTime?)fp.CreateTimeStamp)
					})
					.OrderByDescending(ft => ft.Type == ForumTopicType.Sticky)
					.ThenByDescending(ft => ft.Type == ForumTopicType.Announcement)
					.ThenByDescending(ft => ft.LastPost)
					.Skip(rowsToSkip)
					.Take(paging.PageSize)
					.ToListAsync();

				model.Topics = new PageOf<ForumModel.ForumTopicEntry>(results)
				{
					PageSize = paging.PageSize,
					CurrentPage = paging.CurrentPage,
					RowCount = rowCount,
					SortDescending = paging.SortDescending,
					SortBy = paging.SortBy
				};

				return model;
			}
		}

		/// <summary>
		/// Displays a page of posts for the given topic
		/// </summary>
		public async Task<ForumTopicModel> GetTopicForDisplay(TopicRequest paging, bool allowRestricted)
		{
			var model = await _db.ForumTopics
				.Where(f => allowRestricted || !f.Forum.Restricted)
				.Select(t => new ForumTopicModel
				{
					Id = t.Id,
					Title = t.Title,
					ForumId = t.ForumId,
					ForumName = t.Forum.Name,
					IsLocked = t.IsLocked,
					Poll = t.PollId.HasValue
						? new ForumTopicModel.PollModel { PollId = t.PollId.Value, Question = t.Poll.Question }
						: null
				})
				.SingleOrDefaultAsync(t => t.Id == paging.Id);

			if (model == null)
			{
				return null;
			}

			var lastPostId = (await _db.ForumPosts
				.Where(p => p.TopicId == paging.Id)
				.OrderByDescending(p => p.CreateTimeStamp)
				.FirstAsync())
				.Id;

			model.Posts = _db.ForumPosts
				.Where(p => p.TopicId == paging.Id)
				.Select(p => new ForumTopicModel.ForumPostEntry
				{
					Id = p.Id,
					EnableHtml = p.EnableHtml,
					EnableBbCode = p.EnableBbCode,
					PosterId = p.PosterId,
					CreateTimestamp = p.CreateTimeStamp,
					PosterName = p.Poster.UserName,
					PosterAvatar = p.Poster.Avatar,
					PosterLocation = p.Poster.From,
					PosterRoles = p.Poster.UserRoles
						.Where(ur => !ur.Role.IsDefault)
						.Select(ur => ur.Role.Name),
					PosterJoined = p.Poster.CreateTimeStamp,
					PosterPostCount = p.Poster.Posts.Count,
					Text = p.Text,
					Subject = p.Subject,
					Signature = p.Poster.Signature,
					IsLastPost = p.Id == lastPostId
				})
				.OrderBy(p => p.CreateTimestamp)
				.PageOf(_db, paging);

			foreach (var post in model.Posts)
			{
				post.Awards = await _awardTasks.GetAllAwardsForUser(post.PosterId);
			}

			if (model.Poll != null)
			{
				model.Poll.Options = await _db.ForumPollOptions
					.Where(o => o.PollId == model.Poll.PollId)
					.Select(o => new ForumTopicModel.PollModel.PollOptionModel
					{
						Text = o.Text,
						Ordinal = o.Ordinal,
						Voters = o.Votes.Select(v => v.UserId).ToList()
					})
					.ToListAsync();
			}

			return model;
		}

		/// <summary>
		/// Sets a topics locked status
		/// </summary>
		/// <returns>True if the topic is found, else false</returns>
		public async Task<bool> SetTopicLock(int topicId, bool isLocked, bool allowRestricted)
		{
			var topic = await _db.ForumTopics
				.Where(ft => allowRestricted || !ft.Forum.Restricted)
				.SingleOrDefaultAsync(t => t.Id == topicId);

			if (topic == null)
			{
				return false;
			}

			if (topic.IsLocked != isLocked)
			{
				topic.IsLocked = isLocked;
				await _db.SaveChangesAsync();
			}

			return true;
		}

		// TODO: document
		public async Task<IEnumerable<TopicFeedModel.TopicPost>> GetTopicFeed(int topicId, int limit, bool allowRestricted)
		{
			return await _db.ForumPosts
				.Where(p => p.TopicId == topicId)
				.Where(ft => allowRestricted || !ft.Topic.Forum.Restricted)
				.Select(p => new TopicFeedModel.TopicPost
				{
					Id = p.Id,
					EnableBbCode = p.EnableBbCode,
					EnableHtml = p.EnableBbCode,
					Text = p.Text,
					Subject = p.Subject,
					PosterName = p.Poster.UserName,
					PostTime = p.CreateTimeStamp
				})
				.OrderByDescending(p => p.PostTime)
				.Take(limit)
				.ToListAsync();
		}

		/// <summary>
		/// Returns whether or not a forum exists and if not allowRestircted, then whether it is not restricted
		/// </summary>
		public async Task<bool> ForumAccessible(int forumId, bool allowRestricted)
		{
			return await _db.Forums
				.AnyAsync(f => f.Id == forumId
					&& (allowRestricted || !f.Restricted));
		}

		/// <summary>
		/// Returns whether or not a topicm exists and if not allowRestircted, then whether it is not restricted
		/// </summary>
		public async Task<bool> TopicAccessible(int topicId, bool allowRestricted)
		{
			return await _db.ForumTopics
				.AnyAsync(t => t.Id == topicId
					&& (allowRestricted || !t.Forum.Restricted));
		}

		// TODO: document
		public async Task<TopicCreateModel> GetCreateTopicData(int forumId, bool allowRestricted)
		{
			var forum = await _db.Forums
				.Where(f => allowRestricted || !f.Restricted)
				.SingleOrDefaultAsync(f => f.Id == forumId);

			if (forum == null)
			{
				return null;
			}

			return new TopicCreateModel
			{
				ForumId = forumId,
				ForumName = forum.Name
			};
		}

		/// <summary>
		/// Creates a new <see cref="ForumTopic" /> and the first <see cref="ForumPost"/> of that topic
		/// </summary>
		/// <returns>
		/// The id of the newly created <see cref="ForumTopic" />
		/// If a topic could not be created, returns null
		/// </returns>
		public async Task<int?> CreateTopic(TopicCreatePostModel model, User user, string ipAddress)
		{
			var topic = new ForumTopic
			{
				Type = model.Type,
				Title = model.Title,
				PosterId = user.Id,
				Poster = user,
				ForumId = model.ForumId
			};

			_db.ForumTopics.Add(topic);
			await _db.SaveChangesAsync();

			var forumPostModel = new ForumPostModel
			{
				TopicId = topic.Id,
				Subject = null,
				Post = model.Post
			};

			await CreatePost(forumPostModel, user, ipAddress);
			return topic.Id;
		}

		/// <summary>
		/// Returns necessary data to display on the create post screen
		/// If a topic is not found or not accessible, null is returned
		/// </summary>
		public async Task<ForumPostCreateModel> GetCreatePostData(int topicId, int? postId, bool allowRestricted)
		{
			var topic = await _db.ForumTopics
				.Where(ft => allowRestricted || !ft.Forum.Restricted)
				.SingleOrDefaultAsync(t => t.Id == topicId);

			if (topic == null)
			{
				return null;
			}

			var model = new ForumPostCreateModel
			{
				TopicId = topicId,
				TopicTitle = topic.Title,
				IsLocked = topic.IsLocked
			};

			if (postId.HasValue)
			{
				var post = await _db.ForumPosts
				.Include(p => p.Poster)
				.Where(p => p.Id == postId)
				.SingleOrDefaultAsync();

				model.Post = $"[quote=\"{post.Poster.UserName}\"]{post.Text}[/quote]";
			}

			return model;
		}

		public async Task CreatePost(ForumPostModel model, User user, string ipAddress)
		{
			var forumPost = new ForumPost
			{
				TopicId = model.TopicId,
				PosterId = user.Id,
				IpAddress = ipAddress,
				Subject = model.Subject,
				Text = model.Post,

				// TODO: check for bbcode and if none, set this to false?
				// For now we are not giving the user choices
				EnableHtml = false,
				EnableBbCode = true
			};

			_db.ForumPosts.Add(forumPost);
			await _db.SaveChangesAsync();
		}

		/// <summary>
		/// Returns necessary data to display on the edit post screen
		/// </summary>
		public async Task<ForumPostEditModel> GetEditPostData(int postId, bool seeRestricted)
		{
			var model = await _db.ForumPosts
				.Where(p => seeRestricted || !p.Topic.Forum.Restricted)
				.Select(p => new ForumPostEditModel
				{
					PostId = p.Id,
					CreateTimestamp = p.CreateTimeStamp,
					PosterId = p.PosterId,
					PosterName = p.Poster.UserName,
					EnableBbCode = p.EnableBbCode,
					EnableHtml = p.EnableHtml,
					TopicId = p.TopicId ?? 0,
					TopicTitle = p.Topic.Title,
					Subject = p.Subject,
					Text = p.Text
				})
				.SingleOrDefaultAsync(p => p.PostId == postId);

			var lastPostId = (await _db.ForumPosts
				.Where(p => p.TopicId == model.TopicId)
				.OrderByDescending(p => p.CreateTimeStamp)
				.FirstAsync())
				.Id;

			model.IsLastPost = model.PostId == lastPostId;

			return model;
		}

		/// <summary>
		/// Returns whether or not the poster matches the given user
		/// and that the post is currently the last post of its topic
		/// </summary>
		public async Task<bool> CanEdit(int postId, int userId)
		{
			using (await _db.Database.BeginTransactionAsync())
			{
				var post = await _db.ForumPosts.SingleOrDefaultAsync(p => p.Id == postId);

				if (post == null || post.PosterId != userId)
				{
					return false;
				}

				var lastPostId = await _db.ForumPosts
					.Where(p => p.TopicId == post.TopicId)
					.OrderByDescending(p => p.CreateTimeStamp)
					.Select(p => p.Id)
					.FirstAsync();

				return post.Id == lastPostId;
			}
		}

		/// <summary>
		/// Updates the post with the given post id, with the
		/// given subject and text
		/// </summary>
		public async Task EditPost(ForumPostEditModel model)
		{
			var forumPost = await _db.ForumPosts.SingleAsync(p => p.Id == model.PostId);
			forumPost.Subject = model.Subject;
			forumPost.Text = model.Text;
			await _db.SaveChangesAsync();
		}

		/// <summary>
		/// Adds a vote to the given Poll
		/// </summary>
		/// <returns>Returns the topic id of the poll, if the poll is found</returns>
		public async Task<int?> Vote(User user, int pollId, int ordinal, string ipAddress)
		{
			var pollOption = await _db.ForumPollOptions
				.Include(o => o.Poll)
				.Include(o => o.Votes)
				.SingleOrDefaultAsync(o => o.PollId == pollId && o.Ordinal == ordinal);

			if (pollOption == null)
			{
				return null;
			}

			if (pollOption.Votes.All(v => v.UserId != user.Id))
			{
				pollOption.Votes.Add(new ForumPollOptionVote
				{
					User = user,
					IpAddress = ipAddress
				});
				await _db.SaveChangesAsync();
			}

			return pollOption.Poll.TopicId;
		}

		/// <summary>
		/// Results the vote results of a given <see cref="ForumPoll"/> including
		/// the voters and which options they voted for
		/// </summary>
		public async Task<PollResultModel> GetPollResults(int pollId)
		{
			var poll = await _db.ForumPolls
				.Include(p => p.Topic)
				.Include(p => p.PollOptions)
				.ThenInclude(po => po.Votes)
				.ThenInclude(v => v.User)
				.SingleOrDefaultAsync(p => p.Id == pollId);

			if (poll == null)
			{
				return null;
			}

			return new PollResultModel
			{
				TopicTitle = poll.Topic.Title,
				TopicId = poll.TopicId,
				PollId = pollId,
				Question = poll.Question,
				Votes = poll.PollOptions
					.SelectMany(p => p.Votes)
					.Select(v => new PollResultModel.VoteResult
					{
						UserId = v.UserId,
						UserName = v.User.UserName,
						Ordinal = v.PollOption.Ordinal,
						OptionText = v.PollOption.Text,
						CreateTimestamp = v.CreateTimestamp,
						IpAddress = v.IpAddress
					})
			};
		}

		/// <summary>
		/// Returns a paged list of topics that have no replies (are only the original post that was created)
		/// </summary>
		public async Task<PageOf<UnansweredPostModel>> GetUnansweredPosts(PagedModel paged, bool allowRestricted)
		{
			return await _db.ForumTopics
				.Where(t => allowRestricted || !t.Forum.Restricted)
				.Where(t => t.ForumPosts.Count == 1)
				.Select(t => new UnansweredPostModel
				{
					ForumId = t.ForumId,
					ForumName = t.Forum.Name,
					TopicId = t.Id,
					TopicName = t.Title,
					AuthorId = t.PosterId,
					AuthorName = t.Poster.UserName,
					PostDate = t.CreateTimeStamp
				})
				.OrderByDescending(t => t.PostDate)
				.PageOfAsync(_db, paged);
		}

		/// <summary>
		/// Returns whether or not the topic with the given topic id is currently locked
		/// </summary>
		public async Task<bool> IsTopicLocked(int topicId)
		{
			return await _db.ForumTopics.AnyAsync(t => t.Id == topicId && t.IsLocked);
		}

		public async Task<MoveTopicModel> GetTopicMoveModel(int topicId, bool allowRestricted)
		{
			var model = await _db.ForumTopics
				.Include(t => t.Forum)
				.Where(t => allowRestricted || !t.Forum.Restricted)
				.Select(t => new MoveTopicModel
				{
					TopicId = t.Id,
					TopicTitle = t.Title,
					ForumId = t.Forum.Id,
					ForumName = t.Forum.Name
				})
				.SingleOrDefaultAsync(t => t.TopicId == topicId);

			if (model != null)
			{
				model.AvailableForums = await _db.Forums
					.Where(f => allowRestricted || !f.Restricted)
					.Select(f => new SelectListItem
					{
						Text = f.Name,
						Value = f.Id.ToString(),
						Selected = f.Id == model.ForumId
					})
					.ToListAsync();
			}

			return model;
		}

		public async Task MoveTopic(MoveTopicModel model, bool allowRestricted)
		{
			var topic = await _db.ForumTopics
				.Where(t => allowRestricted || !t.Forum.Restricted)
				.Where(t => t.Id == model.TopicId)
				.SingleOrDefaultAsync();

			if (topic != null)
			{
				topic.ForumId = model.ForumId;
				await _db.SaveChangesAsync();
			}
		}
	}
}
