using System;
using System.Data;
using System.Data.SqlClient;

namespace TASVideos.Legacy.Imports
{
	public static class ImportHelpers
	{
		public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
		{
			// Unix timestamp is seconds past epoch
			DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
			return dtDateTime;
		}

		public static void SetIdentityInsertOff(string tableName, string connectionString)
		{
			using (var sqlConnection = new SqlConnection(connectionString))
			{
				using (var cmd = new SqlCommand
				{
					CommandText = $"SET IDENTITY_INSERT {tableName} OFF",
					CommandType = CommandType.Text,
					Connection = sqlConnection
				})
				{
					sqlConnection.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}

		public static void SetIdentityInsertOn(string tableName, string connectionString)
		{
			using (var sqlConnection = new SqlConnection(connectionString))
			{
				using (var cmd = new SqlCommand
				{
					CommandText = $"SET IDENTITY_INSERT {tableName} ON",
					CommandType = CommandType.Text,
					Connection = sqlConnection
				})
				{
					sqlConnection.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}
