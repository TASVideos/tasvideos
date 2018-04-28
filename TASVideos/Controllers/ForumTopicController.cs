using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TASVideos.ForumEngine;
using TASVideos.Models;
using TASVideos.Tasks;


namespace TASVideos.Controllers
{
	public class ForumTopicController : BaseController
	{
		private readonly ForumTasks _forumTasks;

		public ForumTopicController(
			ForumTasks forumTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_forumTasks = forumTasks;
		}

		[AllowAnonymous]
		public async Task<IActionResult> Index(TopicRequest request)
		{
			var model = await _forumTasks.GetTopicForDisplay(request);

			if (model != null)
			{
				foreach (var post in model.Posts)
				{
					var parsed = PostParser.Parse(post.Text, post.EnableBbCode, post.EnableHtml);
					using (var writer = new StringWriter())
					{
						parsed.WriteHtml(writer);
						post.RenderedText = writer.ToString();
					}

					var sigParsed = PostParser.Parse(post.Signature, post.EnableBbCode, post.EnableHtml);
					using (var writer = new StringWriter())
					{
						sigParsed.WriteHtml(writer);
						post.RenderedSignature = writer.ToString();
					}
				}

				return View(model);
			}

			return NotFound();
		}

		// TODO: permissions, maybe a permission that is auto-added based on post count?
		[Authorize]
		public IActionResult Create(int forumId)
		{
			return new EmptyResult();
		}

		// TODO: permission
		[Authorize]
		[HttpPost]
		public IActionResult GeneratePreview(string post)
		{
			var input = new StreamReader(Request.Body, Encoding.UTF8).ReadToEnd();

			return new ContentResult { Content = "TODO: Genereate Preview" };
		}
	}
}
