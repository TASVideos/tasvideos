using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Legacy.Data.Site;

using LegacyComment = TASVideos.Legacy.Data.Site.Entity.UserFileComment;
using LegacyUserFile = TASVideos.Legacy.Data.Site.Entity.UserFile;

namespace TASVideos.Legacy.Imports
{
	public static class UserFileImporter
	{
		public static void Import(
			ApplicationDbContext context,
			NesVideosSiteContext legacySiteContext)
		{
			using (legacySiteContext.Database.BeginTransaction())
			{
				var userIdsByName = context.Users.ToDictionary(
					user => user.UserName,
					user => user.Id,
					StringComparer.OrdinalIgnoreCase);

				var userFiles = legacySiteContext.UserFiles
					.Include(userFile => userFile.User)
					.Select(userFile => Convert(userFile, userIdsByName))
					.ToList();

				var userFileComments = legacySiteContext.UserFileComments
					.Include(userFile => userFile.User)
					.Select(userFileComment => Convert(userFileComment, userIdsByName))
					.ToList();

				var userFileColumns = new[]
				{
					nameof(UserFile.AuthorId),
					nameof(UserFile.Class),
					nameof(UserFile.Content),
					nameof(UserFile.Description),
					nameof(UserFile.Downloads),
					nameof(UserFile.FileName),
					nameof(UserFile.Frames),
					nameof(UserFile.GameId),
					nameof(UserFile.Hidden),
					nameof(UserFile.Id),
					nameof(UserFile.Length),
					nameof(UserFile.LogicalLength),
					nameof(UserFile.PhysicalLength),
					nameof(UserFile.Rerecords),
					nameof(UserFile.SystemId),
					nameof(UserFile.Title),
					nameof(UserFile.Type),
					nameof(UserFile.UploadTimestamp),
					nameof(UserFile.Views),
					nameof(UserFile.Warnings)
				};

				var userFileCommentColumns = new[]
				{
					nameof(UserFileComment.CreationTimeStamp),
					nameof(UserFileComment.Id),
					nameof(UserFileComment.Ip),
					nameof(UserFileComment.ParentId),
					nameof(UserFileComment.Text),
					nameof(UserFileComment.Title),
					nameof(UserFileComment.UserId),
					nameof(UserFileComment.UserFileId)
				};

				userFiles.BulkInsert(context, userFileColumns, nameof(ApplicationDbContext.UserFiles));
				userFileComments.BulkInsert(context, userFileCommentColumns, nameof(ApplicationDbContext.UserFileComments));
			}
		}

		private static UserFileComment Convert(
			LegacyComment legacyComment,
			Dictionary<string, int> userIdsByName)
		{
			return new UserFileComment
			{
				CreationTimeStamp = ImportHelper.UnixTimeStampToDateTime(legacyComment.Timestamp),
				Id = legacyComment.Id,
				Ip = legacyComment.Ip,
				ParentId = legacyComment.ParentId,
				Text = legacyComment.Text,
				Title = legacyComment.Title,
				UserId = userIdsByName.TryGetValue(legacyComment.User.Name, out var userId)
				? userId
				: -1,
				UserFileId = legacyComment.FileId
			};
		}

		private static UserFile Convert(
			LegacyUserFile legacyFile,
			Dictionary<string, int> userIdsByName)
		{
			return new UserFile
			{
				AuthorId = userIdsByName.TryGetValue(legacyFile.User.Name, out var userId)
					? userId
					: -1,
				Class = string.Equals(legacyFile.Class, "m", StringComparison.OrdinalIgnoreCase)
					? UserFileClass.Movie
					: UserFileClass.Support,
				Content = legacyFile.Content,
				Description = legacyFile.Description,
				Downloads = legacyFile.Downloads,
				FileName = legacyFile.Name,
				Frames = legacyFile.Frames,
				GameId = legacyFile.GameNameId,
				Hidden = legacyFile.Hidden != 0,
				Id = legacyFile.Id,
				Length = legacyFile.Length,
				LogicalLength = legacyFile.LogicalLength,
				PhysicalLength = legacyFile.PhysicalLength,
				Rerecords = (int)legacyFile.Rerecords,
				SystemId = legacyFile.SystemId,
				Title = legacyFile.Title,
				Type = legacyFile.Type,
				UploadTimestamp = ImportHelper.UnixTimeStampToDateTime(legacyFile.Timestamp),
				Views = legacyFile.Views,
				Warnings = legacyFile.Warnings
			};
		}
	}
}
