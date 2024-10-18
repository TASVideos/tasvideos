using System.Security.Claims;

namespace TASVideos;

public static class ClaimsPrincipalExtensions
{
	public static bool CanEditWiki(this ClaimsPrincipal user, string pageName)
		=> WikiHelper.UserCanEditWikiPage(pageName, user.Name(), user.Permissions(), out _);
}
