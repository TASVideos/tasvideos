using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TASVideos.Data.Entity;

namespace TASVideos.Extensions
{
    public static class RazorHelpers
    {
		public static bool UserHasPermission(this ViewDataDictionary viewData, PermissionTo permission)
		{
			return ((IEnumerable<PermissionTo>)viewData["UserPermissions"]).Contains(permission);
		}

		public static bool UserHasAnyPermission(this ViewDataDictionary viewData, IEnumerable<PermissionTo> permissions)
		{
			return permissions.Any(permission => ((IEnumerable<PermissionTo>)viewData["UserPermissions"]).Contains(permission));
		}
	}
}
