using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Services;
using TASVideos.Tasks;

namespace TASVideos.Controllers
{
	[Authorize]
	[Route("[controller]/[action]")]
	public class ProfileController : BaseController
	{
		private readonly UserManager<User> _userManager;
		private readonly SignInManager<User> _signInManager;
		private readonly IEmailSender _emailSender;
		private readonly ILogger _logger;
		private readonly AwardTasks _awardTasks;
		private readonly UserFileTasks _userFileTasks;

		public ProfileController(
			UserManager<User> userManager,
			SignInManager<User> signInManager,
			IEmailSender emailSender,
			ILogger<ProfileController> logger,
			UserTasks userTasks,
			AwardTasks awardTasks,
			UserFileTasks userFileTasks)
			: base(userTasks)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_emailSender = emailSender;
			_logger = logger;
			_awardTasks = awardTasks;
			_userFileTasks = userFileTasks;
		}

		[TempData]
		public string StatusMessage { get; set; }

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var userName = _userManager.GetUserName(User);

			var model = await UserTasks.GetUserProfile(userName);
			if (model == null)
			{
				return NotFound();
			}

			if (!string.IsNullOrWhiteSpace(model.Signature))
			{
				model.Signature = RenderPost(model.Signature, true, false);
			}

			model.Awards = await _awardTasks.GetAllAwardsForUser(model.Id);

			ViewData["ActivePage"] = nameof(Index);
			return View("~/Views/User/Profile.cshtml", model);
		}

		[HttpGet]
		public async Task<IActionResult> Settings()
		{
			var user = await _userManager.GetUserAsync(User);

			var model = new ProfileIndexViewModel
			{
				Username = user.UserName,
				Email = user.Email,
				TimeZoneId = user.TimeZoneId,
				IsEmailConfirmed = user.EmailConfirmed,
				PublicRatings = user.PublicRatings,
				StatusMessage = StatusMessage,
				From = user.From,
				Roles = await UserTasks.GetUserRoles(user.Id)
			};

			return View(model);
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Settings(ProfileIndexViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = await _userManager.GetUserAsync(User);

			var email = user.Email;
			if (model.Email != email)
			{
				var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);

				if (!setEmailResult.Succeeded)
				{
					throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
				}
			}

			if (model.TimeZoneId != user.TimeZoneId || model.PublicRatings != user.PublicRatings)
			{
				await UserTasks.UpdateUserProfile(user.Id, model.TimeZoneId, model.PublicRatings, model.From);
			}

			StatusMessage = "Your profile has been updated";
			return RedirectToAction(nameof(Index));
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> SendVerificationEmail(ProfileIndexViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View("Settings", model);
			}

			var user = await _userManager.GetUserAsync(User);

			var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			var callbackUrl = Url.EmailConfirmationLink(user.Id.ToString(), code, Request.Scheme);
			var email = user.Email;
			await _emailSender.SendEmailConfirmationAsync(email, callbackUrl);

			StatusMessage = "Verification email sent. Please check your email.";
			return RedirectToAction(nameof(Index));
		}

		[HttpGet]
		public async Task<IActionResult> ChangePassword()
		{
			var user = await _userManager.GetUserAsync(User);

			var hasPassword = await _userManager.HasPasswordAsync(user);
			if (!hasPassword)
			{
				return RedirectToAction(nameof(SetPassword));
			}

			var model = new ChangePasswordViewModel { StatusMessage = StatusMessage };
			return View(model);
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = await _userManager.GetUserAsync(User);

			var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
			if (!changePasswordResult.Succeeded)
			{
				AddErrors(changePasswordResult);
				return View(model);
			}

			await _signInManager.SignInAsync(user, isPersistent: false);
			_logger.LogInformation("User changed their password successfully.");
			StatusMessage = "Your password has been changed.";

			return RedirectToAction(nameof(ChangePassword));
		}

		[HttpGet]
		public async Task<IActionResult> SetPassword()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
			}

			var hasPassword = await _userManager.HasPasswordAsync(user);

			if (hasPassword)
			{
				return RedirectToAction(nameof(ChangePassword));
			}

			var model = new SetPasswordViewModel { StatusMessage = StatusMessage };
			return View(model);
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = await _userManager.GetUserAsync(User);

			var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
			if (!addPasswordResult.Succeeded)
			{
				AddErrors(addPasswordResult);
				return View(model);
			}

			await _signInManager.SignInAsync(user, isPersistent: false);
			StatusMessage = "Your password has been set.";

			return RedirectToAction(nameof(SetPassword));
		}

		public IActionResult HomePage()
		{
			return View();
		}

		public async Task<IActionResult> UserFiles()
		{
			var user = await _userManager.GetUserAsync(User);
			var model = await _userFileTasks.GetUserIndex(user.Id, includeHidden: true);
			return View(model);
		}
	}
}
