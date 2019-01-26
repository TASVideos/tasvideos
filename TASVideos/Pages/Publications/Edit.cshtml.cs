using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Publications
{
	[RequirePermission(PermissionTo.EditPublicationMetaData)]
	public class EditModel : BasePageModel
	{
		private readonly PublicationTasks _publicationTasks;
		private readonly ApplicationDbContext _db;

		public EditModel(
			ApplicationDbContext db,
			PublicationTasks publicationTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_db = db;
			_publicationTasks = publicationTasks;
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

			await _publicationTasks.UpdatePublication(Id, Publication);
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
	}
}
