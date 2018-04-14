using System;
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
				foreach (var post in connection.Query<Post>("select top 2000 EnableBbCode, EnableHtml, Text from ForumPosts order by CreateTimeStamp desc"))
				{
					if (post.EnableBbCode)
					{
						var parsed = BbParser.Parse(post.Text);
					}
				}
			}
        }
    }
}
