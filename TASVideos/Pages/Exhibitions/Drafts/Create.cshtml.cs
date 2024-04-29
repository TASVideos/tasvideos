using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Core.Services.Wiki;
using TASVideos.Data.Entity.Exhibition;
using TASVideos.MovieParsers;
using TASVideos.Pages.Exhibitions.Models;

namespace TASVideos.Pages.Exhibitions.Drafts;

public class CreateModel(
	ApplicationDbContext db,
	IMediaFileUploader uploader,
	IMovieParser parser,
	IWikiPages wikiPages,
	ITASVideoAgent tasVideoAgent)
	: BasePageModel
{
	[BindProperty]
	public ExhibitionFormModel ExhibitionForm { get; set; } = new();

	public async Task OnGet()
	{
		await PopulateDropdowns();
	}

	public async Task<IActionResult> OnPost()
	{
		if (ExhibitionForm.Exhibition.Screenshot != null && !ExhibitionForm.Exhibition.Screenshot.IsValidImage())
		{
			ModelState.AddModelError($"{nameof(ExhibitionForm)}.{nameof(ExhibitionForm.Exhibition)}.{nameof(ExhibitionForm.Exhibition.Screenshot)}", "Invalid file type. Must be .png or .jpg");
		}

		if (!ModelState.IsValid)
		{
			await PopulateDropdowns();
			return Page();
		}

		foreach (var movie in ExhibitionForm.Exhibition.Movies)
		{
			if (movie.MovieFile != null)
			{
				var parseResult = await parser.ParseZip(movie.MovieFile.OpenReadStream());

				if (!parseResult.Success)
				{
					ModelState.AddParseErrors(parseResult);
					await PopulateDropdowns();
					return Page();
				}
			}
		}

		Exhibition exhibition = new()
		{
			Title = ExhibitionForm.Exhibition.Title,
			ExhibitionTimestamp = ExhibitionForm.Exhibition.ExhibitionTimestamp,
			Games = await db.Games.Where(g => ExhibitionForm.Exhibition.Games.Contains(g.Id)).ToListAsync(),
			Contributors = await db.Users.Where(g => ExhibitionForm.Exhibition.Contributors.Contains(g.Id)).ToListAsync()
		};

		foreach (var movie in ExhibitionForm.Exhibition.Movies)
		{
			if (movie.MovieFile != null)
			{
				byte[] movieFile = await movie.MovieFile.ToBytes();

				exhibition.Files.Add(new ExhibitionFile
				{
					Type = ExhibitionFileType.MovieFile,
					Description = movie.MovieFileDescription,
					FileData = movieFile,
				});
			}
		}

		foreach (var url in ExhibitionForm.Exhibition.Urls)
		{
			exhibition.Urls.Add(new ExhibitionUrl
			{
				Type = url.Type,
				DisplayName = url.DisplayName,
				Url = url.Url,
			});
		}

		await db.Exhibitions.AddAsync(exhibition);

		await db.SaveChangesAsync();

		if (ExhibitionForm.Exhibition.Screenshot != null)
		{
			await uploader.UploadExhibitionScreenshot(exhibition.Id, ExhibitionForm.Exhibition.Screenshot, ExhibitionForm.Exhibition.ScreenshotDescription);
		}

		var wikiPage = new WikiCreateRequest
		{
			RevisionMessage = $"Auto-generated from Exhibition #{exhibition.Id}",
			PageName = WikiHelper.ToExhibitionWikiPageName(exhibition.Id),
			MinorEdit = false,
			Markup = ExhibitionForm.Exhibition.Markup,
			AuthorId = User.GetUserId()
		};
		var addedWikiPage = await wikiPages.Add(wikiPage);

		string topicTitle = $"D{exhibition.Id}: {exhibition.Title}";
		exhibition.TopicId = await tasVideoAgent.PostExhibitionTopic(exhibition.Id, topicTitle);
		await db.SaveChangesAsync();

		return RedirectToPage("View", new { exhibition.Id });
	}

	private async Task PopulateDropdowns()
	{
		ExhibitionForm.AvailableGames = await db.Games
			.OrderBy(g => g.DisplayName)
			.Select(g => new SelectListItem
			{
				Text = g.DisplayName,
				Value = g.Id.ToString()
			})
			.ToListAsync();

		ExhibitionForm.AvailableUsers = await db.Users
			.OrderBy(u => u.UserName)
			.Select(u => new SelectListItem
			{
				Text = u.UserName,
				Value = u.Id.ToString()
			})
			.ToListAsync();
	}
}
