using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Constants;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Services;

namespace TASVideos.Pages.Publications
{
	[RequirePermission(PermissionTo.EditPublicationMetaData)]
	public class EditModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly IWikiPages _wikiPages;

		public EditModel(
			ApplicationDbContext db,
			IWikiPages wikiPages,
			UserManager userManager)
			: base(userManager)
		{
			_db = db;
			_wikiPages = wikiPages;
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public PublicationEditModel Publication { get; set; } = new PublicationEditModel();

		[Display(Name = "Available Flags")]
		public IEnumerable<SelectListItem> AvailableFlags { get; set; } = new List<SelectListItem>();

		[Display(Name = "Available Tags")]
		public IEnumerable<SelectListItem> AvailableTags { get; set; } = new List<SelectListItem>();

		public IEnumerable<SelectListItem> AvailableMoviesForObsoletedBy { get; set; } = new List<SelectListItem>();

		public async Task<IActionResult> OnGet()
		{
			Publication = await _db.Publications
					.Where(p => p.Id == Id)
					.Select(p => new PublicationEditModel
					{
						Tier = p.Tier.Name,
						TierIconPath = p.Tier.IconPath,
						TierLink = p.Tier.Link,
						SystemCode = p.System.Code,
						Title = p.Title,
						ObsoletedBy = p.ObsoletedById,
						Branch = p.Branch,
						EmulatorVersion = p.EmulatorVersion,
						OnlineWatchingUrl = p.OnlineWatchingUrl,
						MirrorSiteUrl = p.MirrorSiteUrl,
						SelectedFlags = p.PublicationFlags
							.Select(pf => pf.FlagId)
							.ToList(),
						SelectedTags = p.PublicationTags
							.Select(pt => pt.TagId)
							.ToList(),
						Markup = p.WikiContent.Markup
					})
					.SingleOrDefaultAsync();

			if (Publication == null)
			{
				return NotFound();
			}

			AvailableMoviesForObsoletedBy =
					await GetAvailableMoviesForObsoletedBy(Publication.SystemCode);

			AvailableFlags = await GetAvailableFlags(UserPermissions);
			AvailableTags = await GetAvailableTags();

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{
				AvailableMoviesForObsoletedBy =
					await GetAvailableMoviesForObsoletedBy(Publication.SystemCode);
				AvailableFlags = await GetAvailableFlags(UserPermissions);
				AvailableTags = await GetAvailableTags();

				return Page();
			}

			await UpdatePublication(Id, Publication);
			return RedirectToPage("View", new { Id });
		}

		// TODO: document
		private async Task<IEnumerable<SelectListItem>> GetAvailableFlags(IEnumerable<PermissionTo> userPermissions)
		{
			return await _db.Flags
				.Select(f => new SelectListItem
				{
					Text = f.Name,
					Value = f.Id.ToString(),
					Disabled = f.PermissionRestriction.HasValue
						&& !userPermissions.Contains(f.PermissionRestriction.Value)
				})
				.ToListAsync();
		}

		private async Task<IEnumerable<SelectListItem>> GetAvailableTags()
		{
			return await _db.Tags
				.Select(f => new SelectListItem
				{
					Text = f.DisplayName,
					Value = f.Id.ToString(),
				})
				.ToListAsync();
		}

		private async Task<IEnumerable<SelectListItem>> GetAvailableMoviesForObsoletedBy(string systemCode)
		{
			return await _db.Publications
				.ThatAreCurrent()
				.Where(p => p.System.Code == systemCode)
				.Where(p => p.Id != Id)
				.Select(p => new SelectListItem
				{
					Text = p.Title,
					Value = p.Id.ToString()
				})
				.ToListAsync();
		}

		private async Task UpdatePublication(int id, PublicationEditModel model)
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

	}
}
