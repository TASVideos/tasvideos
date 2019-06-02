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
	/*
		1. Clone https://github.com/nattthebear/TASVideosWikiSnaps.git to somewhere
		2. Create a `.params.json` file containing 2 keys:
			ConnectionString - to connect to the database of the tasvideos dev server
			OutDir - wherever you cloned https://github.com/nattthebear/TASVideosWikiSnaps.git to
		3. Build and run the program.  Args:
			--force:  Rerun parsing and output on a page even if the revision is the same as before.
				This makes sense whenever you want to observe changes in the parsing or importing code
			--update:  Process the latest revision of each page, instead of the one recorded in the snapshots.
				This is good for examining user content diffs, but shouldn't be done in the same commit as parsing/import changes to minimize confusion.
			--filter [string]:  Only process pages whose title contains [string].
				Used to quickly observe changes to a single page during development.  Before committing to the snapshot repo, all other pages should be run.
	 */
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
			string filter = null;
			{
				var index = Array.IndexOf(args, "--filter");
				if (index >= 0 && index < args.Length - 1)
					filter = args[index + 1];
			}

			using (var context = new ApplicationDbContext(contextOptions, null))
			{
				var query = context.WikiPages
					.ThatAreNotDeleted()
					.WithNoChildren();
				if (filter != null)
				{
					query = query.Where(wp => wp.PageName.Contains(filter));
				}
				var toProcess = query
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
						nodes = Builtins.MakeErrorPage(markup, e);
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
