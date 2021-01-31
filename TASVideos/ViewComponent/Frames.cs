﻿using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.WikiEngine;

namespace TASVideos.ViewComponents
{
	[WikiModule(WikiModules.Frames)]
	public class Frames : ViewComponent
	{
		private readonly ApplicationDbContext _db;

		public Frames(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<IViewComponentResult> InvokeAsync(WikiPage pageData, double? fps, int amount)
		{
			var model = new FramesModel
			{
				Amount = amount,
				Fps = fps ?? await GuessFps(pageData.PageName)
			};

			return View(model);
		}

		private async Task<double> GuessFps(string? pageName)
		{
			var submissionId = WikiHelper.IsSubmissionPage(pageName);
			if (submissionId.HasValue)
			{
				var sub = await _db.Submissions
					.Where(s => s.Id == submissionId.Value)
					.Select(s => new { s.Id, s.SystemFrameRate!.FrameRate })
					.SingleOrDefaultAsync(s => s.Id == submissionId.Value);

				if (sub?.FrameRate is not null)
				{
					return sub.FrameRate;
				}

				return 60;
			}

			var publicationId = WikiHelper.IsPublicationPage(pageName);
			if (publicationId.HasValue)
			{
				var pub = await _db.Publications
					.Where(p => p.Id == publicationId.Value)
					.Select(p => new { p.Id, p.SystemFrameRate!.FrameRate })
					.SingleOrDefaultAsync();

				if (pub?.FrameRate is not null)
				{
					return pub.FrameRate;
				}
			}

			return 60;
		}
	}
}
