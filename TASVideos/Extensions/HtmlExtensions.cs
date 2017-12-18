using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using TASVideos.Data.Entity;

namespace TASVideos.Extensions
{
	public static class HtmlExtensions
	{
		public static bool WikiCondition(this IHtmlHelper html, string condition)
		{
			bool result = false;

			if (condition.StartsWith('!'))
			{
				result = true;
				condition = condition.TrimStart('!');
			}

			switch (condition)
			{
				default:
					if (Enum.TryParse(condition, out PermissionTo permission))
					{
						result ^= html.ViewData.UserHasPermission(permission);
					}

					break;

				case "CanSubmitMovies": // Legacy system: same as UserIsLoggedIn
				case "CanRateMovies": // Legacy system: same as UserIsLoggedIn
				case "UserIsLoggedIn":
					result ^= html.ViewContext.HttpContext.User.Identity.IsAuthenticated;
					break;
				case "1":
					result ^= true;
					break;
				case "0":
					result ^= false;
					break;

				// Support legacy values, these are deprecated
				case "CanEditPages":
					result ^= html.ViewData.UserHasPermission(PermissionTo.EditWikiPages);
					break;
				case "UserHasHomepage":
					result ^= html.ViewContext.HttpContext.User.Identity
						.IsAuthenticated; // Let's assume every user can have a homepage automatically
					break;
				case "CanViewSubmissions":
					result ^= true; // Legacy system always returned true
					break;
				case "CanJudgeMovies":
					result ^= html.ViewData.UserHasPermission(PermissionTo.JudgeSubmissions);
					break;
				case "CanPublishMovies":
					result ^= html.ViewData.UserHasPermission(PermissionTo.PublishMovies);
					break;
			}

			return result;
		}
	}
}
