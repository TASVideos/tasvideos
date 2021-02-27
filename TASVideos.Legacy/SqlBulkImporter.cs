using System;
using System.Collections.Generic;
using System.Linq;
using FastMember;
using Npgsql;

namespace TASVideos.Legacy
{
	public static class SqlBulkImporter
	{
		public static void BeginImport(string connectionString, string providerName)
		{
			ConnectionString = connectionString;
			IsMsSql = providerName.EndsWith("SqlServer");
		}

		private static string ConnectionString { get; set; } = "";

		public static bool IsMsSql { get; set; }

		public static void BulkInsertMssql<T>(IEnumerable<T> data, string[] columnsToCopy, string tableName)
		{
			var keepIdentity = columnsToCopy.Contains("Id");
			var options = keepIdentity
				? Microsoft.Data.SqlClient.SqlBulkCopyOptions.KeepIdentity
				: Microsoft.Data.SqlClient.SqlBulkCopyOptions.Default;
			using var sqlCopy = new Microsoft.Data.SqlClient.SqlBulkCopy(ConnectionString, options)
			{
				DestinationTableName = tableName,
				BatchSize = 10000,
				BulkCopyTimeout = 1200
			};

			foreach (var param in columnsToCopy)
			{
				sqlCopy.ColumnMappings.Add(param, param);
			}

			using var reader = ObjectReader.Create(data, columnsToCopy);
			sqlCopy.WriteToServer(reader);
		}

		public static void BulkInsertPostgres<T>(
			IEnumerable<T> data,
			string[] columnsToCopy,
			string tableName)
		{
			using var connection = new NpgsqlConnection(ConnectionString);
			connection.Open();

			DisableConstraints(tableName, connection);

			var copyCommand = $"COPY public.\"{tableName}\" ({string.Join(", ", columnsToCopy.Select(c => $"\"{c}\""))}) FROM STDIN (FORMAT BINARY)";
			using (var writer = connection.BeginBinaryImport(copyCommand))
			{
				var writeMethod = writer
					.GetType()
					.GetMethods()
					.Single(m => m.Name == "Write" && m.GetParameters().Length == 1);

				var propertiesDic = columnsToCopy
					.ToDictionary(c => c, c => typeof(T).GetProperty(c));

				foreach (var row in data)
				{
					writer.StartRow();
					foreach (var column in columnsToCopy)
					{
						var prop = propertiesDic[column];
						var value = prop!.GetValue(row);
						if (value == null)
						{
							writer.WriteNull();
						}
						else if (prop!.PropertyType.IsEnum)
						{
							writer.Write(Convert.ToInt32(value));
						}
						else
						{
							var type = prop!.PropertyType;
							if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
							{
								type = type.GetGenericArguments()[0];
							}

							writeMethod
								.MakeGenericMethod(type)
								.Invoke(writer, new[] { value });
						}
					}
				}

				writer.Complete();
			}

			EnableConstraints(tableName, connection);

			// Ideally we would determine if the Id column is actually an identity column
			// However, only user files has this situation, so shenanigans are good enough
			if (columnsToCopy.Contains("Id") && tableName != "UserFiles")
			{
				SetIdSeed(tableName, connection);
			}
		}

		private static void DisableConstraints(string tableName, NpgsqlConnection connection)
		{
			var sql = $"alter table public.\"{tableName}\" disable trigger all ";
			using var cmd = new NpgsqlCommand(sql, connection);
			cmd.ExecuteScalar();
		}

		private static void EnableConstraints(string tableName, NpgsqlConnection connection)
		{
			var sql = $"alter table public.\"{tableName}\" enable trigger all ";
			using var cmd = new NpgsqlCommand(sql, connection);
			cmd.ExecuteScalar();
		}

		private static void SetIdSeed(string tableName, NpgsqlConnection connection)
		{
			int identitySeed;

			using (var cmd = new NpgsqlCommand($"select \"Id\" from public.\"{tableName}\" order by \"Id\" desc limit 1", connection))
			{
				identitySeed = (int)cmd.ExecuteScalar();
			}

			using (var cmd2 = new NpgsqlCommand($"ALTER TABLE public.\"{tableName}\" ALTER COLUMN \"Id\" RESTART WITH {identitySeed + 1}", connection))
			{
				cmd2.ExecuteScalar();
			}
		}
	}
}
