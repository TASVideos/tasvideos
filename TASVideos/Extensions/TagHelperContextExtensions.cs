using Microsoft.AspNetCore.Razor.TagHelpers;
using TASVideos.TagHelpers;

namespace TASVideos.Extensions;

public static class TagHelperContextExtensions
{
	extension(TagHelperContext context)
	{
		private string GetConditionKey() => $"{nameof(ConditionTagHelper.Condition)}-{context.UniqueId}";
		private string GetPermissionKey() => $"{nameof(PermissionTagHelper.Permission)}-{context.UniqueId}";

		public void SetConditionResult(bool condition)
			=> context.Items[GetConditionKey(context)] = condition;

		public void SetPermissionResult(bool hasPermission)
			=> context.Items[GetPermissionKey(context)] = hasPermission;

		public bool IsSuppressedDueToConditionOrPermission()
		{
			if (context.Items.TryGetValue(GetConditionKey(context), out var conditionObj)
				&& conditionObj is bool condition
				&& !condition)
			{
				return true;
			}

			if (context.Items.TryGetValue(GetPermissionKey(context), out var permissionObj)
				&& permissionObj is bool hasPermission
				&& !hasPermission)
			{
				return true;
			}

			return false;
		}
	}
}
