namespace TASVideos.Pages.Messages;

[RequirePermission(PermissionTo.SendPrivateMessages)]
public class CreateModel(IUserManager userManager, IPrivateMessageService privateMessageService) : BasePageModel
{
	[FromQuery]
	public int? ReplyTo { get; set; }

	[FromQuery]
	public string? DefaultToUser { get; set; }

	[BindProperty]
	[StringLength(100, MinimumLength = 3)]
	public string Subject { get; set; } = "";

	[BindProperty]
	[StringLength(10000, MinimumLength = 5)]
	public string MessageBody { get; set; } = "";

	[BindProperty]
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
			await privateMessageService.SendMessageToRole(User.GetUserId(), ToUser, Subject, MessageBody);
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

			await privateMessageService.SendMessage(User.GetUserId(), ToUser, Subject, MessageBody);
		}

		return BasePageRedirect("Inbox");
	}

	private async Task SetAvailableGroupRoles()
	{
		AvailableGroupRoles = (await privateMessageService.AllowedRoles())
			.ToDropDown()
			.WithDefaultEntry();
	}

	private async Task SetReplyingTo()
	{
		if (ReplyTo > 0)
		{
			var message = await privateMessageService.GetMessage(User.GetUserId(), ReplyTo.Value);
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
