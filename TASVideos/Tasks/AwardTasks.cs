using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity.Awards;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Tasks
{
	public class AwardTasks
	{
		private const string AwardCacheKey = "AwardsCache";

		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;
		private readonly IMapper _mapper;

		public AwardTasks(
			ApplicationDbContext db,
			ICacheService cache,
			IMapper mapper)
		{
			_db = db;
			_cache = cache;
			_mapper = mapper;
		}

		public async Task<IEnumerable<AwardDetailsModel>> GetAwardsForModule(int year)
		{
			var allAwards = await AllAwardsCache();

			var model = allAwards
				.Where(a => a.Year + 2000 == year)
				.Select(_mapper.Map<AwardDetailsModel>)
				.ToList();

			return model;
		}

		/// <summary>
		/// Gets all awards for the user, or any movie for which the user is an author of
		/// </summary>
		public async Task<IEnumerable<AwardDisplayModel>> GetAllAwardsForUser(int userId)
		{
			var allAwards = await AllAwardsCache();

			return allAwards
				.Where(a => a.Users.Select(u => u.Id).Contains(userId))
				.Select(ua => new AwardDisplayModel
				{
					ShortName = ua.ShortName,
					Description = ua.Description,
					Year = ua.Year
				})
				.ToList();
		}

		private async Task<IEnumerable<AwardDto>> AllAwardsCache()
		{
			if (_cache.TryGetValue(AwardCacheKey, out IEnumerable<AwardDto> awards))
			{
				return awards;
			}

			// TODO: optimize these with EF 2.1, 2.0 is so bad with GroupBy that it is hopeless
			using (_db.Database.BeginTransactionAsync())
			{
				var userAwards = await _db.UserAwards
					.GroupBy(gkey => new
						{
							gkey.Award.Description, gkey.Award.ShortName, gkey.Year
						}, gvalue => new AwardDto.UserDto
						{
							Id = gvalue.UserId, UserName = gvalue.User.UserName
						})
					.Select(g => new AwardDto
					{
						ShortName = g.Key.ShortName,
						Description = g.Key.Description + " of " + (g.Key.Year + 2000).ToString(),
						Year = g.Key.Year,
						Type = AwardType.User,
						Publications = Enumerable.Empty<AwardDto.PublicationDto>(),
						Users = g.ToList()
					})
					.ToListAsync();

				var pubLists = await _db.PublicationAwards
					.Include(pa => pa.Award)
					.Include(pa => pa.Publication)
					.ThenInclude(pa => pa.Authors)
					.ThenInclude(a => a.Author)
					.ToListAsync();

				var publicationAwards = pubLists
					.GroupBy(gkey => new
						{
							gkey.Award.Description, gkey.Award.ShortName, gkey.Year
						}, gvalue => new
						{
							Publication = new
							{
								Id = gvalue.PublicationId,
								Title = gvalue.Publication.Title
							},
							Users = gvalue.Publication.Authors.Select(a => new
							{
								a.UserId, a.Author.UserName
							})
						})
					.Select(g => new AwardDto
					{
						ShortName = g.Key.ShortName,
						Description = g.Key.Description + " of " + (g.Key.Year + 2000).ToString(),
						Year = g.Key.Year,
						Type = AwardType.Movie,
						Publications = g.Select(gv => new AwardDto.PublicationDto { Id = gv.Publication.Id, Title  = gv.Publication.Title }).ToList(),
						Users = g.SelectMany(gv => gv.Users).Select(u => new AwardDto.UserDto { Id = u.UserId, UserName = u.UserName }).ToList()
					})
					.ToList();

				var allAwards = userAwards.Concat(publicationAwards);

				_cache.Set(AwardCacheKey, allAwards, DurationConstants.OneWeekInSeconds);

				return allAwards;
			}
		}

		public class AwardDto
		{
			public string ShortName { get; set; }
			public string Description { get; set; }
			public int Year { get; set; }
			public AwardType Type { get; set; }
			public IEnumerable<PublicationDto> Publications { get; set; } = new HashSet<PublicationDto>();
			public IEnumerable<UserDto> Users { get; set; } = new HashSet<UserDto>();

			public class UserDto
			{
				public int Id { get; set; }
				public string UserName { get; set; }
			}

			public class PublicationDto
			{
				public int Id { get; set; }
				public string Title { get; set; }
			}
		}
	}
}
