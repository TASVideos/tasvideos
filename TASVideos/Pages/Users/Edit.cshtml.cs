using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using TASVideos.Data.Entity;
using TASVideos.Extensions;
using TASVideos.Models;
using TASVideos.Tasks;

namespace TASVideos.Pages.Users
{
	[RequirePermission(PermissionTo.EditUsers)]
	public class EditModel : BasePageModel
	{
		public EditModel(UserTasks userTasks) : base(userTasks)
		{
		}

		[FromRoute]
		public int Id { get; set; }

		[BindProperty]
		public UserEditModel UserToEdit { get; set; } = new UserEditModel();

		public async Task<IActionResult> OnGet()
		{
			var userName = await UserTasks.GetUserNameById(Id);

			if (userName == null)
			{
				return NotFound();
			}

			UserToEdit = await UserTasks.GetUserForEdit(userName, User.GetUserId());

			return Page();
		}

		public async Task<IActionResult> OnPost()
		{
			if (!ModelState.IsValid)
			{	
				UserToEdit.AvailableRoles = await UserTasks
					.GetAllRolesUserCanAssign(User.GetUserId(), UserToEdit.SelectedRoles);
				return Page();
			}

			await UserTasks.EditUser(Id, UserToEdit);
			return RedirectToPage("List");
		}

		public async Task<IActionResult> OnGetUnlock(string returnUrl)
		{
			await UserTasks.UnlockUser(Id);
			return RedirectToLocal(returnUrl);
		}
	}
}
