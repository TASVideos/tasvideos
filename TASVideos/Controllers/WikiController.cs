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
using TASVideos.Razor;
using TASVideos.Services;
using TASVideos.Services.ExternalMediaPublisher;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	public class WikiController : BaseController
	{
		private readonly WikiTasks _wikiTasks;
		private readonly IWikiPages _wikiPages;
		private readonly WikiMarkupFileProvider _wikiMarkupFileProvider;
		private readonly ExternalMediaPublisher _publisher;

		public WikiController(
			UserTasks userTasks,
			WikiTasks wikiTasks,
			IWikiPages wikiPages,
			WikiMarkupFileProvider wikiMarkupFileProvider,
			ExternalMediaPublisher publisher)
			: base(userTasks)
		{
			_wikiTasks = wikiTasks;
			_wikiPages = wikiPages;
			_wikiMarkupFileProvider = wikiMarkupFileProvider;
			_publisher = publisher;
		}

		[AllowAnonymous]
		public IActionResult RenderWikiPage(string url, int? revision = null)
		{
			url = url.Trim('/').Replace(".html", "");

			if (url.ToLower() == "frontpage")
			{
				return Redirect("/");
			}

			if (!WikiHelper.IsValidWikiPageName(url))
			{
				return RedirectToPage("/Wiki/PageNotFound", new { possibleUrl = WikiHelper.NormalizeWikiPageName(url) });
			}

			var existingPage = _wikiPages.Page(url, revision);

			if (existingPage != null)
			{
				ViewData["WikiPage"] = existingPage;
				ViewData["Title"] = existingPage.PageName;
				ViewData["Layout"] = "_WikiLayout";
				_wikiMarkupFileProvider.WikiPages = _wikiPages;
				return View(WikiMarkupFileProvider.Prefix + existingPage.Id, existingPage);
			}

			// Account for garbage revision values
			if (revision.HasValue && _wikiPages.Exists(url)) 
			{
				return Redirect("/" + url);
			}

			return RedirectToPage("/Wiki/PageDoesNotExist", new { url });
		}

		public async Task<IActionResult> UserEditHistory(string userName)
		{
			var model = await _wikiTasks.GetEditHistoryForUser(userName);
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
		public IActionResult Edit(string path)
		{
			path = path?.Trim('/');
			if (!WikiHelper.IsValidWikiPageName(path))
			{
				return RedirectHome();
			}

			var existingPage = _wikiPages.Page(path);

			var model = new WikiEditModel
			{
				PageName = path,
				Markup = existingPage?.Markup ?? ""
			};

			return View(model);
		}

		[RequireEdit]
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(WikiEditModel model)
		{
			if (ModelState.IsValid)
			{
				var page = new WikiPage
				{
					PageName = model.PageName,
					Markup = model.Markup,
					MinorEdit = model.MinorEdit,
					RevisionMessage = model.RevisionMessage
				};
				await _wikiPages.Add(page);

				if (page.Revision == 1 || !model.MinorEdit)
				{
					_publisher.SendGeneralWiki(
						$"Page {model.PageName} {(page.Revision > 1 ? "edited" : "created")} by {User.Identity.Name}",
						$"{model.RevisionMessage}",
						$"{BaseUrl}/{model.PageName}");
				}

				return Redirect("/" + model.PageName.Trim('/'));
			}

			return View(model);
		}

		[AllowAnonymous]
		[HttpPost]
		public ViewResult GeneratePreview()
		{
			var input = new StreamReader(Request.Body, Encoding.UTF8).ReadToEnd();

			ViewData["WikiPage"] = null;
			ViewData["Title"] = "Generated Preview";
			ViewData["Layout"] = null;
			var name = _wikiMarkupFileProvider.SetPreviewMarkup(input);

			return View(name);
		}

		[RequirePermission(PermissionTo.MoveWikiPages)]
		public IActionResult MovePage(string path)
		{
			if (!string.IsNullOrWhiteSpace(path))
			{
				path = path.Trim('/');
				if (_wikiPages.Exists(path))
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

			if (_wikiPages.Exists(model.DestinationPageName, includeDeleted: true))
			{
				ModelState.AddModelError(nameof(WikiMoveModel.DestinationPageName), "The destination page already exists.");
			}

			if (ModelState.IsValid)
			{
				await _wikiPages.Move(model.OriginalPageName, model.DestinationPageName);

				_publisher.SendGeneralWiki(
						$"Page {model.OriginalPageName} moved to {model.DestinationPageName} by {User.Identity.Name}",
						"",
						$"{BaseUrl}/{model.DestinationPageName}");

				return Redirect("/" + model.DestinationPageName);
			}

			return View(model);
		}

		[RequirePermission(PermissionTo.DeleteWikiPages)]
		public async Task<IActionResult> DeletePage(string path)
		{
			if (!string.IsNullOrWhiteSpace(path))
			{
				var result = await _wikiPages.Delete(path.Trim('/'));

				_publisher.SendGeneralWiki(
					$"Page {path} DELETED by {User.Identity.Name}",
					$"({result} revisions)",
					"");
			}

			return RedirectToAction(nameof(DeletedPages));
		}

		[RequirePermission(PermissionTo.DeleteWikiPages)]
		public async Task<IActionResult> DeleteRevision(string path, int revision)
		{
			if (string.IsNullOrWhiteSpace(path) || revision == 0)
			{
				return RedirectHome();
			}

			path = path.Trim('/');
			await _wikiPages.Delete(path, revision);

			_publisher.SendGeneralWiki(
					$"Revision {revision} of Page {path} DELETED by {User.Identity.Name}",
					"",
					"");

			return Redirect("/" + path);
		}

		[RequirePermission(PermissionTo.SeeDeletedWikiPages)]
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
			await _wikiPages.Undelete(path);

			_publisher.SendGeneralWiki(
					$"Page {path} UNDELETED by {User.Identity.Name}",
					"",
					$"{BaseUrl}/path");

			return Redirect("/" + path);
		}

		[RequirePermission(PermissionTo.SeeAdminPages)]
		public IActionResult SiteMap()
		{
			var model = CorePages();
			var wikiPages = _wikiTasks.GetSubPages("");
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
				|| action.DeclaringType.GetCustomAttribute<AuthorizeAttribute>() != null)
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
