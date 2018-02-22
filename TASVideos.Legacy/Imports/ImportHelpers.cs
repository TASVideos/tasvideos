using System;
using System.Collections.Generic;
using System.Data.SqlClient;

using FastMember;
using Microsoft.EntityFrameworkCore;

namespace TASVideos.Legacy.Imports
{
	public static class ImportHelper
	{
		public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
		{
			// Unix timestamp is seconds past epoch
			DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
			return dtDateTime;
		}

		public static void BulkInsert<T>(
			this IEnumerable<T> data,
			DbContext context,
			string[] columnsToCopy,
			string tableName,
			SqlBulkCopyOptions options = SqlBulkCopyOptions.KeepIdentity,
			int batchSize = 10000)
		{
			using (var sqlCopy = new SqlBulkCopy(context.Database.GetDbConnection().ConnectionString, options))
			{
				sqlCopy.DestinationTableName = tableName;
				sqlCopy.BatchSize = batchSize;

				foreach (var param in columnsToCopy)
				{
					sqlCopy.ColumnMappings.Add(param, param);
				}

				using (var reader = ObjectReader.Create(data, columnsToCopy))
				{
					sqlCopy.WriteToServer(reader);
				}
			}
		}
	}
}
