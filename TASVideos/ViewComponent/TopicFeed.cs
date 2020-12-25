using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.ForumEngine;

namespace TASVideos.ViewComponents
{
    public class TopicFeed : ViewComponent
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;

		public TopicFeed(ApplicationDbContext db, IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
		}

		public async Task<IViewComponentResult> InvokeAsync(string pp)
		{
			int limit = ParamHelper.GetInt(pp, "l") ?? 5;
			int topicId = ParamHelper.GetInt(pp, "t")
				?? throw new ArgumentException("the parameter t can not be null");

			var model = new TopicFeedModel
			{
				RightAlign = ParamHelper.HasParam(pp, "right"),
				Heading = ParamHelper.GetValueFor(pp, "heading"),
				HideContent = ParamHelper.HasParam(pp, "hidecontent"),
				Posts = await _mapper.ProjectTo<TopicFeedModel.TopicPost>(
						_db.ForumPosts
						.ForTopic(topicId)
						.ExcludeRestricted(false) // By design, let's not allow restricted topics as wiki feeds
						.ByMostRecent())
					.Take(limit)
					.ToListAsync()
			};

			foreach (var post in model.Posts)
			{
				post.RenderedText = RenderPost(post.Text, post.EnableBbCode, post.EnableHtml);
			}

			return View(model);
		}

		// TODO: this is the same code in BaseController
		private static string RenderPost(string text, bool useBbCode, bool useHtml)
		{
			var parsed = PostParser.Parse(text, useBbCode, useHtml);
			using var writer = new StringWriter();
			parsed.WriteHtml(writer);
			return writer.ToString();
		}
	}
}
