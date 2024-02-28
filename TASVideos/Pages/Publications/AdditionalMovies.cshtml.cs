﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Core.Services;
using TASVideos.Core.Services.ExternalMediaPublisher;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.MovieParsers;

namespace TASVideos.Pages.Publications;

[RequirePermission(PermissionTo.CreateAdditionalMovieFiles)]
public class AdditionalMoviesModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly ExternalMediaPublisher _publisher;
	private readonly IPublicationMaintenanceLogger _publicationMaintenanceLogger;
	private readonly IMovieParser _parser;

	public AdditionalMoviesModel(
		ApplicationDbContext db,
		ExternalMediaPublisher publisher,
		IPublicationMaintenanceLogger publicationMaintenanceLogger,
		IMovieParser parser)
	{
		_db = db;
		_publisher = publisher;
		_publicationMaintenanceLogger = publicationMaintenanceLogger;
		_parser = parser;
	}

	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public string PublicationTitle { get; set; } = "";

	public IReadOnlyCollection<PublicationFileModel> AvailableMovieFiles { get; set; } = new List<PublicationFileModel>();

	[BindProperty]
	[StringLength(50)]
	[Display(Name = "Display Name")]
	public string DisplayName { get; set; } = "";

	[Required]
	[BindProperty]
	[Display(Name = "Add an additional movie file:", Description = "Your movie packed in a ZIP file (max size: 150k)")]
	public IFormFile? AdditionalMovieFile { get; set; }

	public async Task<IActionResult> OnGet()
	{
		var publicationTitle = await _db.Publications
			.Where(p => p.Id == Id)
			.Select(p => p.Title)
			.SingleOrDefaultAsync();

		if (publicationTitle is null)
		{
			return NotFound();
		}

		PublicationTitle = publicationTitle;
		await PopulateAvailableMovieFiles();
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		var publication = await _db.Publications
			.Where(p => p.Id == Id)
			.Select(p => new { p.Id, p.Title })
			.SingleOrDefaultAsync();

		if (publication is null)
		{
			return NotFound();
		}

		if (!AdditionalMovieFile.IsZip())
		{
			ModelState.AddModelError(nameof(AdditionalMovieFile), "Not a valid .zip file");
		}

		if (!AdditionalMovieFile.LessThanMovieSizeLimit())
		{
			ModelState.AddModelError(
				nameof(AdditionalMovieFile),
				".zip is too big, are you sure this is a valid movie file?");
		}

		if (!ModelState.IsValid)
		{
			PublicationTitle = publication.Title;
			await PopulateAvailableMovieFiles();
			return Page();
		}

		var parseResult = await _parser.ParseZip(AdditionalMovieFile!.OpenReadStream());
		if (!parseResult.Success)
		{
			ModelState.AddParseErrors(parseResult);
			PublicationTitle = publication.Title;
			await PopulateAvailableMovieFiles();
			return Page();
		}

		var publicationFile = new PublicationFile
		{
			Path = AdditionalMovieFile!.FileName,
			PublicationId = Id,
			Description = DisplayName,
			Type = FileType.MovieFile,
			FileData = await AdditionalMovieFile.ToBytes()
		};

		_db.PublicationFiles.Add(publicationFile);

		string log = $"Added new movie file: {DisplayName}";
		await _publicationMaintenanceLogger.Log(Id, User.GetUserId(), log);
		var result = await ConcurrentSave(_db, log, "Unable to add file");
		if (result)
		{
			await _publisher.SendPublicationEdit(
				$"{Id}M edited by {User.Name()}",
				$"[{Id}M]({{0}}) edited by {User.Name()}",
				$"{log} | {PublicationTitle}",
				$"{Id}M");
		}

		return RedirectToPage("AdditionalMovies", new { Id });
	}

	public async Task<IActionResult> OnPostDelete(int publicationFileId)
	{
		var file = await _db.PublicationFiles
			.SingleOrDefaultAsync(pf => pf.Id == publicationFileId);

		if (file != null)
		{
			_db.PublicationFiles.Remove(file);

			string log = $"Removed movie file {file.Path}";
			await _publicationMaintenanceLogger.Log(file.PublicationId, User.GetUserId(), log);
			var result = await ConcurrentSave(_db, log, "Unable to delete file");

			if (result)
			{
				await _publisher.SendPublicationEdit(
					$"{Id}M edited by {User.Name()}",
					$"[{Id}M]({{0}}) edited by {User.Name()}",
					$"{log}",
					$"{Id}M");
			}
		}

		return RedirectToPage("AdditionalMovies", new { Id });
	}

	private async Task PopulateAvailableMovieFiles()
	{
		AvailableMovieFiles = await _db.PublicationFiles
			.ThatAreMovieFiles()
			.ForPublication(Id)
			.Select(pf => new PublicationFileModel
			{
				Id = pf.Id,
				Description = pf.Description,
				FileName = pf.Path
			})
			.ToListAsync();
	}

	public class PublicationFileModel
	{
		public int Id { get; set; }
		public string? Description { get; set; }
		public string FileName { get; set; } = "";
	}
}
