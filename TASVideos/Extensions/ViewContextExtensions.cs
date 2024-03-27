using Microsoft.AspNetCore.Mvc.Rendering;

namespace TASVideos.Extensions;

public static class ViewContextExtensions
{
	public static string Page(this ViewContext viewContext)
	{
		return viewContext.ActionDescriptor.DisplayName ?? "";
	}

	public static string PageGroup(this ViewContext viewContext)
	{
		return viewContext.ActionDescriptor.DisplayName
			?.SplitWithEmpty("/")
			.FirstOrDefault() ?? "";
	}

	public static bool WikiCondition(this ViewContext viewContext, string condition)
	{
		var viewData = viewContext.ViewData;
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
					result ^= viewData.UserHas(permission);
				}

				break;

			case "CanSubmitMovies": // Legacy system: same as UserIsLoggedIn
			case "CanRateMovies": // Legacy system: same as UserIsLoggedIn
			case "UserIsLoggedIn":
				result ^= viewContext.HttpContext.User.IsLoggedIn();
				break;
			case "1":
				result ^= true;
				break;
			case "0":
				result ^= false;
				break;

			// Support legacy values, these are deprecated
			case "CanEditPages":
				result ^= viewData.UserHas(PermissionTo.EditWikiPages);
				break;
			case "UserHasHomepage":
				result ^= viewContext.HttpContext.User.IsLoggedIn(); // Let's assume every user can have a homepage automatically
				break;
			case "CanViewSubmissions":
				result ^= true; // Legacy system always returned true
				break;
			case "CanJudgeMovies":
				result ^= viewData.UserHas(PermissionTo.JudgeSubmissions);
				break;
			case "CanPublishMovies":
				result ^= viewData.UserHas(PermissionTo.PublishMovies);
				break;
		}

		return result;
	}
}
