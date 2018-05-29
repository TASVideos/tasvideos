using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

using FastMember;

namespace TASVideos.Legacy.Imports
{
	public static class ImportHelper
	{
		private static readonly DateTime UnixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

		public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
		{
			return UnixStart.AddSeconds(unixTimeStamp);
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
			int batchSize = 10000,
			int? bulkCopyTimeout = null)
		{
			using (var sqlCopy = new SqlBulkCopy(connectionString, options))
			{
				sqlCopy.DestinationTableName = tableName;
				sqlCopy.BatchSize = batchSize;
				if (bulkCopyTimeout.HasValue)
				{
					sqlCopy.BulkCopyTimeout = bulkCopyTimeout.Value;
				}

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
