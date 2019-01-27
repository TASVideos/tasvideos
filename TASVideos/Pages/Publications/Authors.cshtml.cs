using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Publications
{
	[AllowAnonymous]
	public class AuthorsModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;

		public AuthorsModel(
			ApplicationDbContext db,
			UserManager userManager) 
			: base(userManager)
		{
			_db = db;
		}

		public IEnumerable<AuthorListEntry> Authors { get; set; } = new List<AuthorListEntry>();

		public async Task OnGet()
		{
			Authors = await _db.Users
				.Where(u => u.Publications.Any())
				.Select(u => new AuthorListEntry
				{
					Id = u.Id,
					UserName = u.UserName,
					ActivePublicationCount = u.Publications.Count(pa => !pa.Publication.ObsoletedById.HasValue),
					ObsoletePublicationCount = u.Publications.Count(pa => pa.Publication.ObsoletedById.HasValue)
				})
				.ToListAsync();
		}
	}
}
