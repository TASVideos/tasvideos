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
using TASVideos.Models;
using TASVideos.MovieParsers;
using TASVideos.WikiEngine;

namespace TASVideos.Tasks
{
	public class SubmissionTasks
	{
		private readonly ApplicationDbContext _db;
		private readonly MovieParser _parser;
		private readonly WikiTasks _wikiTasks;
		private readonly IMapper _mapper;
		private readonly IHostingEnvironment _hostingEnvironment;

		public SubmissionTasks(
			ApplicationDbContext db,
			MovieParser parser,
			WikiTasks wikiTasks,
			IMapper mapper,
			IHostingEnvironment hostingEnvironment)
		{
			_db = db;
			_parser = parser;
			_wikiTasks = wikiTasks;
			_mapper = mapper;
			_hostingEnvironment = hostingEnvironment;
		}

		/// <summary>
		/// Gets a list of available <see cref="Tier" />s
		/// </summary>
		public async Task<IEnumerable<SelectListItem>> GetAvailableTiers()
		{
			return await _db.Tiers
				.Select(t => new SelectListItem
				{
					Value = t.Id.ToString(),
					Text = t.Name
				})
				.ToListAsync();
		}

		/// <summary>
		/// Gets a list of <see cref="Submission"/>s for the submission queue filtered on the given <see cref="criteria" />
		/// </summary>
		public async Task<SubmissionListViewModel> GetSubmissionList(SubmissionSearchRequest criteria)
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
			// And needs a varierty of information from sub-tables, hence all the includes
			var results = await query.ToListAsync();
			return new SubmissionListViewModel
			{
				User = criteria.User,
				StatusFilter = criteria.StatusFilter
					.Cast<int>()
					.ToList(),
				Entries = results.Select(s => new SubmissionListViewModel.Entry
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
		/// Returns data for a <see cref="Submission"/> with the given <see cref="id" />
		/// for the purpose of display
		/// If a submission can not be found, null is returned
		/// </summary>
		public async Task<SubmissionViewModel> GetSubmission(int id, string userName)
		{
			using (_db.Database.BeginTransactionAsync())
			{
				var submissionModel = await _db.Submissions
					.Where(s => s.Id == id)
					.Select(s => new SubmissionViewModel // It is important to use a projection here to avoid querying the file data which is not needed and can be slow
					{
						Id = s.Id,
						SystemDisplayName = s.System.DisplayName,
						SystemCode = s.System.Code,
						GameName = s.GameName,
						GameVersion = s.GameVersion,
						RomName = s.RomName,
						Branch = s.Branch,
						Emulator = s.EmulatorVersion,
						FrameCount = s.Frames,
						FrameRate = s.SystemFrameRate.FrameRate,
						RerecordCount = s.RerecordCount,
						CreateTimestamp = s.CreateTimeStamp,
						Submitter = s.Submitter.UserName,
						LastUpdateTimeStamp = s.WikiContent.LastUpdateTimeStamp,
						LastUpdateUser = s.WikiContent.LastUpdateUserName,
						Status = s.Status,
						EncodeEmbedLink = s.EncodeEmbedLink,
						Judge = s.Judge != null ? s.Judge.UserName : "",
						Title = s.Title,
						TierName = s.IntendedTier != null ? s.IntendedTier.Name : "",
						Publisher = s.Publisher != null ? s.Publisher.UserName : "",
						SystemId = s.SystemId,
						SystemFrameRateId = s.SystemFrameRateId,
						GameId = s.GameId,
						RomId = s.RomId
					})
					.SingleOrDefaultAsync();

				if (submissionModel != null)
				{
					submissionModel.Authors = await _db.SubmissionAuthors
						.Where(sa => sa.SubmissionId == submissionModel.Id)
						.Select(sa => sa.Author.UserName)
						.ToListAsync();

					submissionModel.CanEdit = !string.IsNullOrWhiteSpace(userName)
						&& (userName == submissionModel.Submitter
							|| submissionModel.Authors.Contains(userName));
				}

				return submissionModel;
			}
		}

		/// <summary>
		/// Returns the submission file as bytes with the given id
		/// If no submission is found, an empty byte array is returned
		/// </summary>
		public async Task<byte[]> GetSubmissionFile(int id)
		{
			var data = await _db.Submissions
				.Where(s => s.Id == id)
				.Select(s => s.MovieFile)
				.SingleOrDefaultAsync();

			return data ?? new byte[0];
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
		/// Returns values necessary to determine which <see cref="SubmissionStatus" /> can be assigned to a <seealso cref="Submission"/>
		/// </summary>
		public async Task<SubmissionStatusValidationModel> GetStatusVerificationValues(int id, string userName)
		{
			return await _db.Submissions
				.Where(s => s.Id == id)
				.Select(s => new SubmissionStatusValidationModel
				{
					UserIsJudge = s.Judge != null && s.Judge.UserName == userName,
					UserIsAuhtorOrSubmitter = s.Submitter.UserName == userName || s.SubmissionAuthors.Any(sa => sa.Author.UserName == userName),
					CurrentStatus = s.Status,
					CreateDate = s.CreateTimeStamp
				})
				.SingleAsync();
		}

		/// <summary>
		/// Takes the given data and generates a movie submission
		/// If successful, the Success flag will be true, and the Id field will be
		/// greater than 0 and represent the id to the newly created submission
		/// If unsuccessful, the success flag will be false, Id will be 0 and the 
		/// Errors property will be filled with any relevant error messages
		/// </summary>
		public async Task<SubmitResult> SubmitMovie(SubmissionCreateViewModel model, string userName)
		{
			var submission = _mapper.Map<Submission>(model);

			// Parse movie file
			// TODO: check warnings
			var parseResult = _parser.Parse(model.MovieFile.OpenReadStream());
			if (parseResult.Success)
			{
				using (_db.Database.BeginTransaction())
				{
					submission.Frames = parseResult.Frames;
					submission.RerecordCount = parseResult.RerecordCount;
					submission.MovieExtension = parseResult.FileExtension;
					submission.System = await _db.GameSystems.SingleOrDefaultAsync(g => g.Code == parseResult.SystemCode);

					if (submission.System == null)
					{
						return new SubmitResult($"Unknown system type of {parseResult.SystemCode}");
					}

					submission.Submitter = await _db.Users.SingleAsync(u => u.UserName == userName);
					submission.SystemFrameRate = await _db.GameSystemFrameRates
						.SingleOrDefaultAsync(f => f.GameSystemId == submission.System.Id
							&& f.RegionCode == parseResult.Region.ToString());
				}
			}
			else
			{
				return new SubmitResult(parseResult.Errors);
			}

			using (var memoryStream = new MemoryStream())
			{
				await model.MovieFile.CopyToAsync(memoryStream);
				submission.MovieFile = memoryStream.ToArray();
			}

			_db.Submissions.Add(submission);
			await _db.SaveChangesAsync();

			// Create a wiki page corresponding to this submission
			var wikiPage = new WikiPage
			{
				RevisionMessage = $"Auto-generated from Submission #{submission.Id}",
				PageName = LinkConstants.SubmissionWikiPage + submission.Id,
				MinorEdit = false,
				Markup = model.Markup
			};

			_db.WikiPages.Add(wikiPage);

			submission.WikiContent = wikiPage;

			// Add authors
			var users = await _db.Users
				.Where(u => model.Authors.Contains(u.UserName))
				.ToListAsync();

			var submissionAuthors = users.Select(u => new SubmissionAuthor
			{
				SubmissionId = submission.Id,
				UserId = u.Id
			});

			_db.SubmissionAuthors.AddRange(submissionAuthors);

			submission.GenerateTitle();
			await _db.SaveChangesAsync();

			return new SubmitResult(submission.Id);
		}

		/// <summary>
		/// Returns data for a <see cref="Submission"/> with the given <see cref="id" />
		/// for the purpose of edit
		/// If a submission can not be found, null is returned
		/// </summary>
		public async Task<SubmissionEditModel> GetSubmissionForEdit(int id)
		{
			using (_db.Database.BeginTransactionAsync())
			{
				var submissionModel = await _db.Submissions
					.Where(s => s.Id == id)
					.Select(s => new SubmissionEditModel // It is important to use a projection here to avoid querying the file data which not needed and can be slow
					{
						Id = s.Id,
						SystemDisplayName = s.System.DisplayName,
						SystemCode = s.System.Code,
						GameName = s.GameName,
						GameVersion = s.GameVersion,
						RomName = s.RomName,
						Branch = s.Branch,
						Emulator = s.EmulatorVersion,
						FrameCount = s.Frames,
						FrameRate = s.SystemFrameRate.FrameRate,
						RerecordCount = s.RerecordCount,
						CreateTimestamp = s.CreateTimeStamp,
						Submitter = s.Submitter.UserName,
						LastUpdateTimeStamp = s.WikiContent.LastUpdateTimeStamp,
						LastUpdateUser = s.WikiContent.LastUpdateUserName,
						Status = s.Status,
						EncodeEmbedLink = s.EncodeEmbedLink,
						Markup = s.WikiContent.Markup,
						Judge = s.Judge != null ? s.Judge.UserName : "",
						TierId = s.IntendedTierId
					})
					.SingleOrDefaultAsync();

				if (submissionModel != null)
				{
					submissionModel.Authors = await _db.SubmissionAuthors
						.Where(sa => sa.SubmissionId == submissionModel.Id)
						.Select(sa => sa.Author.UserName)
						.ToListAsync();

					submissionModel.AvailableTiers = await _db.Tiers
						.Select(t => new SelectListItem
						{
							Value = t.Id.ToString(),
							Text = t.Name
						})
						.ToListAsync();
				}

				return submissionModel;
			}
		}

		/// <summary>
		/// Updates an existing <see cref="Submission"/> with the given values
		/// </summary>
		public async Task<SubmitResult> UpdateSubmission(SubmissionEditModel model, string userName)
		{
			var submission = await _db.Submissions
				.Include(s => s.Judge)
				.Include(s => s.Publisher)
				.Include(s => s.System)
				.Include(s => s.SystemFrameRate)
				.Include(s => s.SubmissionAuthors)
				.ThenInclude(sa => sa.Author)
				.SingleAsync(s => s.Id == model.Id);

			// Parse movie file if it exists
			if (model.MovieFile != null)
			{
				// TODO: check warnings
				var parseResult = _parser.Parse(model.MovieFile.OpenReadStream());
				if (parseResult.Success)
				{
					submission.Frames = parseResult.Frames;
					submission.RerecordCount = parseResult.RerecordCount;
					submission.System = await _db.GameSystems.SingleOrDefaultAsync(g => g.Code == parseResult.SystemCode);
					if (submission.System == null)
					{
						return new SubmitResult($"Unknown system type of {parseResult.SystemCode}");
					}

					submission.SystemFrameRate = await _db.GameSystemFrameRates
						.SingleOrDefaultAsync(f => f.GameSystemId == submission.System.Id
							&& f.RegionCode == parseResult.Region.ToString());
				}
				else
				{
					return new SubmitResult(parseResult.Errors);
				}

				using (var memoryStream = new MemoryStream())
				{
					await model.MovieFile.CopyToAsync(memoryStream);
					submission.MovieFile = memoryStream.ToArray();
				}
			}

			// If a judge is claiming the submission
			if (model.Status == SubmissionStatus.JudgingUnderWay
				&& submission.Status != SubmissionStatus.JudgingUnderWay)
			{
				submission.Judge = await _db.Users.SingleAsync(u => u.UserName == userName);
			}
			else if (submission.Status == SubmissionStatus.JudgingUnderWay // If judge is unclaiming, remove them
				&& model.Status == SubmissionStatus.New
				&& submission.Judge != null)
			{
				submission.Judge = null;
			}

			if (model.Status == SubmissionStatus.PublicationUnderway
				&& submission.Status != SubmissionStatus.PublicationUnderway)
			{
				submission.Publisher = await _db.Users.SingleAsync(u => u.UserName == userName);
			}
			else if (submission.Status == SubmissionStatus.Accepted // If publisher is unclaiming, remove them
				&& model.Status == SubmissionStatus.PublicationUnderway)
			{
				submission.Publisher = null;
			}

			if (submission.Status != model.Status)
			{
				var history = new SubmissionStatusHistory
				{
					SubmissionId = submission.Id,
					Status = model.Status
				};
				submission.History.Add(history);
				_db.SubmissionStatusHistory.Add(history);
			}

			if (model.TierId.HasValue)
			{
				submission.IntendedTier = await _db.Tiers.SingleAsync(t => t.Id == model.TierId.Value);
			}
			else
			{
				submission.IntendedTier = null;
			}

			submission.GameVersion = model.GameVersion;
			submission.GameName = model.GameName;
			submission.EmulatorVersion = model.Emulator;
			submission.Branch = model.Branch;
			submission.RomName = model.RomName;
			submission.EncodeEmbedLink = model.EncodeEmbedLink;
			submission.Status = model.Status;

			var id = await _wikiTasks.SavePage(new WikiEditModel
			{
				PageName = $"{LinkConstants.SubmissionWikiPage}{model.Id}",
				Markup = model.Markup,
				MinorEdit = model.MinorEdit,
				RevisionMessage = model.RevisionMessage,
				Referrals = Util.GetAllWikiLinks(model.Markup)
					.Select(wl => new WikiReferralModel
					{
						Link = wl.Link,
						Excerpt = wl.Excerpt
					})
			});

			submission.WikiContent = await _db.WikiPages.SingleAsync(wp => wp.Id == id);

			submission.GenerateTitle();
			await _db.SaveChangesAsync();

			return new SubmitResult(submission.Id);
		}

		/// <summary>
		/// Returns whether or not the <see cref="Submission"/> with the given <see cref="id"/>
		/// is ready to be published
		/// </summary>
		public async Task<bool> CanPublish(int id)
		{
			return await _db.Submissions
				.AnyAsync(s => s.Id == id
					&& s.SystemId.HasValue
					&& s.SystemFrameRateId.HasValue
					&& s.GameId.HasValue
					&& s.RomId.HasValue
					&& s.IntendedTierId.HasValue
					&& s.Status == SubmissionStatus.PublicationUnderway);
		}

		/// <summary>
		/// Returns the <see cref="Submission"/> with the given <see cref="id"/>
		/// for the purpose of publishing
		/// </summary>
		public async Task<SubmissionPublishModel> GetSubmissionForPublish(int id)
		{
			using (_db.Database.BeginTransactionAsync())
			{
				var model = await _db.Submissions
					.Where(s => s.Id == id)
					.Select(s => new SubmissionPublishModel
					{
						Id = s.Id,
						Title = s.Title,
						Markup = s.WikiContent.Markup,
						SystemCode = s.System.Code,
						SystemId = s.SystemId ?? 0,
						SystemRegion = s.SystemFrameRate.RegionCode + " " + s.SystemFrameRate.FrameRate,
						Game = s.Game.GoodName,
						GameId = s.GameId ?? 0,
						Rom = s.Rom.Name,
						RomId = s.RomId ?? 0,
						Tier = s.IntendedTier.Name,
						Branch = s.Branch,
						EmulatorVersion = s.EmulatorVersion,
						MovieExtension = s.MovieExtension
					})
					.SingleOrDefaultAsync();

				if (model != null)
				{
					model.AvailableMoviesToObsolete = await GetAvailableMoviesToObsolete(model.SystemId);
				}

				return model;
			}
		}

		/// <summary>
		/// Returns a list of published, non-obsolete movies for the given system
		/// Intended to be used for the publication form
		/// </summary>
		public async Task<IEnumerable<SelectListItem>> GetAvailableMoviesToObsolete(int systemId)
		{
			return await _db.Publications
				.ThatAreCurrent()
				.Where(p => p.SystemId == systemId)
				.Select(p => new SelectListItem
				{
					Value = p.Id.ToString(),
					Text = p.Title
				})
				.ToListAsync();
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
					.Select(s => new SubmissionCatalogModel
					{
						Id = s.Id,
						RomId = s.RomId,
						GameId = s.GameId,
						SystemId = s.SystemId,
						SystemFrameRateId = s.SystemFrameRateId,
					})
					.SingleAsync(s => s.Id == id);

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
		public async Task UpdateCatalog(SubmissionCatalogModel model)
		{
			var submission = await _db.Submissions.SingleAsync(s => s.Id == model.Id);
			_mapper.Map(model, submission);
			await _db.SaveChangesAsync();
		}

		/// <summary>
		/// Takes a <see cref="Submission"/> and creates a new <see cref="Publication"/>
		/// If successful, the id of the new publication is returned
		/// </summary>
		/// <returns>The id of the new <see cref="Publication"/></returns>
		public async Task<int> PublishSubmission(SubmissionPublishModel model)
		{
			var submission = await _db.Submissions
				.Include(s => s.IntendedTier)
				.Include(s => s.System)
				.Include(s => s.SystemFrameRate)
				.Include(s => s.Game)
				.Include(s => s.Rom)
				.Include(s => s.SubmissionAuthors)
				.ThenInclude(sa => sa.Author)
				.SingleAsync(s => s.Id == model.Id);

			var publication = new Publication
			{
				TierId = submission.IntendedTier.Id,
				SystemId = submission.System.Id,
				SystemFrameRateId = submission.SystemFrameRate.Id,
				GameId = submission.Game.Id,
				RomId = submission.Rom.Id,
				Branch = submission.Branch,
				EmulatorVersion = model.EmulatorVersion,
				OnlineWatchingUrl = model.OnlineWatchingUrl,
				MirrorSiteUrl = model.MirrorSiteUrl,
				Frames = submission.Frames,
				RerecordCount = submission.RerecordCount,
				MovieFileName = model.MovieFileName + "." + model.MovieExtension
			};

			// Unzip the submission file, and rezip it while renaming the contained file
			using (var publicationFileStream = new MemoryStream())
			{
				using (var publicationZipArchive = new ZipArchive(publicationFileStream, ZipArchiveMode.Create))
				using (var submissionFileStream = new MemoryStream(submission.MovieFile))
				using (var submissionZipArchive = new ZipArchive(submissionFileStream, ZipArchiveMode.Read))
				{
					var publicationZipEntry = publicationZipArchive.CreateEntry(model.MovieFileName + "." + model.MovieExtension);
					var submissionZipEntry = submissionZipArchive.Entries.Single();

					using (var publicationZipEntryStream = publicationZipEntry.Open())
					using (var submissionZipEntryStream = submissionZipEntry.Open())
					{
						await submissionZipEntryStream.CopyToAsync(publicationZipEntryStream);
					}
				}

				publication.MovieFile = publicationFileStream.ToArray();
			}

			var publicationAuthors = submission.SubmissionAuthors
				.Select(sa => new PublicationAuthor
				{
					Publication = publication,
					Author = sa.Author
				});

			foreach (var author in publicationAuthors)
			{
				publication.Authors.Add(author);
			}

			publication.Submission = submission;
			_db.Publications.Add(publication);

			await _db.SaveChangesAsync(); // Need an Id for the Title
			publication.GenerateTitle();

			byte[] screenshotBytes;
			using (var memoryStream = new MemoryStream())
			{
				await model.Screenshot.CopyToAsync(memoryStream);
				screenshotBytes = memoryStream.ToArray();
			}

			string screenshotFileName = $"{publication.Id}M{Path.GetExtension(model.Screenshot.FileName)}";
			string screenshotPath = Path.Combine(_hostingEnvironment.WebRootPath, "media", screenshotFileName);
			File.WriteAllBytes(screenshotPath, screenshotBytes);

			var screenshot = new PublicationFile
			{
				Publication = publication,
				Path = screenshotFileName,
				Type = FileType.Screenshot
			};
			_db.PublicationFiles.Add(screenshot);
			publication.Files.Add(screenshot);

			byte[] torrentBytes;
			using (var memoryStream = new MemoryStream())
			{
				await model.TorrentFile.CopyToAsync(memoryStream);
				torrentBytes = memoryStream.ToArray();
			}

			string torrentFileName = $"{publication.Id}M.torrent";
			string torrentPath = Path.Combine(_hostingEnvironment.WebRootPath, "media", torrentFileName);
			File.WriteAllBytes(torrentPath, torrentBytes);

			var torrent = new PublicationFile
			{
				Publication = publication,
				Path = torrentFileName,
				Type = FileType.Torrent
			};
			_db.PublicationFiles.Add(torrent);
			publication.Files.Add(torrent);

			// Create a wiki page corresponding to this submission
			var wikiPage = new WikiPage
			{
				RevisionMessage = $"Auto-generated from Movie #{publication.Id}",
				PageName = LinkConstants.PublicationWikiPage + publication.Id,
				MinorEdit = false,
				Markup = model.MovieMarkup
			};

			_db.WikiPages.Add(wikiPage);
			publication.WikiContent = wikiPage;

			submission.Status = SubmissionStatus.Published;
			var history = new SubmissionStatusHistory
			{
				SubmissionId = submission.Id,
				Status = SubmissionStatus.Published
			};
			submission.History.Add(history);
			_db.SubmissionStatusHistory.Add(history);

			if (model.MovieToObsolete.HasValue)
			{
				var toObsolete = await _db.Publications.SingleAsync(p => p.Id == model.MovieToObsolete);
				toObsolete.ObsoletedById = publication.Id;
			}

			await _db.SaveChangesAsync();

			return publication.Id;
		}
	}
}
