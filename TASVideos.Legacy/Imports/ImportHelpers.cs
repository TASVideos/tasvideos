using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using FastMember;
using Microsoft.Data.SqlClient;
using SharpCompress.Compressors.Xz;
using TASVideos.Extensions;

namespace TASVideos.Legacy.Imports
{
	public static class ImportHelper
	{
		private static readonly DateTime UnixStart = new (1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

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

		public static string ConvertNotNullLatin1String(string input)
		{
			return ConvertLatin1String(input) ?? "";
		}

		public static string? ConvertLatin1String(string? input)
		{
			if (input == null)
			{
				return null;
			}

			var enc = Encoding.GetEncoding(1252);
			var data = enc.GetBytes(input);
			return Encoding.UTF8.GetString(data);
		}

		public static string ReplaceInsensitive(this string input, string search, string replacement)
		{
			return Regex.Replace(
				input,
				Regex.Escape(search),
				replacement.Replace("$", "$$"),
				RegexOptions.IgnoreCase);
		}

		public static void BulkInsert<T>(
			this IEnumerable<T> data,
			string connectionString,
			string[] columnsToCopy,
			string tableName,
			int batchSize = 10000,
			int? bulkCopyTimeout = null)
		{
			var keepIdentity = columnsToCopy.Contains("Id");
			var options = keepIdentity ? SqlBulkCopyOptions.KeepIdentity : SqlBulkCopyOptions.Default;
			using var sqlCopy = new SqlBulkCopy(connectionString, options)
			{
				DestinationTableName = tableName,
				BatchSize = batchSize
			};
			if (bulkCopyTimeout.HasValue)
			{
				sqlCopy.BulkCopyTimeout = bulkCopyTimeout.Value;
			}

			foreach (var param in columnsToCopy)
			{
				sqlCopy.ColumnMappings.Add(param, param);
			}

			using var reader = ObjectReader.Create(data, columnsToCopy);
			sqlCopy.WriteToServer(reader);
		}

		public static string? Cap(this string? str, int limit)
		{
			if (str == null)
			{
				return null;
			}

			return str.Length < limit
				? str
				: str[..limit];
		}

		public static IEnumerable<string> ParseUserNames(this string authors)
		{
			if (string.IsNullOrWhiteSpace(authors))
			{
				return Enumerable.Empty<string>();
			}

			var names = authors
				.SplitWithEmpty(",")
				.SelectMany(s => s.SplitWithEmpty("&"))
				.SelectMany(s => s.SplitWithEmpty(" and "))
				.Select(s => s.Trim())
				.Where(s => !string.IsNullOrWhiteSpace(s));

			return names;
		}

		public static string? NullIfWhiteSpace(this string? str)
		{
			return string.IsNullOrWhiteSpace(str)
				? null
				: str;
		}

		/// <summary>
		/// Converts an XZ compressed file to a GZIP compressed file.
		/// </summary>
		public static byte[] ConvertXz(this byte[] content)
		{
			if (content.Length == 0)
			{
				return content;
			}

			using var targetStream = new MemoryStream();
			using var gzipStream = new GZipStream(targetStream, CompressionLevel.Optimal);
			using var sourceStream = new MemoryStream(content);
			using var xzStream = new XZStream(sourceStream);
			xzStream.CopyTo(gzipStream);
			var result = targetStream.ToArray();

			return result;
		}

		public static string? IpFromHex(this string? hexIp)
		{
			if (string.IsNullOrWhiteSpace(hexIp) || hexIp == "?")
			{
				return null;
			}

			// IPv4
			if (hexIp.Length == 8)
			{
				var ipv4 = new IPAddress(long.Parse(hexIp, NumberStyles.AllowHexSpecifier));
				return ipv4.ToString();
			}

			// IPv6
			var hex = ParseHex(hexIp);
			var ipv6 = new IPAddress(hex);
			return ipv6.ToString();
		}

		private static byte[] ParseHex(string hex)
		{
			int offset = hex.StartsWith("0x") ? 2 : 0;
			if ((hex.Length % 2) != 0)
			{
				throw new ArgumentException("Invalid length: " + hex.Length);
			}

			byte[] ret = new byte[(hex.Length - offset) / 2];

			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = (byte)((ParseNybble(hex[offset]) << 4)
					| ParseNybble(hex[offset + 1]));
				offset += 2;
			}

			return ret;
		}

		private static int ParseNybble(char c)
		{
			return c switch
			{
				'0' => c - '0',
				'1' => c - '0',
				'2' => c - '0',
				'3' => c - '0',
				'4' => c - '0',
				'5' => c - '0',
				'6' => c - '0',
				'7' => c - '0',
				'8' => c - '0',
				'9' => c - '0',
				'A' => c - 'A' + 10,
				'B' => c - 'A' + 10,
				'C' => c - 'A' + 10,
				'D' => c - 'A' + 10,
				'E' => c - 'A' + 10,
				'F' => c - 'A' + 10,
				'a' => c - 'a' + 10,
				'b' => c - 'a' + 10,
				'c' => c - 'a' + 10,
				'd' => c - 'a' + 10,
				'e' => c - 'a' + 10,
				'f' => c - 'a' + 10,
				_ => throw new ArgumentException("Invalid hex digit: " + c)
			};
		}
	}
}
