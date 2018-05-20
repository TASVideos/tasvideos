using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

using FastMember;
using Microsoft.EntityFrameworkCore;

namespace TASVideos.Legacy.Imports
{
	public static class ImportHelper
	{
		public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
		{
			// Unix timestamp is seconds past epoch
			DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			start = start.AddSeconds(unixTimeStamp);
			return start;
		}

		public static string FixString(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return input;
			}

			var b = new byte[input.Length];
			for (var i = 0; i < input.Length; i++)
			{
				b[i] = (byte)input[i];
			}

			return Encoding.UTF8.GetString(b);
		}

		public static void BulkInsert<T>(
			this IEnumerable<T> data,
			string connectionString,
			string[] columnsToCopy,
			string tableName,
			SqlBulkCopyOptions options = SqlBulkCopyOptions.KeepIdentity,
			int batchSize = 10000)
		{
			using (var sqlCopy = new SqlBulkCopy(connectionString, options))
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
