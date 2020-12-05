using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;
using TASVideos.Extensions;
using TASVideos.MovieParsers;
using TASVideos.Pages.UserFiles.Models;
using TASVideos.Services;

namespace TASVideos.Pages.UserFiles
{
	[RequirePermission(PermissionTo.UploadUserFiles)]
	public class UploadModel : BasePageModel
	{
		private static readonly string[] SupportedSupplementalTypes = { ".lua", ".wch", ".gst" };

		private readonly ApplicationDbContext _db;
		private readonly MovieParser _parser;
		private readonly IFileService _fileService;

		public UploadModel(
			ApplicationDbContext db,
			MovieParser parser,
			IFileService fileService)
		{
			_db = db;
			_parser = parser;
			_fileService = fileService;
		}

		[BindProperty]
		public UserFileUploadModel UserFile { get; set; } = new();

		public int StorageUsed { get; set; } 

		public IEnumerable<SelectListItem> AvailableSystems { get; set; } = new List<SelectListItem>();

		public IEnumerable<SelectListItem> AvailableGames { get; set; } = new List<SelectListItem>();

		public async Task OnGet()
		{
			await Initialize();
		}

		public async Task<IActionResult> OnPost()
		{
			await Initialize();

			if (!ModelState.IsValid)
			{
				return Page();
			}

			if (UserFile.File.IsCompressed())
			{
				ModelState.AddModelError(
					$"{nameof(UserFile)}.{nameof(UserFile.File)}",
					"Compressed files are not supported.");
				return Page();
			}

			var fileExt = Path.GetExtension(UserFile.File!.FileName);

			if (!SupportedSupplementalTypes.Contains(fileExt)
				&& !_parser.SupportedMovieExtensions.Contains(fileExt))
			{
				ModelState.AddModelError(
					$"{nameof(UserFile)}.{nameof(UserFile.File)}",
					$"Unsupported file type: {fileExt}");
				return Page();
			}

			// Note: we calculate storage used by compressed site, so technically we should compress then check
			// but if they are cutting it this close, it's probably going to be a problem anyway
			// and we don't want to waste time compressing the file if we do not need to
			if (StorageUsed + UserFile.File!.Length > SiteGlobalConstants.UserFileStorageLimit)
			{
				ModelState.AddModelError(
					$"{nameof(UserFile)}.{nameof(UserFile.File)}",
					"File exceeds your available storage space. Remove unecessary files and try again.");
				return Page();
			}

			var userFile = new UserFile
			{
				Id = DateTime.UtcNow.Ticks,
				Title = UserFile.Title,
				Description = UserFile.Description,
				SystemId = UserFile.SystemId,
				GameId = UserFile.GameId,
				AuthorId = User.GetUserId(),
				LogicalLength = (int)UserFile.File.Length,
				UploadTimestamp = DateTime.UtcNow,
				Class = SupportedSupplementalTypes.Contains(fileExt)
					? UserFileClass.Support
					: UserFileClass.Movie,
				Type = fileExt.Replace(".", ""),
				
				FileName = UserFile.File.FileName
			};

			if (_parser.SupportedMovieExtensions.Contains(fileExt))
			{
				var parseResult = _parser.ParseFile(UserFile.File.FileName, UserFile.File.OpenReadStream());
				if (!parseResult.Success)
				{
					ModelState.AddParseErrors(parseResult, $"{nameof(UserFile)}.{nameof(UserFile.File)}");
					await Initialize();
					return Page();
				}

				userFile.Rerecords = parseResult.RerecordCount;
				userFile.Frames = parseResult.Frames;

				decimal frameRate = 60.0M;
				if (parseResult.FrameRateOverride.HasValue)
				{
					frameRate = (decimal)parseResult.FrameRateOverride.Value;
				}
				else
				{
					var system = await _db.GameSystems.SingleOrDefaultAsync(s => s.Code == parseResult.SystemCode);
					var frameRateData = await _db.GameSystemFrameRates
						.ForSystem(system.Id)
						.ForRegion(parseResult.Region.ToString())
						.FirstOrDefaultAsync();

					if (frameRateData != null)
					{
						frameRate = (decimal)frameRateData.FrameRate;
					}
				}
				
				userFile.Length = Math.Round(userFile.Frames / frameRate);
			}

			var fileBytes = await FormFileToBytes(UserFile.File);
			var fileResult = await _fileService.Compress(fileBytes);

			userFile.PhysicalLength = fileResult.CompressedSize;
			userFile.CompressionType = fileResult.Type;
			userFile.Content = fileResult.Data;

			_db.UserFiles.Add(userFile);
			await _db.SaveChangesAsync();

			return RedirectToPage("/Profile/UserFiles");
		}

		private async Task Initialize()
		{
			var userId = User.GetUserId();
			StorageUsed = await _db.UserFiles
				.Where(uf => uf.AuthorId == userId)
				.SumAsync(uf => uf.LogicalLength);

			AvailableSystems = UiDefaults.DefaultEntry.Concat(await _db.GameSystems
				.Select(s => new SelectListItem
				{
					Value = s.Id.ToString(),
					Text = s.Code
				})
				.ToListAsync());

			AvailableGames = UiDefaults.DefaultEntry.Concat(await _db.Games
				.OrderBy(g => g.SystemId)
				.ThenBy(g => g.DisplayName)
				.ToDropDown()
				.ToListAsync());
		}
	}
}
