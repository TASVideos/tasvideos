using Microsoft.AspNetCore.Mvc.Rendering;

namespace TASVideos.Pages.Messages;

[RequirePermission(PermissionTo.SendPrivateMessages)]
public class CreateModel(
	UserManager userManager,
	IPrivateMessageService privateMessageService)
	: BasePageModel
{
	[FromQuery]
	public int? ReplyTo { get; set; }

	[FromQuery]
	public string? DefaultToUser { get; set; }

	[BindProperty]
	[Display(Name = "Subject")]
	[StringLength(100, MinimumLength = 3)]
	public string Subject { get; set; } = "";

	[BindProperty]
	[Display(Name = "Message Body")]
	[StringLength(10000, MinimumLength = 5)]
	public string Text { get; set; } = "";

	[BindProperty]
	[Display(Name = "Username", Description = "Enter a UserName")]
	public string ToUser { get; set; } = "";

	public PrivateMessageModel? ReplyingTo { get; set; }

	public List<SelectListItem> AvailableGroupRoles { get; set; } = [];

	public bool IsReply => ReplyingTo is not null;

	public async Task OnGet()
	{
		await SetReplyingTo();
		await SetAvailableGroupRoles();
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
			await SetAvailableGroupRoles();
			return Page();
		}

		var allowedRoles = await privateMessageService.AllowedRoles();
		if (allowedRoles.Contains(ToUser))
		{
			await privateMessageService.SendMessageToRole(User.GetUserId(), ToUser, Subject, Text);
		}
		else
		{
			var exists = await userManager.Exists(ToUser);
			if (!exists)
			{
				ModelState.AddModelError(nameof(ToUser), "User does not exist");
				await SetReplyingTo();
				return Page();
			}

			await privateMessageService.SendMessage(User.GetUserId(), ToUser, Subject, Text);
		}

		return BasePageRedirect("Inbox");
	}

	private async Task SetAvailableGroupRoles()
	{
		AvailableGroupRoles =
		[
			.. UiDefaults.DefaultEntry,
			.. (await privateMessageService.AllowedRoles()).ToDropDown()
		];
	}

	private async Task SetReplyingTo()
	{
		if (ReplyTo > 0)
		{
			var message = await userManager.GetMessage(User.GetUserId(), ReplyTo.Value);
			if (message is not null)
			{
				DefaultToUser = message.FromUserName;
				ReplyingTo = new PrivateMessageModel(
					message.Subject,
					message.SentOn,
					message.Text,
					message.FromUserId,
					message.FromUserName,
					message.ToUserId,
					message.ToUserName);
			}
		}
	}

	public record PrivateMessageModel(string? Subject, DateTime SentOn, string Text, int FromUserId, string FromUserName, int ToUserId, string ToUserName);
}
