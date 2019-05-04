using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;

namespace TASVideos.WikiEngineTest
{
	class Program
	{
		class Options
		{
			public string ConnectionString { get; set; }
			public string OutDir { get; set; }
		}
		static int Main(string[] args)
		{
			if (!File.Exists(".params.json"))
			{
				Console.WriteLine(".params.json not found");
				return -1;
			}
			var settings = JsonConvert.DeserializeObject<Options>(File.ReadAllText(".params.json"));
			if (string.IsNullOrWhiteSpace(settings.ConnectionString) || string.IsNullOrWhiteSpace(settings.OutDir))
			{
				Console.WriteLine("Need to set values in .params.json");
				return -1;
			}

			var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseSqlServer(settings.ConnectionString)
				.Options;

			var wantUpdate = args.Any(s => s == "--update");
			var force = args.Any(s => s == "--force");

			using (var context = new ApplicationDbContext(contextOptions, null))
			{
				var toProcess = context.WikiPages
					.ThatAreNotDeleted()
					.ThatAreCurrentRevisions()
					.Select(wp => new
					{
						wp.PageName,
						wp.Revision
					})
					.ToList();
				Console.WriteLine($"Found {toProcess.Count} current page revisions.");

				var progress = 0;
				Console.Write("{0,8}", progress);

				foreach (var wp in toProcess)
				{
					Console.Write(new string('\b', 8));
					Console.Write("{0,8}", progress++);

					var directory = Path.Combine(settings.OutDir, wp.PageName);
					new DirectoryInfo(directory).Create();
					var path = Path.Combine(directory, "content");

					var existingRevision = -1;
					if (File.Exists(path))
					{
						using (var tr = new StreamReader(path))
							existingRevision = int.Parse(tr.ReadLine());
					}

					var revision = wantUpdate ? wp.Revision : existingRevision != -1 ? existingRevision : wp.Revision;

					if (existingRevision == revision && !force)
						continue;

					var markup = context.WikiPages
						.Where(p => p.Revision == revision && p.PageName == wp.PageName)
						.Select(p => p.Markup)
						.Single();

					List<WikiEngine.AST.INode> nodes;

					try
					{
						nodes = NewParser.Parse(markup);
					}
					catch (NewParser.SyntaxException e)
					{
						nodes = NewParser.MakeErrorPage(markup, e);
					}

					using (var tw = new StreamWriter(path))
					{
						tw.WriteLine(revision);
						foreach (var node in nodes)
						{
							node.DumpContentDescriptive(tw, "");
						}
					}
				}
				Console.Write(new string('\b', 8));
				Console.Write("{0,8}", progress++);
			}
			Console.WriteLine();

			return 0;
		}
	}
}
