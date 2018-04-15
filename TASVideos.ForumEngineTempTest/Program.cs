using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using TASVideos.ForumEngine;

namespace TASVideos.ForumEngineTempTest
{
	class Post
	{
		public bool EnableBbCode { get; set; }
		public bool EnableHtml { get; set; }
		public string Text { get; set; }
		public int PosterId { get; set; }
		public int Id { get; set; }
	}

	class Program
	{
		static void Main(string[] args)
		{
			var builder = new SqlConnectionStringBuilder();
			builder.DataSource = "(localdb)\\mssqllocaldb";
			builder.InitialCatalog = "TASVideos";
			builder.IntegratedSecurity = true;
			using (var connection = new SqlConnection(builder.ToString()))
			{
				var htmlCount = 0;
				foreach (var post in connection.Query<Post>("select EnableBbCode, EnableHtml, Text, PosterId, Id from ForumPosts"))
				{
					try
					{
						var parsed = PostParser.Parse(post.Text, post.EnableBbCode, post.EnableHtml);
						if (post.EnableHtml &&HtmlParser.ContainsHtml(post.Text))
							htmlCount++;
					}
					catch (Exception e)
					{
						Console.WriteLine(post.Text);
						Console.WriteLine("#####" + post.Id);
						Console.WriteLine(e.Message);
						return;
					}
				}
				Console.WriteLine(htmlCount);
			}
		}
	}
}
