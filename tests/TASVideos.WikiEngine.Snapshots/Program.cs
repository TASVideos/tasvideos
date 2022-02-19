using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.WikiEngine;
using TASVideos.WikiEngine.AST;

//namespace TASVideos.WikiEngine.Snapshots;

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

if (!File.Exists(".params.json"))
{
	Console.WriteLine(".params.json not found");
	return -1;
}

var settings = JsonSerializer.Deserialize<Options>(File.ReadAllText(".params.json"));
if (string.IsNullOrWhiteSpace(settings?.ConnectionString) || string.IsNullOrWhiteSpace(settings.OutDir))
{
	Console.WriteLine("Need to set values in .params.json");
	return -1;
}

var serviceProvider = new ServiceCollection()
	.AddTasvideosData(settings.ConnectionString)
	.BuildServiceProvider();

var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

var wantUpdate = args.Any(s => s == "--update");
var force = args.Any(s => s == "--force");
string? filter = null;
{
	var index = Array.IndexOf(args, "--filter");
	if (index >= 0 && index < args.Length - 1)
	{
		filter = args[index + 1];
	}
}

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
		using var tr = new StreamReader(path);
		var result = int.TryParse(tr.ReadLine(), out int parsedRevision);
		if (result)
		{
			existingRevision = parsedRevision;
		}
	}

	var revision = wantUpdate ? wp.Revision : existingRevision != -1 ? existingRevision : wp.Revision;

	if (existingRevision == revision && !force)
		continue;

	var markup = context.WikiPages
		.Where(p => p.Revision == revision && p.PageName == wp.PageName)
		.Select(p => p.Markup)
		.Single();

	List<INode> nodes;

	try
	{
		nodes = NewParser.Parse(markup);
	}
	catch (NewParser.SyntaxException e)
	{
		nodes = Builtins.MakeErrorPage(markup, e);
	}

	using var tw = new StreamWriter(path);
	tw.WriteLine(revision);
	foreach (var node in nodes)
	{
		node.DumpContentDescriptive(tw, "");
	}
}

Console.Write(new string('\b', 8));
Console.Write("{0,8}", ++progress);
Console.WriteLine();
return 0;

internal class Options
{
	public string? ConnectionString { get; init; }
	public string? OutDir { get; init; }
}
