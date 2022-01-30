using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using TASVideos.Data.Entity;

namespace TASVideos.Api.Filters;

/// <summary>
/// Checks if a user has the necessary permission in order to use the API endpoint,
/// If the user lacks permissions, a 401 is returned
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public class RequirePermissionAttribute : ActionFilterAttribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="RequirePermissionAttribute"/> class.
	/// </summary>
	public RequirePermissionAttribute(PermissionTo permission)
	{
		RequiredPermissions = new HashSet<PermissionTo> { permission };
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="RequirePermissionAttribute"/> class.
	/// </summary>
	public RequirePermissionAttribute(bool matchAny, params PermissionTo[] requiredPermissions)
	{
		MatchAny = matchAny;
		RequiredPermissions = requiredPermissions.ToHashSet();
	}

	/// <summary>
	/// Gets a value indicating whether or not to allow the user as long as only one permission is available,
	/// else all declared permissions are required
	/// </summary>
	public bool MatchAny { get; }

	/// <summary>
	/// Gets the permissions the user is required to have.
	/// </summary>
	public HashSet<PermissionTo> RequiredPermissions { get; }

	/// <inheritdoc />
	public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var user = context.HttpContext.User;
		if (user.IsLoggedIn())
		{
			var userPerms = await GetUserPermissions(context);
			if ((MatchAny && RequiredPermissions.Any(r => userPerms.Contains(r)))
				|| RequiredPermissions.IsSubsetOf(userPerms))
			{
				await base.OnActionExecutionAsync(context, next);
			}
			else
			{
				Unauthorized(context);
			}
		}
		else
		{
			Unauthorized(context);
		}
	}

	private static void Unauthorized(ActionExecutingContext context) => context.Result = new UnauthorizedResult();

	private static async Task<IEnumerable<PermissionTo>> GetUserPermissions(ActionExecutingContext context)
	{
		// On Post calls, we are potentially changing data, which could be malicious
		// Let's take the database hit to get the most recent permissions rather than relying
		// on the user cookie, in case the user's permissions have recently changed, such as from being "banned"
		// We are assuming we don't have malicious GET calls, and that for GETs we can afford to wait f
		// for the cookie expiration
		if (context.HttpContext.Request.Method == "Post")
		{
			var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager>();
			return await userManager.GetUserPermissionsById(context.HttpContext.User.GetUserId());
		}

		return context.HttpContext.User.Permissions();
	}
}
