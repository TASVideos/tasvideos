namespace TASVideos.Extensions;

public static class ViewContextExtensions
{
	public static string Page(this ViewContext viewContext)
		=> viewContext.ActionDescriptor.DisplayName ?? "";

	public static string PageGroup(this ViewContext viewContext)
		=> viewContext.ActionDescriptor.DisplayName
			?.SplitWithEmpty("/")
			.FirstOrDefault() ?? "";

	public static bool WikiCondition(this ViewContext viewContext, string condition)
	{
		var user = viewContext.HttpContext.User;
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
					result ^= user.Has(permission);
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
				result ^= user.Has(PermissionTo.EditWikiPages);
				break;
			case "UserHasHomepage":
				result ^= user.IsLoggedIn(); // Let's assume every user can have a homepage automatically
				break;
			case "CanViewSubmissions":
				result ^= true; // Legacy system always returned true
				break;
			case "CanJudgeMovies":
				result ^= user.Has(PermissionTo.JudgeSubmissions);
				break;
			case "CanPublishMovies":
				result ^= user.Has(PermissionTo.PublishMovies);
				break;
		}

		return result;
	}
}
