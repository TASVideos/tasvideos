namespace TASVideos.Extensions;

public static class ViewContextExtensions
{
	extension(ViewContext viewContext)
	{
		public string Page() => viewContext.ActionDescriptor.DisplayName ?? "";

		public string PageGroup()
			=> viewContext.ActionDescriptor.DisplayName
				?.SplitWithEmpty("/")
				.FirstOrDefault() ?? "";

		public bool WikiCondition(string condition)
		{
			var user = viewContext.HttpContext.User;
			var result = false;

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
}
