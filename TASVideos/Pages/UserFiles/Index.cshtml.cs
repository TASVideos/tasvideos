using System;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Pages.UserFiles.Models;
using TASVideos.Services.ExternalMediaPublisher;

namespace TASVideos.Pages.UserFiles
{
	[AllowAnonymous]
	public class IndexModel : BasePageModel
	{
		private readonly ApplicationDbContext _db;
		private readonly ExternalMediaPublisher _publisher;

		public IndexModel(
			ApplicationDbContext db,
			ExternalMediaPublisher publisher)
		{
			_db = db;
			_publisher = publisher;
		}

		public UserFileIndexModel Data { get; set; }

		public async Task OnGet()
		{
			Data = new UserFileIndexModel
			{
				UsersWithMovies = await _db.UserFiles
					.Where(uf => !uf.Hidden)
					.GroupBy(gkey => gkey.Author.UserName, gvalue => gvalue.UploadTimestamp).Select(
						uf => new UserFileIndexModel.UserWithMovie { UserName = uf.Key, Latest = uf.Max() })
					.ToListAsync(),
				LatestMovies = await _db.UserFiles
					.ThatArePublic()
					.ByRecentlyUploaded()
					.ProjectTo<UserMovieListModel>()
					.Take(10)
					.ToListAsync(),
				GamesWithMovies = await _db.Games
					.Where(g => g.UserFiles.Any())
					.OrderBy(g => g.System.Code)
					.ThenBy(g => g.DisplayName)
					.Select(g => new UserFileIndexModel.GameWithMovie
					{
						GameId = g.Id,
						GameName = g.DisplayName,
						SystemCode = g.System.Code,
						Latest = g.UserFiles.Select(uf => uf.UploadTimestamp).Max()
					})
					.ToListAsync()
			};
		}

		public async Task<IActionResult> OnPostComment(long fileId, string comment, string returnUrl)
		{
			if (User.Has(PermissionTo.CreateForumPosts)
				&& !string.IsNullOrWhiteSpace(comment))
			{
				var userFile = await _db.UserFiles.SingleOrDefaultAsync(u => u.Id == fileId);
				if (userFile != null)
				{
					_db.UserFileComments.Add(new UserFileComment
					{
						UserFileId = fileId,
						Text = comment,
						UserId = User.GetUserId(),
						Ip = IpAddress.ToString()
					});

					await _db.SaveChangesAsync();
					_publisher.SendUserFile(
						$"New comment by {User.Identity.Name} on ({userFile.Title} (WIP))", 
						$"{BaseUrl}/UserFiles/Info/{fileId}");
				}
			}

			return RedirectToLocal(returnUrl);
		}

		public async Task<IActionResult> OnPostEditComment(long commentId, string comment, string returnUrl)
		{
			if (User.Has(PermissionTo.CreateForumPosts)
				&& !string.IsNullOrWhiteSpace(comment))
			{
				var fileComment = await _db.UserFileComments
					.Include(c => c.UserFile)
					.SingleOrDefaultAsync(u => u.Id == commentId);

				if (fileComment != null)
				{
					fileComment.Text = comment;

					try
					{
						await _db.SaveChangesAsync();
						_publisher.SendUserFile(
							$"Comment edited by {User.Identity.Name} on ({fileComment.UserFile.Title} (WIP))",
							$"{BaseUrl}/UserFiles/Info/{fileComment.UserFile.Id}");
					}
					catch (DbUpdateConcurrencyException)
					{
						// Do nothing
					}
				}
			}

			return RedirectToLocal(returnUrl);
		}

		public async Task<IActionResult> OnPostDeleteComment(long commentId, string returnUrl)
		{
			if (User.Has(PermissionTo.CreateForumPosts))
			{
				var fileComment = await _db.UserFileComments
					.Include(c => c.UserFile)
					.Include(c => c.User)
					.SingleOrDefaultAsync(u => u.Id == commentId);

				if (fileComment != null)
				{
					_db.UserFileComments.Remove(fileComment);

					try
					{
						await _db.SaveChangesAsync();
						_publisher.SendUserFile(
							$"Comment by {fileComment.User.UserName} on ({fileComment.UserFile.Title} (WIP)) deleted by {User.Identity.Name}", 
							$"{BaseUrl}/UserFiles/Info/{fileComment.UserFile.Id}");
					}
					catch (DbUpdateConcurrencyException)
					{
						// Do nothing
					}
				}
			}

			return RedirectToLocal(returnUrl);
		}
	}
}
