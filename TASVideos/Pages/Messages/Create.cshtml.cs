using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Messages
{
	[Authorize]
	public class CreateModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<User> _userManager;
		private readonly PrivateMessageTasks _pmTasks;

		public CreateModel(
			ApplicationDbContext db,
			UserManager<User> userManager,
			PrivateMessageTasks privateMessageTasks,
			UserTasks userTasks)
			: base(userTasks)
		{
			_db = db;
			_userManager = userManager;
			_pmTasks = privateMessageTasks;
		}

		[FromQuery]
		public int? ReplyTo { get; set; }

		[FromQuery]
		public string DefaultToUser { get; set; } = "";

		[BindProperty]
		[Required]
		[Display(Name = "Subject")]
		[StringLength(100, MinimumLength = 3)]
		public string Subject { get; set; }

		[BindProperty]
		[Required]
		[Display(Name = "Message Body")]
		[StringLength(1000, MinimumLength = 5)]
		public string Text { get; set; }

		[BindProperty]
		[Required]
		[Display(Name = "Username", Description = "Enter a UserName")]
		public string ToUser { get; set; }

		public PrivateMessageModel ReplyingTo { get; set; }

		public bool IsReply => ReplyingTo != null;

		public async Task OnGet()
		{
			await SetReplyingTo();

			ToUser = DefaultToUser;
			if (IsReply)
			{
				Subject = "Re: " + ReplyingTo.Subject;
				ToUser = DefaultToUser;
			}
		}

		public async Task<IActionResult> OnPost()
		{
			if (User.Identity.Name == ToUser)
			{
				ModelState.AddModelError(nameof(ToUser), "Can not send a message to yourself!");
			}

			if (!ModelState.IsValid)
			{
				await SetReplyingTo();
				return Page();
			}

			var exists = await _db.Users.Exists(ToUser);
			if (!exists)
			{
				ModelState.AddModelError(nameof(ToUser), "User does not exist");
				await SetReplyingTo();
				return Page();
			}

			var user = await _userManager.GetUserAsync(User);
			await _pmTasks.SendMessage(user, this, IpAddress.ToString());

			return RedirectToPage("Inbox");
		}

		private async Task SetReplyingTo()
		{
			if (ReplyTo > 0)
			{
				var user = await _userManager.GetUserAsync(User);
				var message = await _pmTasks.GetMessage(user, ReplyTo.Value);
				if (message != null)
				{
					DefaultToUser = message.FromUserName;
					ReplyingTo = new PrivateMessageModel
					{
						Subject = message.Subject,
						Text = message.Text,
						SentOn = message.SentOn,
						FromUserName = message.FromUserName,
						FromUserId = message.FromUserId,
						ToUserName = message.ToUserName,
						ToUserId = message.ToUserId
					};
				}
			}
		}
	}
}
