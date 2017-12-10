using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data.Entity;

namespace TASVideos.Extensions
{
    public static class HtmlExtensions
    {
		public static bool WikiCondition<TModel>(this IHtmlHelper<TModel> html, string condition)
		{
			switch (condition)
			{
				default:
					if (Enum.TryParse(condition, out PermissionTo permission))
					{
						return html.ViewData.UserHasPermission(permission);
					}

					return false;

				case "CanSubmitMovies": // Legacy system: same as UserIsLoggedIn
				case "CanRateMovies": // Legacy system: same as UserIsLoggedIn
				case "UserIsLoggedIn":
					return html.ViewContext.HttpContext.User.Identity.IsAuthenticated;

				case "1":
					return true;
				case "0":
					return false;

				// Support legacy values, these are deprecated
				case "CanEditPages":
					return html.ViewData.UserHasPermission(PermissionTo.EditWikiPages);
				case "UserHasHomepage":
					return html.ViewContext.HttpContext.User.Identity.IsAuthenticated; // Let's assume every user can have a homepage automatically
				case "CanViewSubmissions":
					return true; // Legacy system always returned true
				case "CanJudgeMovies":
					return false; // TODO: need judge perm
				case "CanPublishMovies":
					return false; // TODO: need publish perm;
			}
		}
    }
}
