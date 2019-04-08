using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
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

		public static string ConvertUtf8(string input)
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

		public static string ConvertLatin1String(string input)
		{
			if (input == null)
			{
				return null;
			}

			var enc = Encoding.GetEncoding(1252);
			var data = enc.GetBytes(input);
			return Encoding.UTF8.GetString(data);
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

		public static string Cap(this string str, int limit)
		{
			if (str == null)
			{
				return null;
			}

			if (str.Length < limit)
			{
				return str;
			}

			return str.Substring(0, limit);
		}

		public static IEnumerable<string> ParseUserNames(this string authors)
		{
			if (string.IsNullOrWhiteSpace(authors))
			{
				return Enumerable.Empty<string>();
			}

			var names = authors
				.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
				.SelectMany(s => s.Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries))
				.SelectMany(s => s.Split(new[] { " and " }, StringSplitOptions.RemoveEmptyEntries))
				.Select(s => s.Trim())
				.Where(s => !string.IsNullOrWhiteSpace(s));

			return names;
		}

		public static string NullIfWhiteSpace(this string str)
		{
			return string.IsNullOrWhiteSpace(str)
				? null
				: str;
		}
	}
}
