﻿using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Core.Services;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Pages.Messages;

[RequirePermission(PermissionTo.SendPrivateMessages)]
public class CreateModel : BasePageModel
{
	private readonly ApplicationDbContext _db;
	private readonly UserManager _userManager;

	public CreateModel(
		ApplicationDbContext db,
		UserManager userManager)
	{
		_db = db;
		_userManager = userManager;
	}

	[FromQuery]
	public int? ReplyTo { get; set; }

	[FromQuery]
	public string? DefaultToUser { get; set; }

	[BindProperty]
	[Required]
	[Display(Name = "Subject")]
	[StringLength(100, MinimumLength = 3)]
	public string Subject { get; set; } = "";

	[BindProperty]
	[Required]
	[Display(Name = "Message Body")]
	[StringLength(1000, MinimumLength = 5)]
	public string Text { get; set; } = "";

	[BindProperty]
	[Required]
	[Display(Name = "Username", Description = "Enter a UserName")]
	public string ToUser { get; set; } = "";

	public PrivateMessageModel? ReplyingTo { get; set; }

	public bool IsReply => ReplyingTo is not null;

	public async Task OnGet()
	{
		await SetReplyingTo();

		ToUser = DefaultToUser ?? "";
		if (IsReply)
		{
			Subject = "Re: " + ReplyingTo!.Subject;
			ToUser = DefaultToUser ?? "";
		}
	}

	public async Task<IActionResult> OnPost()
	{
		if (User.Name() == ToUser)
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

		await SendMessage();

		return BasePageRedirect("Inbox");
	}

	private async Task SetReplyingTo()
	{
		if (ReplyTo > 0)
		{
			var message = await _userManager.GetMessage(User.GetUserId(), ReplyTo.Value);
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

	private async Task SendMessage()
	{
		var toUserId = await _db.Users
			.Where(u => u.UserName == ToUser)
			.Select(u => u.Id)
			.SingleAsync();

		var message = new PrivateMessage
		{
			FromUserId = User.GetUserId(),
			ToUserId = toUserId,
			Subject = Subject,
			Text = Text,
			IpAddress = IpAddress,
			EnableBbCode = true
		};

		_db.PrivateMessages.Add(message);
		await _db.SaveChangesAsync();
	}
}
