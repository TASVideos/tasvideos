using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Pages.Profile
{
	[Authorize]
	public class UserFilesModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;

		public UserFilesModel(ApplicationDbContext db, IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
		}

		public string UserName { get; set; } = "";

		public IEnumerable<UserFileModel> Files { get; set; } = new List<UserFileModel>();

		public async Task OnGet()
		{
			UserName = User!.Identity!.Name!;
			Files = await _mapper.ProjectTo<UserFileModel>(
				_db.UserFiles
					.ForAuthor(UserName)
					.FilterByHidden(includeHidden: true))
					.OrderByDescending(uf => uf.UploadTimestamp)
				.ToListAsync();
		}
	}
}
