using System;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using TASVideos.Data;
using TASVideos.ForumEngine;

namespace TASVideos.ForumEngineTempTest
{
	public class Program
	{
		public static void Main()
		{
			var content = @"Bonk boff <b>Bu[i]r[/i]p</b>";
			var containsHtml = BbParser.ContainsHtml(content, true);
			var parsed = PostParser.Parse(content, true, containsHtml);

			Console.WriteLine(containsHtml);
			parsed.WriteHtml(Console.Out);
		}

		public static void MainDa(string[] args)
		{
			/*var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TASVideos;Trusted_Connection=True;MultipleActiveResultSets=true") // TODO: app settings
				.Options;

			using (var context = new ApplicationDbContext(options, null))
			{
				var posts = context.ForumPosts.Take(500).ToList();

				var htmlCount = 0;
				foreach (var post in posts)
				{
					try
					{
						var parsed = PostParser.Parse(post.Text, post.EnableBbCode, post.EnableHtml);
						parsed.WriteHtml(Console.Out);
						if (post.EnableHtml && HtmlParser.ContainsHtml(post.Text))
						{
							htmlCount++;
						}
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
			}*/
		}
	}
}
