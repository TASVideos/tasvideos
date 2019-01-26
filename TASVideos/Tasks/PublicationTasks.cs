using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Tasks
{
	public class PublicationTasks
	{
		private readonly ApplicationDbContext _db;
		private readonly ICacheService _cache;
		private readonly IMapper _mapper;
		private readonly IWikiPages _wikiPages;
		
		public PublicationTasks(
			ApplicationDbContext db,
			ICacheService cache,
			IMapper mapper,
			IWikiPages wikiPages)
		{
			_db = db;
			_cache = cache;
			_mapper = mapper;
			_wikiPages = wikiPages;
		}

		/// <summary>
		/// Gets all the possible values that can be tokens in the Movies- url
		/// </summary>
		public async Task<PublicationSearchModel> GetMovieTokenData()
		{
			var cacheKey = $"{nameof(PublicationTasks)}{nameof(GetMovieTokenData)}";
			if (_cache.TryGetValue(cacheKey, out PublicationSearchModel cachedResult))
			{
				return cachedResult;
			}

			using (await _db.Database.BeginTransactionAsync())
			{
				var result = new PublicationSearchModel
				{
					Tiers = await _db.Tiers.Select(t => t.Name.ToLower()).ToListAsync(),
					SystemCodes = await _db.GameSystems.Select(s => s.Code.ToLower()).ToListAsync(),
					Tags = await _db.Tags.Select(t => t.Code.ToLower()).ToListAsync(),
					Genres = await _db.Genres.Select(g => g.DisplayName.ToLower()).ToListAsync(),
					Flags = await _db.Flags.Select(f => f.Token.ToLower()).ToListAsync()
				};

				_cache.Set(cacheKey, result);

				return result;
			}
		}

		/// <summary>
		/// Gets the title of a movie with the given id
		/// If the movie is not found, null is returned
		/// </summary>
		public async Task<string> GetTitle(int id)
		{
			return (await _db.Publications
				.Select(s => new { s.Id, s.Title })
				.SingleOrDefaultAsync(s => s.Id == id))?.Title;
		}

		// TODO: paging
		/// <summary>
		/// Returns a list of publications with the given <see cref="searchCriteria" />
		/// for the purpose of displaying on a movie listings page
		/// </summary>
		public async Task<IEnumerable<PublicationModel>> GetMovieList(PublicationSearchModel searchCriteria)
		{
			var query = _db.Publications
				.AsQueryable();

			if (searchCriteria.MovieIds.Any())
			{
				query = query.Where(p => searchCriteria.MovieIds.Contains(p.Id));
			}
			else
			{
				if (searchCriteria.SystemCodes.Any())
				{
					query = query.Where(p => searchCriteria.SystemCodes.Contains(p.System.Code));
				}

				if (searchCriteria.Tiers.Any())
				{
					query = query.Where(p => searchCriteria.Tiers.Contains(p.Tier.Name));
				}

				if (!searchCriteria.ShowObsoleted)
				{
					query = query.ThatAreCurrent();
				}

				if (searchCriteria.Years.Any())
				{
					query = query.Where(p => searchCriteria.Years.Contains(p.CreateTimeStamp.Year));
				}

				if (searchCriteria.Tags.Any())
				{
					query = query.Where(p => p.PublicationTags.Any(t => searchCriteria.Tags.Contains(t.Tag.Code)));
				}

				if (searchCriteria.Genres.Any())
				{
					query = query.Where(p => p.Game.GameGenres.Any(gg => searchCriteria.Genres.Contains(gg.Genre.DisplayName)));
				}

				if (searchCriteria.Flags.Any())
				{
					query = query.Where(p => p.PublicationFlags.Any(f => searchCriteria.Flags.Contains(f.Flag.Token)));
				}

				if (searchCriteria.Authors.Any())
				{
					query = query.Where(p => p.Authors.Select(a => a.UserId).Any(a => searchCriteria.Authors.Contains(a)));
				}
			}

			// TODO: AutoMapper, single movie is the same logic
			return await query
				.OrderBy(p => p.System.Code)
				.ThenBy(p => p.Game.DisplayName)
				.Select(p => new PublicationModel
				{
					Id = p.Id,
					CreateTimeStamp = p.CreateTimeStamp,
					Title = p.Title,
					OnlineWatchingUrl = p.OnlineWatchingUrl,
					MirrorSiteUrl = p.MirrorSiteUrl,
					ObsoletedBy = p.ObsoletedById,
					MovieFileName = p.MovieFileName,
					SubmissionId = p.SubmissionId,
					RatingCount = p.PublicationRatings.Count / 2,
					TierIconPath = p.Tier.IconPath,
					Files = p.Files.Select(f => new PublicationModel.FileModel
					{
						Path = f.Path,
						Type = f.Type
					}).ToList(),
					Tags = p.PublicationTags
						.Select(pt => new PublicationModel.TagModel
						{
							DisplayName = pt.Tag.DisplayName,
							Code = pt.Tag.Code
						})
						.ToList(),
					GenreTags = p.Game.GameGenres
						.Select(gg => new PublicationModel.TagModel
						{
							DisplayName = gg.Genre.DisplayName,
							Code = gg.Genre.DisplayName // TODO
						})
						.ToList(),
					Flags = p.PublicationFlags
						.Where(pf => pf.Flag.IconPath != null)
						.Select(pf => new PublicationModel.FlagModel
						{
							IconPath = pf.Flag.IconPath,
							LinkPath = pf.Flag.LinkPath,
							Name = pf.Flag.Name
						})
						.ToList()
				})
				.ToListAsync();
		}

		// TODO: document
		public async Task UpdatePublication(int id, PublicationEditModel model)
		{
			var publication = await _db.Publications
				.Include(p => p.WikiContent)
				.Include(p => p.System)
				.Include(p => p.SystemFrameRate)
				.Include(p => p.Game)
				.Include(p => p.Authors)
				.ThenInclude(pa => pa.Author)
				.SingleOrDefaultAsync(p => p.Id == id);

			if (publication != null)
			{
				publication.Branch = model.Branch;
				publication.ObsoletedById = model.ObsoletedBy;
				publication.EmulatorVersion = model.EmulatorVersion;
				publication.OnlineWatchingUrl = model.OnlineWatchingUrl;
				publication.MirrorSiteUrl = model.MirrorSiteUrl;

				publication.GenerateTitle();

				publication.PublicationFlags.Clear();
				_db.PublicationFlags.RemoveRange(
					_db.PublicationFlags.Where(pf => pf.PublicationId == publication.Id));

				foreach (var flag in model.SelectedFlags)
				{
					publication.PublicationFlags.Add(new PublicationFlag
					{
						PublicationId = publication.Id,
						FlagId = flag
					});
				}

				publication.PublicationTags.Clear();
				_db.PublicationTags.RemoveRange(
					_db.PublicationTags.Where(pt => pt.PublicationId == publication.Id));

				foreach (var tag in model.SelectedTags)
				{
					publication.PublicationTags.Add(new PublicationTag
					{
						PublicationId = publication.Id,
						TagId = tag
					});
				}

				await _db.SaveChangesAsync();

				if (model.Markup != publication.WikiContent.Markup)
				{
					var revision = new WikiPage
					{
						PageName = $"{LinkConstants.PublicationWikiPage}{id}",
						Markup = model.Markup,
						MinorEdit = model.MinorEdit,
						RevisionMessage = model.RevisionMessage,
					};
					await _wikiPages.Add(revision);

					publication.WikiContentId = revision.Id;
				}
			}
		}

		/// <summary>
		/// Returns the <see cref="Publication"/> with the given <see cref="id"/>
		/// for the purpose of setting <see cref="TASVideos.Data.Entity.Game.Game"/> cataloging information.
		/// If no publication is found, null is returned
		/// </summary>
		public async Task<PublicationCatalogModel> Catalog(int id)
		{
			using (_db.Database.BeginTransactionAsync())
			{
				var model = await _db.Publications
					.Where(p => p.Id == id)
					.Select(p => new PublicationCatalogModel
					{
						RomId = p.RomId,
						GameId = p.GameId,
						SystemId = p.SystemId,
						SystemFrameRateId = p.SystemFrameRateId,
					})
					.SingleAsync();

				if (model == null)
				{
					return null;
				}

				await PopulateCatalogDropDowns(model);
				return model;
			}
		}

		public async Task PopulateCatalogDropDowns(PublicationCatalogModel model)
		{
			using (_db.Database.BeginTransactionAsync())
			{
				model.AvailableRoms = await _db.Roms
					.ForGame(model.GameId)
					.ForSystem(model.SystemId)
					.Select(r => new SelectListItem
					{
						Value = r.Id.ToString(),
						Text = r.Name
					})
					.ToListAsync();

				model.AvailableGames = await _db.Games
					.ForSystem(model.SystemId)
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

				model.AvailableSystemFrameRates = await _db.GameSystemFrameRates
					.ForSystem(model.SystemId)
					.Select(sf => new SelectListItem
					{
						Value = sf.Id.ToString(),
						Text = sf.RegionCode + " (" + sf.FrameRate + ")"
					})
					.ToListAsync();
			}
		}

		/// <summary>
		/// Updates the given <see cref="Publication"/> with the given <see cref="TASVideos.Data.Entity.Game.Game"/> catalog information
		/// </summary>
		public async Task UpdateCatalog(int id, PublicationCatalogModel model)
		{
			var publication = await _db.Publications.SingleAsync(s => s.Id == id);
			_mapper.Map(model, publication);
			await _db.SaveChangesAsync();
		}

		// TODO: document
		public async Task<PublicationTierEditModel> GetTiersForEdit(int publicationId)
		{
			var model = await _db.Publications
				.Where(p => p.Id == publicationId)
				.Select(p => new PublicationTierEditModel
				{
					Id = p.Id,
					Title = p.Title,
					TierId = p.TierId 
				})
				.SingleOrDefaultAsync();

			if (model != null)
			{
				model.AvailableTiers = await _db.Tiers
					.Select(t => new SelectListItem
					{
						Text = t.Name,
						Value = t.Id.ToString()
					})
					.ToListAsync();
			}

			return model;
		}

		// TODO: document
		public async Task<bool> UpdateTier(int publicationId, int tierId)
		{
			var publication = await _db.Publications
				.SingleOrDefaultAsync(p => p.Id == publicationId);

			if (publication == null)
			{
				return false;
			}

			var tier = await _db.Tiers.SingleOrDefaultAsync(t => t.Id == tierId);
			if (tier == null)
			{
				return false;
			}

			publication.TierId = tierId;
			await _db.SaveChangesAsync();
			return true;
		}

		/// <summary>
		/// Returns movie rating data for hte given user and publication
		/// </summary>
		public async Task<PublicationRateModel> GetRatingModel(User user, int publicationId)
		{
			if (user == null)
			{
				throw new ArgumentException($"{nameof(user)} can not be null.");
			}

			var publication = await _db.Publications.SingleOrDefaultAsync(p => p.Id == publicationId);
			if (publication == null)
			{
				return null;
			}

			var ratings = await _db.PublicationRatings
				.ForPublication(publicationId)
				.ForUser(user.Id)
				.ToListAsync();
			return new PublicationRateModel
			{
				Title = publication.Title,
				TechRating = ratings
					.SingleOrDefault(r => r.Type == PublicationRatingType.TechQuality)
					?.Value,
				EntertainmentRating = ratings
					.SingleOrDefault(r => r.Type == PublicationRatingType.Entertainment)
					?.Value
			};
		}

		/// <summary>
		/// Inserts or updates a movie rating for the given publication and user
		/// </summary>
		public async Task RatePublication(int id, PublicationRateModel model, User user)
		{
			if (user == null)
			{
				throw new ArgumentException($"{nameof(user)} can not be null.");
			}

			if (!model.TechRating.HasValue && !model.EntertainmentRating.HasValue)
			{
				throw new ArgumentException("At least one rating must be set");
			}

			var ratings = await _db.PublicationRatings
				.ForPublication(id)
				.ForUser(user.Id)
				.ToListAsync();

			var tech = ratings
				.SingleOrDefault(r => r.Type == PublicationRatingType.TechQuality);

			var entertainment = ratings
				.SingleOrDefault(r => r.Type == PublicationRatingType.Entertainment);

			UpdateRating(tech, id, user.Id, PublicationRatingType.TechQuality, model.TechRating);
			UpdateRating(entertainment, id, user.Id, PublicationRatingType.Entertainment, model.EntertainmentRating);

			await _db.SaveChangesAsync();
		}

		private void UpdateRating(PublicationRating rating, int id, int userId, PublicationRatingType type, double? value)
		{
			if (rating != null)
			{
				if (value.HasValue)
				{
					// Update
					rating.Value = value.Value;
				}
				else
				{
					// Remove
					_db.PublicationRatings.Remove(rating);
				}
			}
			else
			{
				if (value.HasValue)
				{
					// Add
					_db.PublicationRatings.Add(new PublicationRating
					{
						PublicationId = id,
						UserId = userId,
						Type = type,
						Value = value.Value
					});
				}

				// Else do nothing
			}
		}
	}
}
