using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data.Entity;
using TASVideos.Pages.Wiki.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Wiki
{
	[AllowAnonymous]
	public class EditHistoryModel : BasePageModel
	{
		private readonly IWikiPages _wikiPages;

		public EditHistoryModel(IWikiPages wikiPages)
		{
			_wikiPages = wikiPages;
		}

		[FromRoute]
		public string UserName { get; set; }

		public UserWikiEditHistoryModel History { get; set; }

		public async Task OnGet()
		{
			History = new UserWikiEditHistoryModel
			{
				UserName = UserName,
				Edits = await _wikiPages.Query
					.ThatAreNotDeleted()
					.CreatedBy(UserName)
					.ByMostRecent()
					.ProjectTo<UserWikiEditHistoryModel.EditEntry>()
					.ToListAsync()
			};
		}
	}
}
