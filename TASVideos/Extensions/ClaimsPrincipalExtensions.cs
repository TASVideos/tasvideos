using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

using TASVideos.Data.Constants;
using TASVideos.Data.Entity;

namespace TASVideos
{
	public static class ClaimsPrincipalExtensions
	{
		public static int GetUserId(this ClaimsPrincipal user)
		{
			if (user == null || !user.Identity.IsAuthenticated)
			{
				return -1;
			}

			return int.Parse(user.Claims
				.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
		}

		public static IEnumerable<PermissionTo> Permissions(this ClaimsPrincipal user)
		{
			if (user == null || !user.Identity.IsAuthenticated)
			{
				return Enumerable.Empty<PermissionTo>();
			}

			return user.Claims
				.Where(c => c.Type == CustomClaimTypes.Permission)
				.Select(c => Enum.Parse<PermissionTo>(c.Value))
				.ToList();
		}

		public static bool Has(this ClaimsPrincipal user, PermissionTo permission)
		{
			return user.Permissions().Contains(permission);
		}
	}
}
