using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Models;

namespace TASVideos.Tasks
{
	public class UserTasks
	{
		private readonly ApplicationDbContext _db;

		public UserTasks(ApplicationDbContext db)
		{
			_db = db;
		}

		/// <summary>
		/// Returns a list of all permissions of the <seea cref="User"/> with the given id
		/// </summary>
		public IEnumerable<PermissionTo> GetUserPermissionsById(int userId)
		{
			return GetUserPermissionByIdQuery(userId)
				.ToList();
		}

		/// <summary>
		/// Returns a list of all permissions of the <seea cref="User"/> with the given id
		/// </summary>
		public async Task<IEnumerable<PermissionTo>> GetUserPermissionsByIdAsync(int userId)
		{
			return await GetUserPermissionByIdQuery(userId)
				.ToListAsync();
		}

		private IQueryable<PermissionTo> GetUserPermissionByIdQuery(int userId)
		{
			return _db.Users
				.Include(u => u.UserRoles)
				.ThenInclude(u => u.Role)
				.ThenInclude(r => r.RolePermission)
				.ThenInclude(rp => rp.Permission)
				.Where(u => u.Id == userId)
				.SelectMany(u => u.UserRoles)
				.SelectMany(ur => ur.Role.RolePermission)
				.Select(rp => rp.PermissionId)
				.Distinct();
		}

		public async Task<IEnumerable<RoleBasicDisplay>> GetUserRoles(int userId)
		{
			return await _db.Users
				.Include(u => u.UserRoles)
				.ThenInclude(ur => ur.Role)
				.Where(u => u.Id == userId)
				.SelectMany(u => u.UserRoles)
				.Select(ur => ur.Role)
				.Select(r => new RoleBasicDisplay
				{
					Id = r.Id,
					Name = r.Name,
					Description = r.Description
				})
				.ToListAsync();
		}

		// TODO: document
		public PageOf<UserListViewModel> GetPageOfUsers(PagedModel paging)
		{
			var data = _db.Users
				.Include(u => u.UserRoles)
				.ThenInclude(ur => ur.Role)
				.Select(u => new UserListViewModel
				{
					Id = u.Id,
					UserName = u.UserName,
					Roles = u.UserRoles.Select(ur => ur.Role.Name)
				})
				.OrderBy(u => u.Id) // TODO: soring
				.Paginate(_db, paging.CurrentPage, paging.PageSize, out int rowCount);

			var paged = new PageOf<UserListViewModel>(data)
			{
				PageSize = paging.PageSize,
				CurrentPage = paging.CurrentPage,
				RowCount = rowCount
			};

			return paged;
		}

		// TODO: document, for the read-only user view page
		public async Task<UserDetailsViewModel> GetUserDetails(int id)
		{
			return await _db.Users
				.Select(u => new UserDetailsViewModel
				{
					Id = u.Id,
					UserName = u.UserName,
					Roles = u.UserRoles.Select(ur => ur.Role.Name)
				})
				.SingleAsync(u => u.Id == id);
		}

		// TODO: document, for the User/Edit screen
		public async Task<UserEditViewModel> GetUserForEdit(int id)
		{
			// TODO: BeginTransaction
			var model = await _db.Users
				.Select(u => new UserEditViewModel
				{
					Id = u.Id,
					UserName = u.UserName,
					Email = u.Email,
					EmailConfirmed = u.EmailConfirmed,
					IsLockedOut = u.LockoutEnabled && u.LockoutEnd.HasValue
				})
				.SingleAsync(u => u.Id == id);

			model.AvailableRoles = await _db.Roles
				.Select(r => new SelectListItem
				{
					Value = r.Id.ToString(),
					Text = r.Name,
					Selected = model.SelectedRoles.Contains(r.Id)
				})
				.ToListAsync();

			return model;
		}

		// TODO: document
		public async Task EditUser(UserEditViewModel model)
		{
			var user = await _db.Users.SingleAsync(u => u.Id == model.Id);
			// TODO edit and save
		}

		// TODO: document
		public async Task UnlockUser(int id)
		{
			var user = await _db.Users.SingleAsync(u => u.Id == id);
			user.LockoutEnd = null;
			await _db.SaveChangesAsync();
		}

		public async Task<bool> CheckUserNameExists(string userName)
		{
			return await _db.Users
				.AnyAsync(u => u.UserName == userName);
		}
	}
}
