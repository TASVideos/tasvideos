using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data.Entity;
using TASVideos.Pages.Wiki.Models;

namespace TASVideos.Pages.Wiki;

[AllowAnonymous]
public class EditHistoryModel : BasePageModel
{
	private readonly IWikiPages _wikiPages;
	private readonly IMapper _mapper;

	public EditHistoryModel(IWikiPages wikiPages, IMapper mapper)
	{
		_wikiPages = wikiPages;
		_mapper = mapper;
	}

	[FromRoute]
	public string UserName { get; set; } = "";

	public UserWikiEditHistoryModel History { get; set; } = new();

	public async Task OnGet()
	{
		History = new UserWikiEditHistoryModel
		{
			UserName = UserName,
			Edits = await _mapper.ProjectTo<UserWikiEditHistoryModel.EditEntry>(
				_wikiPages.Query
					.ThatAreNotDeleted()
					.CreatedBy(UserName)
					.ByMostRecent())
				.ToListAsync()
		};
	}
}
