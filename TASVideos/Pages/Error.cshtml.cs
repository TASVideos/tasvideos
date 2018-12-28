using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using TASVideos.Tasks;

namespace TASVideos.Pages
{
	[AllowAnonymous]
	public class ErrorModel : BasePageModel
	{
		public ErrorModel(UserTasks userTasks)
			: base(userTasks)
		{
		}

		public string RequestId { get; set; }
		public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

		public void OnGet()
		{
			RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
		}
	}
}
