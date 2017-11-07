using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using TASVideos.Controllers;

namespace TASVideos.Filter
{
	/// <summary>
	/// Adds user properties to ViewBag/ViewData that have general purposes usage throughout any view
	/// such as User information
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
	public class SetViewBagAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (filterContext.Controller is Controller controller)
			{
				if (filterContext.Controller is BaseController baseController)
				{
					controller.ViewBag.UserPermissions = baseController.UserPermissions;
					controller.ViewBag.Version = baseController.Version;
				}
			}

			base.OnActionExecuting(filterContext);
		}
	}
}
