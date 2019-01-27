using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Forum;
using TASVideos.Models;

namespace TASVideos.Tasks
{
	public class SubmissionTasks
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;
		private readonly IHostingEnvironment _hostingEnvironment;

		public SubmissionTasks(
			ApplicationDbContext db,
			IMapper mapper,
			IHostingEnvironment hostingEnvironment)
		{
			_db = db;
			_mapper = mapper;
			_hostingEnvironment = hostingEnvironment;
		}

		/// <summary>
		/// Gets a list of <see cref="Submission"/>s for the submission queue filtered on the given <see cref="criteria" />
		/// </summary>
		public async Task<SubmissionListModel> GetSubmissionList(SubmissionSearchRequest criteria)
		{
			var iquery = _db.Submissions
				.Include(s => s.Submitter)
				.Include(s => s.System)
				.Include(s => s.SystemFrameRate)
				.Include(s => s.SubmissionAuthors)
				.ThenInclude(sa => sa.Author);

			IQueryable<Submission> query = iquery.AsQueryable();

			if (!string.IsNullOrWhiteSpace(criteria.User))
			{
				query = iquery.Where(s => s.SubmissionAuthors.Any(sa => sa.Author.UserName == criteria.User)
					|| s.Submitter.UserName == criteria.User);
			}

			if (criteria.Cutoff.HasValue)
			{
				query = query.Where(s => s.CreateTimeStamp >= criteria.Cutoff.Value);
			}

			if (criteria.StatusFilter.Any())
			{
				query = query.Where(s => criteria.StatusFilter.Contains(s.Status));
			}

			if (criteria.Limit.HasValue)
			{
				query = query.Take(criteria.Limit.Value);
			}

			// It is important to actually query for an Entity object here instead of a ViewModel
			// Because we need the title property which is a derived property that can't be done in Linq to Sql
			// And needs a variety of information from sub-tables, hence all the includes
			var results = await query.ToListAsync();
			return new SubmissionListModel
			{
				User = criteria.User,
				StatusFilter = criteria.StatusFilter
					.Cast<int>()
					.ToList(),
				Entries = results.Select(s => new SubmissionListModel.Entry
				{
					Id = s.Id,
					System = s.System.Code,
					GameName = s.GameName,
					Time = s.Time,
					Branch = s.Branch,
					Author = string.Join(" & ", s.SubmissionAuthors.Select(sa => sa.Author.UserName).ToList()),
					Submitted = s.CreateTimeStamp,
					Status = s.Status
				}) 
			};
		}

		/// <summary>
		/// Gets the title of a submission with the given id
		/// If the submission is not found, null is returned
		/// </summary>
		public async Task<string> GetTitle(int id)
		{
			return (await _db.Submissions
				.Select(s => new { s.Id, s.Title })
				.SingleOrDefaultAsync(s => s.Id == id))?.Title;
		}

		/// <summary>
		/// Returns the <see cref="Submission"/> with the given <see cref="id"/>
		/// for the purpose of setting <see cref="TASVideos.Data.Entity.Game.Game"/> cataloging information.
		/// If no submission is found, null is returned
		/// </summary>
		public async Task<SubmissionCatalogModel> Catalog(int id)
		{
			using (_db.Database.BeginTransactionAsync())
			{
				var model = await _db.Submissions
					.Where(s => s.Id == id)
					.Select(s => new SubmissionCatalogModel
					{
						RomId = s.RomId,
						GameId = s.GameId,
						SystemId = s.SystemId,
						SystemFrameRateId = s.SystemFrameRateId,
					})
					.SingleOrDefaultAsync();

				if (model == null)
				{
					return null;
				}

				await PopulateCatalogDropDowns(model);
				return model;
			}
		}

		public async Task PopulateCatalogDropDowns(SubmissionCatalogModel model)
		{
			using (_db.Database.BeginTransactionAsync())
			{
				model.AvailableRoms = await _db.Roms
					.Where(r => !model.SystemId.HasValue || r.Game.SystemId == model.SystemId)
					.Where(r => !model.GameId.HasValue || r.GameId == model.GameId)
					.Select(r => new SelectListItem
					{
						Value = r.Id.ToString(),
						Text = r.Name
					})
					.ToListAsync();

				model.AvailableGames = await _db.Games
					.Where(g => !model.SystemId.HasValue || g.SystemId == model.SystemId)
					.Select(g => new SelectListItem
					{
						Value = g.Id.ToString(),
						Text = g.GoodName
					})
					.ToListAsync();

				model.AvailableSystems = await _db.GameSystems
					.Select(s => new SelectListItem
					{
						Value = s.Id.ToString(),
						Text = s.Code
					})
					.ToListAsync();

				model.AvailableSystemFrameRates = model.SystemId.HasValue
					? await _db.GameSystemFrameRates
						.Where(sf => sf.GameSystemId == model.SystemId)
						.Select(sf => new SelectListItem
						{
							Value = sf.Id.ToString(),
							Text = sf.RegionCode + " (" + sf.FrameRate + ")"
						})
						.ToListAsync()
					: new List<SelectListItem>();
			}
		}

		/// <summary>
		/// Updates the given <see cref="Submission"/> with the given <see cref="TASVideos.Data.Entity.Game.Game"/> catalog information
		/// </summary>
		public async Task UpdateCatalog(int id, SubmissionCatalogModel model)
		{
			var submission = await _db.Submissions.SingleAsync(s => s.Id == id);
			_mapper.Map(model, submission);
			await _db.SaveChangesAsync();
		}
	}
}
