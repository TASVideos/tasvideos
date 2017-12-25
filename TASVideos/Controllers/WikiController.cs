using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Filter;
using TASVideos.Models;
using TASVideos.Tasks;
using TASVideos.WikiEngine;

namespace TASVideos.Controllers
{
	public class WikiController : BaseController
	{
		private readonly WikiTasks _wikiTasks;

		public WikiController(
			UserTasks userTasks,
			WikiTasks wikiTasks)
			: base(userTasks)
		{
			_wikiTasks = wikiTasks;
		}

		[AllowAnonymous]
		public async Task<IActionResult> RenderWikiPage(string url, int? revision = null)
		{
			url = url.Trim('/').Replace(".html", "");
			if (!WikiHelper.IsValidWikiPageName(url))
			{
				return RedirectToAction(nameof(PageNotFound), new { possibleUrl = WikiHelper.TryConvertToValidPageName(url) });
			}

			var existingPage = await _wikiTasks.GetPage(url, revision);

			if (existingPage != null)
			{
				ViewData["WikiPage"] = existingPage;
				ViewData["Title"] = existingPage.PageName;
				ViewData["Layout"] = "/Views/Shared/_WikiLayout.cshtml";
				return View(Razor.WikiMarkupFileProvider.Prefix + existingPage.Id, existingPage);
			}
			else if (revision.HasValue && await _wikiTasks.PageExists(url)) // Account for garbage revision values
			{
				return Redirect("/" + url);
			}

			return RedirectToAction(nameof(PageDoesNotExist), new { url });
		}

		[AllowAnonymous]
		public IActionResult PageNotFound(string possibleUrl)
		{
			ViewData["possibleUrl"] = possibleUrl;
			return View();
		}

		[AllowAnonymous]
		public IActionResult PageDoesNotExist(string url)
		{
			ViewData["Title"] = url?.Trim('/');
			return View();
		}

		[AllowAnonymous]
		public async Task<IActionResult> ViewSource(string path, int? revision = null)
		{
			var existingPage = await _wikiTasks.GetPage(path, revision);

			if (existingPage != null)
			{
				return View(existingPage);
			}

			return RedirectHome();
		}

		[AllowAnonymous]
		public async Task<IActionResult> PageHistory(string path)
		{
			var model = await _wikiTasks.GetPageHistory(path);
			return View(model);
		}

		[AllowAnonymous]
		public async Task<IActionResult> Diff(string path, int? fromRevision, int? toRevision)
		{
			path = path.Trim('/');

			var model = fromRevision.HasValue && toRevision.HasValue
				? await _wikiTasks.GetPageDiff(path, fromRevision.Value, toRevision.Value)
				: await _wikiTasks.GetLatestPageDiff(path);

			return View(model);
		}

		[AllowAnonymous]
		public async Task<IActionResult> DiffData(string path, int fromRevision, int toRevision)
		{
			var data = await _wikiTasks.GetPageDiff(path.Trim('/'), fromRevision, toRevision);
			return Json(data);
		}

		[AllowAnonymous]
		public async Task<IActionResult> Referrers(string path)
		{
			if (!string.IsNullOrWhiteSpace(path))
			{
				path = path.Trim('/');
				var referrers = await _wikiTasks.GetReferrers(path);
				ViewData["PageName"] = path;

				return View(referrers);
			}

			return RedirectHome();
		}

		[RequireEdit]
		public async Task<IActionResult> Edit(string path)
		{
			path = path?.Trim('/');
			if (!WikiHelper.IsValidWikiPageName(path))
			{
				return RedirectHome();
			}

			var existingPage = await _wikiTasks.GetPage(path);

			var model = new WikiEditModel
			{
				PageName = path,
				Markup = existingPage?.Markup ?? ""
			};

			return View(model);
		}

		[RequireEdit]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(WikiEditModel model)
		{
			if (ModelState.IsValid)
			{
				model.Referrals = Util.GetAllWikiLinks(model.Markup)
					.Select(l => new WikiReferralModel
					{
						Link = l,
						Excerpt = "TODO we need an except here"
					});

				await _wikiTasks.SavePage(model);
				return Redirect("/" + model.PageName.Trim('/'));
			}

			return View(model);
		}

		[AllowAnonymous]
		[HttpPost]
		public ContentResult GeneratePreview()
		{
			var input = new StreamReader(Request.Body, Encoding.UTF8).ReadToEnd();
			var w = new StringWriter();
			Util.DebugWriteHtml(input, w);
			return Content(w.ToString(), "text/plain");
		}

