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
			var content = @"
			[video]https://www.youtube.com/watch?v=yLORZbc-PZw[/video]
			[video]https://youtu.be/yLORZbc-PZw[/video]
			[video]http://www.youtube.com/view_play_list?p=76E50B82FA870C1D[/video]
			[video]http://www.dailymotion.com/video/xf4u2m_snes-breath-of-fire-wip-by-janus_videogames[/video]
			[video]http://vimeo.com/49142543[/video]
			[video]https://www.nicovideo.jp/watch/sm35061034[/video]
			";
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
