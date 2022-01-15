using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;

namespace TASVideos.Pages
{
	[AllowAnonymous]
	public class ErrorModel : PageModel
	{
		private readonly IHostEnvironment _env;

		public ErrorModel(IHostEnvironment env)
		{
			_env = env;
		}

		public string RequestId { get; set; } = "";
		public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

		public Exception? Exception { get; set; } 

		public void OnGet()
		{
			if (_env.IsDevelopment())
			{
				var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
				if (exceptionHandlerFeature != null)
				{
					Exception = exceptionHandlerFeature.Error;
				}
			}

			RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier;
		}
	}
}
