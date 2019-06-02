using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Pages.Forum.Posts.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Forum.Posts
{
	[Authorize]
	public class NewModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager _userManager;
		private readonly IAwards _awards;
		
		public NewModel(
			ApplicationDbContext db,
			UserManager userManager,
			IAwards awards)
		{
			_db = db;
			_userManager = userManager;
			_awards = awards;
		}

		[FromQuery]
		public PagingModel Search { get; set; }

		public PageOf<PostsSinceLastVisitModel> Posts { get; set; }

		public async Task OnGet()
		{
			var user = await _userManager.GetUserAsync(User);
			var allowRestricted = User.Has(PermissionTo.SeeRestrictedForums);
			var since = user.LastLoggedInTimeStamp ?? DateTime.UtcNow;
			Posts = await _db.ForumPosts
				.ExcludeRestricted(allowRestricted)
				.Since(since)
				.Select(p => new PostsSinceLastVisitModel
				{
					Id = p.Id,
					CreateTimestamp = p.CreateTimeStamp,
					EnableBbCode = p.EnableBbCode,
					EnableHtml = p.EnableHtml,
					Text = p.Text,
					Subject = p.Subject,
					TopicId = p.TopicId ?? 0,
					TopicTitle = p.Topic.Title,
					ForumId = p.Topic.ForumId,
					ForumName = p.Topic.Forum.Name,
					PosterId = p.PosterId,
					PosterName = p.Poster.UserName,
					PosterRoles = p.Poster.UserRoles
						.Where(ur => !ur.Role.IsDefault)
						.Select(ur => ur.Role.Name)
						.ToList(),
					PosterLocation = p.Poster.From,
					Signature = p.Poster.Signature,
					PosterAvatar = p.Poster.Avatar,
					PosterJoined = p.Poster.CreateTimeStamp,
					PosterPostCount = p.Poster.Posts.Count,
				})
				.OrderBy(p => p.CreateTimestamp)
				.PageOf(_db, Search);

			foreach (var post in Posts)
			{
				post.Awards = await _awards.ForUser(post.PosterId);
				post.RenderedText = RenderPost(post.Text, post.EnableBbCode, post.EnableHtml);
				post.RenderedSignature = RenderSignature(post.Signature);
			}
		}
	}
}
