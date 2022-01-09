using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Pages.UserFiles
{
	[AllowAnonymous]
	public class ForUserModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;

		public ForUserModel(ApplicationDbContext db, IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
		}

		[FromQuery]
		public PagingModel Search { get; set; } = new ();

		[FromRoute]
		public string UserName { get; set; } = "";

		public IEnumerable<UserFileModel> Files { get; set; } = new List<UserFileModel>();

		public async Task OnGet()
		{
			Files = await _mapper.ProjectTo<UserFileModel>(
				_db.UserFiles
					.ForAuthor(UserName)
					.FilterByHidden(includeHidden: false))
					.OrderByDescending(uf => uf.UploadTimestamp)
			.PageOf(Search);
		}
	}
}
