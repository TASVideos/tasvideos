namespace TASVideos.Pages.Users;

[RequirePermission(matchAny: false, PermissionTo.SeeEmails, PermissionTo.EditUsers)]
public class EditEmailModel(
	ApplicationDbContext db,
	IExternalMediaPublisher publisher,
	IUserMaintenanceLogger userMaintenanceLogger,
	ISignInManager signInManager,
	IUserManager userManager)
	: BasePageModel
{
	[FromRoute]
	public int Id { get; set; }

	[BindProperty]
	public UserEmailEditModel UserToEdit { get; set; } = new();

	public async Task<IActionResult> OnGet()
	{
		if (User.GetUserId() == Id)
		{
			return RedirectToPage("/Profile/Settings");
		}

		var userToEdit = await db.Users
			.Where(u => u.Id == Id)
			.Select(u => new UserEmailEditModel
			{
				UserName = u.UserName,
				Email = u.Email,
				EmailConfirmed = u.EmailConfirmed
			})
			.SingleOrDefaultAsync();

		if (userToEdit is null)
		{
			return NotFound();
		}

		UserToEdit = userToEdit;
		return Page();
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		var user = await db.Users.FindAsync(Id);
		if (user is null)
		{
			return NotFound();
		}

		if (!string.Equals(UserToEdit.Email, user.Email, StringComparison.InvariantCultureIgnoreCase)
			&& await signInManager.EmailExists(UserToEdit.Email))
		{
			ModelState.AddModelError($"{nameof(UserToEdit)}.{nameof(UserToEdit.Email)}", "Email already exists.");
			return Page();
		}

		user.Email = UserToEdit.Email;
		user.EmailConfirmed = UserToEdit.EmailConfirmed;
		user.NormalizedEmail = userManager.NormalizeEmail(user.Email) ?? "";

		var result = await db.TrySaveChanges();
		SetMessage(result, "", $"Unable to update user data for {user.UserName}");
		if (result.IsSuccess())
		{
			await publisher.SendUserManagement(
				$"User [{user.UserName}]({{0}}) email changed by {User.Name()}", user.UserName);
			var message = $"User {user.UserName} email changed by {User.Name()}";
			await userMaintenanceLogger.Log(user.Id, message, User.GetUserId());
			SuccessStatusMessage($"User {user.UserName} email changed by {User.Name()}");
		}

		// If username is changed, we want to ignore the returnUrl that will be the old name
		return BasePageRedirect("Edit", new { Id });
	}

	public class UserEmailEditModel
	{
		public string UserName { get; init; } = "";

		[Required]
		[EmailAddress]
		public string Email { get; init; } = "";
		public bool EmailConfirmed { get; init; }
	}
}