		[RequirePermission(PermissionTo.MoveWikiPages)]
		public async Task<IActionResult> MovePage(string path)
		{
			if (!string.IsNullOrWhiteSpace(path))
			{
				path = path.Trim('/');
				if (await _wikiTasks.PageExists(path))
				{
					return View(new WikiMoveModel
					{
						OriginalPageName = path,
						DestinationPageName = path
					});
				}
			}

			return RedirectHome();
		}

		[HttpPost]
		[RequirePermission(PermissionTo.MoveWikiPages)]
		public async Task<IActionResult> MovePage(WikiMoveModel model)
		{
			model.OriginalPageName = model.OriginalPageName.Trim('/');
			model.DestinationPageName = model.DestinationPageName.Trim('/');

			if (await _wikiTasks.PageExists(model.DestinationPageName, includeDeleted: true))
			{
				ModelState.AddModelError(nameof(WikiMoveModel.DestinationPageName), "The destination page already exists.");
			}

			if (ModelState.IsValid)
			{
				await _wikiTasks.MovePage(model);
				return Redirect("/" + model.DestinationPageName);
			}

			return View(model);
		}

		[RequirePermission(PermissionTo.DeleteWikiPages)]
		public async Task<IActionResult> DeletePage(string path)
		{
			if (!string.IsNullOrWhiteSpace(path))
			{
				await _wikiTasks.DeleteWikiPage(path.Trim('/'));
			}

			return RedirectToAction(nameof(WikiController.DeletedPages));
		}

		[RequirePermission(PermissionTo.DeleteWikiPages)]
		public async Task<IActionResult> DeleteRevision(string path, int revision)
		{
			if (string.IsNullOrWhiteSpace(path) || revision == 0)
			{
				return RedirectHome();
			}

			path = path.Trim('/');
			await _wikiTasks.DeleteWikiPageRevision(path, revision);

			return Redirect("/" + path);
		}

		[RequirePermission(PermissionTo.DeleteWikiPages)]
		public async Task<IActionResult> DeletedPages()
		{
			var model = await _wikiTasks.GetDeletedPages();
			return View(model);
		}

		[RequirePermission(PermissionTo.DeleteWikiPages)]
		public async Task<IActionResult> Undelete(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return RedirectHome();
			}

			path = path.Trim('/');
			await _wikiTasks.UndeletePage(path);
			return Redirect("/" + path);
		}

		[RequirePermission(PermissionTo.SeeAdminPages)]
		public async Task<IActionResult> SiteMap()
		{
			var model = CorePages();
			var wikiPages = await _wikiTasks.GetSubPages("");
			model.AddRange(wikiPages
				.Distinct()
				.Select(p => new SiteMapModel
				{
					PageName = p,
					IsWiki = true,
					AccessRestriction = "Anonymous"
				}));

			return View(model);
		}

		private static List<SiteMapModel> _corePages;
		private List<SiteMapModel> CorePages()
		{
			if (_corePages == null)
			{
				var asm = Assembly.GetAssembly(typeof(WikiController));
				_corePages = asm.GetTypes()
					.Where(type => typeof(Controller).IsAssignableFrom(type))
					.SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
					.Where(m => !m.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Any())
					.Where(m => m.GetCustomAttribute<HttpPostAttribute>() == null)
					.Select(m => new SiteMapModel
					{
						PageName = m.Name == "Index"
							? m.DeclaringType.Name.Replace("Controller", "")
							: $"{m.DeclaringType.Name.Replace("Controller", "")}/{m.Name}",
						IsWiki = false,
						AccessRestriction = AccessRestriction(m)
					})
					.ToList();
			}

			return _corePages;
		}

		private string AccessRestriction(MethodInfo action)
		{
			// This logic is far from robust and full of assumptions, the idea is to tweak as necessary
			if (action.GetCustomAttribute<AllowAnonymousAttribute>() != null
				|| action.DeclaringType.GetCustomAttribute<AllowAnonymousAttribute>() != null)
			{
				return "Anonymous";
			}

			if (action.GetCustomAttribute<AuthorizeAttribute>() != null
				|| action.DeclaringType.GetCustomAttribute< AuthorizeAttribute>() != null)
			{
				return "Logged In";
			}

			if (action.GetCustomAttribute<RequireEditAttribute>() != null
				|| action.DeclaringType.GetCustomAttribute<RequireEditAttribute>() != null)
			{
				return "Edit Permissions";
			}

			var requiredPermAttr = action.GetCustomAttribute<RequirePermissionAttribute>()
				?? action.DeclaringType.GetCustomAttribute<RequirePermissionAttribute>();
			if (requiredPermAttr != null)
			{
				return requiredPermAttr.MatchAny
					? string.Join(" or ", requiredPermAttr.RequiredPermissions)
					: string.Join(", ", requiredPermAttr.RequiredPermissions);
			}

			return "Unknown";
		}
	}
}
