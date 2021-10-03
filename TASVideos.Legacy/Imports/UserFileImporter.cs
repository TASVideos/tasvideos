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
	internal static class UserFileImporter
	{
		public static void Import(NesVideosSiteContext legacySiteContext, IReadOnlyDictionary<int, int> userIdMapping)
		{
			var userFiles = legacySiteContext.UserFiles
				.Include(userFile => userFile.User)
				.ToList()
				.Select(userFile => Convert(userFile, userIdMapping))
				.ToList();

			var userFileComments = legacySiteContext.UserFileComments
				.Include(comment => comment.User)
				.Select(comment => Convert(comment, userIdMapping))
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
				nameof(UserFile.Warnings),
				nameof(UserFile.CompressionType)
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

			userFiles.BulkInsert(userFileColumns, nameof(ApplicationDbContext.UserFiles));
			userFileComments.BulkInsert(userFileCommentColumns, nameof(ApplicationDbContext.UserFileComments));
		}

		private static UserFileComment Convert(
			LegacyComment legacyComment,
			IReadOnlyDictionary<int, int> userIdMapping)
		{
			return new ()
			{
				CreationTimeStamp = ImportHelper.UnixTimeStampToDateTime(legacyComment.Timestamp),
				Id = legacyComment.Id,
				Ip = legacyComment.Ip,
				ParentId = legacyComment.ParentId,
				Text = legacyComment.Text,
				Title = ImportHelper.ConvertNotNullLatin1String(legacyComment.Title),
				UserId = userIdMapping[legacyComment.User!.Id],
				UserFileId = legacyComment.FileId
			};
		}

		private static UserFile Convert(
			LegacyUserFile legacyFile,
			IReadOnlyDictionary<int, int> userIdMapping)
		{
			return new ()
			{
				AuthorId = userIdMapping[legacyFile.User!.Id],
				Class = string.Equals(legacyFile.Class, "m", StringComparison.OrdinalIgnoreCase)
					? UserFileClass.Movie
					: UserFileClass.Support,
				Content = legacyFile.Content.ConvertXz(),
				Description = legacyFile.Description.NullIfWhiteSpace(),
				Downloads = legacyFile.Downloads,
				FileName = ImportHelper.ConvertNotNullLatin1String(legacyFile.Name),
				Frames = legacyFile.Frames,
				GameId = legacyFile.GameNameId,
				Hidden = legacyFile.Hidden != 0,
				Id = legacyFile.Id,
				Length = legacyFile.Length,
				LogicalLength = legacyFile.LogicalLength,
				PhysicalLength = legacyFile.PhysicalLength,
				Rerecords = (int)legacyFile.Rerecords,
				SystemId = legacyFile.SystemId,
				Title = ImportHelper.ConvertNotNullLatin1String(legacyFile.Title),
				Type = legacyFile.Type,
				UploadTimestamp = ImportHelper.UnixTimeStampToDateTime(legacyFile.Timestamp),
				Views = legacyFile.Views,
				Warnings = legacyFile.Warnings.NullIfWhiteSpace(),
				CompressionType = Compression.Gzip
			};
		}
	}
}
