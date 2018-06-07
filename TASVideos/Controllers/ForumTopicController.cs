using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Filter;
using TASVideos.Models;
using TASVideos.Tasks;


namespace TASVideos.Controllers
{
	public class ForumTopicController : BaseController
	{
		private readonly ForumTasks _forumTasks;
		private readonly UserManager<User> _userManager;

		public ForumTopicController(
			ForumTasks forumTasks,
			UserTasks userTasks,
			UserManager<User> userManager)
			: base(userTasks)
		{
			_forumTasks = forumTasks;
			_userManager = userManager;
		}

		
		
	}
}
