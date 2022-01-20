using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Extensions;
using TASVideos.Pages.Forum.Models;
using TASVideos.Pages.Forum.Posts.Models;
using TASVideos.Pages.Forum.Topics.Models;

namespace TASVideos.Pages.Forum
{
	public class BaseForumModel : BasePageModel
	{
		protected static readonly IEnumerable<SelectListItem> TopicTypeList = Enum
			.GetValues(typeof(ForumTopicType))
			.Cast<ForumTopicType>()
			.Select(m => new SelectListItem
			{
				Value = ((int)m).ToString(),
				Text = m.EnumDisplayName()
			})
			.ToList();

		protected static readonly IEnumerable<SelectListItem> MoodList = Enum
			.GetValues(typeof(ForumPostMood))
			.Cast<ForumPostMood>()
			.Select(m => new SelectListItem
			{
				Value = ((int)m).ToString(),
				Text = m.EnumDisplayName(),
				Group = m >= ForumPostMood.AltNormal ? AltGroup : StandardGroup
			})
			.ToList();

		private readonly ApplicationDbContext _db;
		private readonly ITopicWatcher _topicWatcher;
		private readonly IForumService _forumService;

		private static readonly SelectListGroup StandardGroup = new () { Name = "Standard" };
		private static readonly SelectListGroup AltGroup = new () { Name = "Alternate" };

		public BaseForumModel(
			ApplicationDbContext db,
			ITopicWatcher topicWatcher,
			IForumService forumService)
		{
			_db = db;
			_topicWatcher = topicWatcher;
			_forumService = forumService;
		}

		public IEnumerable<SelectListItem> Moods => MoodList;
		public IEnumerable<SelectListItem> TopicTypes => TopicTypeList;

		protected async Task<PostPositionModel?> GetPostPosition(int postId, bool seeRestricted)
		{
			var post = await _db.ForumPosts
				.ExcludeRestricted(seeRestricted)
				.SingleOrDefaultAsync(p => p.Id == postId);

			if (post == null)
			{
				return null;
			}

			var posts = await _db.ForumPosts
				.ForTopic(post.TopicId ?? -1)
				.OldestToNewest()
				.ToListAsync();

			var position = posts.IndexOf(post);
			return new PostPositionModel
			{
				Page = (position / ForumConstants.PostsPerPage) + 1,
				TopicId = post.TopicId ?? 0
			};
		}

		protected async Task<int> CreatePost(int topicId, int forumId, ForumPostModel model, int userId, string userName, string ipAddress, bool watchTopic)
		{
			var forumPost = new ForumPost
			{
				TopicId = topicId,
				ForumId = forumId,
				PosterId = userId,
				IpAddress = ipAddress,
				Subject = model.Subject,
				Text = model.Text,
				PosterMood = model.Mood,

				// New posts are always bbcode = true, html = false
				EnableHtml = false,
				EnableBbCode = true
			};

			_db.ForumPosts.Add(forumPost);
			await _db.SaveChangesAsync();
			_forumService.CacheLatestPost(forumId, topicId, new LatestPost(forumPost.Id, forumPost.CreateTimestamp, userName));

			if (watchTopic)
			{
				await _topicWatcher.WatchTopic(topicId, userId, canSeeRestricted: true);
			}
			else
			{
				await _topicWatcher.UnwatchTopic(topicId, userId);
			}

			return forumPost.Id;
		}

		protected async Task CreatePoll(ForumTopic topic, PollCreateModel model)
		{
			var poll = new ForumPoll
			{
				TopicId = topic.Id,
				Question = model.Question ?? "",
				CloseDate = model.DaysOpen.HasValue
					? DateTime.UtcNow.AddDays(model.DaysOpen.Value)
					: null,
				MultiSelect = model.MultiSelect,
				PollOptions = model.PollOptions.Select((po, i) => new ForumPollOption
				{
					Text = po,
					Ordinal = i
				})
				.ToList()
			};

			_db.ForumPolls.Add(poll);
			topic.Poll = poll;
			await _db.SaveChangesAsync();
		}

		public new IActionResult NotFound()
		{
			return RedirectToPage("/Forum/NotFound");
		}
	}
}
