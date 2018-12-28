using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Tasks;

namespace TASVideos.Pages.Account
{
	public class LogoutModel : BasePageModel
	{
		private readonly SignInManager<User> _signInManager;

		public LogoutModel(
			SignInManager<User> signInManager,
			UserTasks userTasks)
			: base(userTasks)
		{
			_signInManager = signInManager;
		}

		public async Task<IActionResult> OnPost()
		{
			await _signInManager.SignOutAsync();
			return Login();
		}
	}
}
