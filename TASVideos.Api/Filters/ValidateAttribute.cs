using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TASVideos.Api.Requests;
using TASVideos.Data;

#pragma warning disable 1591
namespace TASVideos.Api
{
	public class ValidateAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			// Validate the sort parameter is valid based on the response type
			// ProducesResponseType must be present, and response type must be an enumerable
			if (context.Filters.FirstOrDefault(f => f is ProducesResponseTypeAttribute) is ProducesResponseTypeAttribute responseTypeFilter)
			{
				var responseType = responseTypeFilter.Type.GetGenericArguments().FirstOrDefault();
				if (responseType is not null)
				{
					var apiRequest = context.ActionArguments.Values.FirstOrDefault(v => v is ApiRequest);

					if (apiRequest != null)
					{
						var sort = (apiRequest as ApiRequest)?.Sort;
						if (!string.IsNullOrWhiteSpace(sort))
						{
							if (!((ApiRequest)apiRequest).IsValidSort(responseType))
							{
								context.ModelState.AddModelError(nameof(ApiRequest.Sort), $"Invalid Sort parameter: {sort}");
							}
						}
					}
				}
			}

			if (!context.ModelState.IsValid)
			{
				context.Result = new BadRequestObjectResult(context.ModelState);
			}

			base.OnActionExecuting(context);
		}
	}
}
