using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TASVideos.Data;
using TASVideos.ForumEngine;

/*
 * TASVideos.ForumEngine.Snapshots
 *
 * This tool generates snapshots of rendered forum posts for regression testing.

 *
 * Setup:
 * 1. Create a .params.json file with ConnectionString
 * 2. Build and run this project with appropriate arguments
 *
 * Arguments:
 * --generate: Generate initial snapshots (creates baseline files)
 * --compare:  Compare current rendering against snapshots (reports differences)
 * --update:   Update snapshots with current rendering results
 * --count N:  Number of posts to process (default: 10000)
 * --offset N: Starting offset for post selection (default: 0)
 */

var arguments = Environment.GetCommandLineArgs().Skip(1).ToArray();

var generateMode = arguments.Contains("--generate");
var compareMode = arguments.Contains("--compare");
var updateMode = arguments.Contains("--update");

var count = GetArgValue(arguments, "--count", 10000);
var offset = GetArgValue(arguments, "--offset", 0);

if (!generateMode && !compareMode && !updateMode)
{
	Console.WriteLine("Usage: TASVideos.ForumEngine.Snapshots [--generate|--compare|--update] [--count N] [--offset N]");
	Console.WriteLine("  --generate: Generate initial snapshots");
	Console.WriteLine("  --compare:  Compare current rendering against snapshots");
	Console.WriteLine("  --update:   Update snapshots with current rendering");
	Console.WriteLine("  --count N:  Number of posts to process (default: 10000)");
	Console.WriteLine("  --offset N: Starting offset for posts (default: 0)");
	return -1;
}

if (!File.Exists(".params.json"))
{
	Console.WriteLine(".params.json not found");
	Console.WriteLine("Create a .params.json file with:");
	Console.WriteLine("{ \"ConnectionString\": \"your_connection_string_here\" }");
	return -1;
}

var settings = JsonSerializer.Deserialize<Options>(File.ReadAllText(".params.json"));
if (string.IsNullOrWhiteSpace(settings?.ConnectionString))
{
	Console.WriteLine("Need to set ConnectionString value in .params.json");
	return -1;
}

var connectionString = settings.ConnectionString;

var serviceProvider = new ServiceCollection()
	.AddTasvideosData(true, connectionString)
	.BuildServiceProvider();

var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

Console.WriteLine($"Processing {count} forum posts starting at offset {offset}...");

// Get forum posts with diverse content for testing
var posts = await context.ForumPosts
	.Include(p => p.Poster)
	.Include(p => p.Topic)
	.Include(p => p.Forum)
	.Where(p => !string.IsNullOrEmpty(p.Text))
	.OrderBy(p => p.Id)
	.Skip(offset)
	.Take(count)
	.Select(p => new
	{
		p.Id,
		p.Text,
		p.EnableHtml,
		p.EnableBbCode,
		p.Subject,
		PosterName = p.Poster!.UserName,
		TopicTitle = p.Topic!.Title,
		ForumName = p.Forum!.Name
	})
	.ToListAsync();

Console.WriteLine($"Retrieved {posts.Count} posts from database.");

var snapshotsDir = Path.Combine(Directory.GetCurrentDirectory(), "snapshots");
Directory.CreateDirectory(snapshotsDir);

var errors = new List<string>();
var differences = new List<string>();
var processed = 0;

foreach (var post in posts)
{
	processed++;
	if (processed % 100 == 0)
	{
		Console.WriteLine($"Processed {processed}/{posts.Count} posts...");
	}

	var postDir = Path.Combine(snapshotsDir, $"post_{post.Id}");
	Directory.CreateDirectory(postDir);

	var metadataPath = Path.Combine(postDir, "metadata.json");
	var contentPath = Path.Combine(postDir, "rendered.txt");

	try
	{
		// Parse the forum post
		var parsed = PostParser.Parse(post.Text, post.EnableBbCode, post.EnableHtml);
		var rendered = RenderNode(parsed);

		var metadata = new PostMetadata
		{
			Id = post.Id,
			Subject = post.Subject,
			PosterName = post.PosterName,
			TopicTitle = post.TopicTitle,
			ForumName = post.ForumName,
			EnableHtml = post.EnableHtml,
			EnableBbCode = post.EnableBbCode,
			TextLength = post.Text.Length,
			GeneratedAt = DateTime.UtcNow
		};

		if (generateMode || updateMode)
		{
			// Write/update the snapshots
			await File.WriteAllTextAsync(metadataPath, JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));
			await File.WriteAllTextAsync(contentPath, rendered);
		}
		else if (compareMode)
		{
			// Compare against existing snapshots
			if (!File.Exists(contentPath))
			{
				differences.Add($"Post {post.Id}: Missing snapshot file");
				continue;
			}

			var existingContent = await File.ReadAllTextAsync(contentPath);
			if (existingContent != rendered)
			{
				differences.Add($"Post {post.Id}: Content differs from snapshot");

				// Write diff file for inspection
				var diffPath = Path.Combine(postDir, "diff.txt");
				await File.WriteAllTextAsync(diffPath, $"=== EXPECTED ===\n{existingContent}\n\n=== ACTUAL ===\n{rendered}");
			}
		}
	}
	catch (Exception ex)
	{
		errors.Add($"Post {post.Id}: {ex.Message}");
	}
}

Console.WriteLine($"\nCompleted processing {processed} posts.");

if (errors.Any())
{
	Console.WriteLine($"\nErrors encountered ({errors.Count}):");
	foreach (var error in errors.Take(10))
	{
		Console.WriteLine($"  {error}");
	}

	if (errors.Count > 10)
	{
		Console.WriteLine($"  ... and {errors.Count - 10} more errors");
	}
}

if (compareMode && differences.Any())
{
	Console.WriteLine($"\nDifferences found ({differences.Count}):");
	foreach (var diff in differences.Take(10))
	{
		Console.WriteLine($"  {diff}");
	}

	if (differences.Count > 10)
	{
		Console.WriteLine($"  ... and {differences.Count - 10} more differences");
	}

	return 1; // Exit with error code if differences found
}

Console.WriteLine(generateMode ? "Initial snapshots generated successfully." :
				  updateMode ? "Snapshots updated successfully." :
				  "All snapshots match current rendering.");

return 0;

static int GetArgValue(string[] args, string argName, int defaultValue)
{
	var index = Array.IndexOf(args, argName);
	if (index >= 0 && index < args.Length - 1 && int.TryParse(args[index + 1], out var value))
	{
		return value;
	}

	return defaultValue;
}

static string RenderNode(Element node, int depth = 0)
{
	var result = new StringWriter();
	RenderNodeRecursive(node, result, depth);
	return result.ToString();
}

static void RenderNodeRecursive(INode node, StringWriter writer, int depth)
{
	var indent = new string(' ', depth * 2);

	switch (node)
	{
		case Element element:
			writer.WriteLine($"{indent}<{element.Name}>");
			if (!string.IsNullOrEmpty(element.Options))
			{
				writer.WriteLine($"{indent}  @options={element.Options}");
			}

			foreach (var child in element.Children)
			{
				RenderNodeRecursive(child, writer, depth + 1);
			}

			writer.WriteLine($"{indent}</{element.Name}>");
			break;

		case Text text:
			var lines = text.Content.Split('\n');
			foreach (var line in lines)
			{
				if (!string.IsNullOrEmpty(line))
				{
					writer.WriteLine($"{indent}TEXT: {line}");
				}
			}

			break;
	}
}

public class PostMetadata
{
	public int Id { get; set; }
	public string? Subject { get; set; }
	public string? PosterName { get; set; }
	public string? TopicTitle { get; set; }
	public string? ForumName { get; set; }
	public bool EnableHtml { get; set; }
	public bool EnableBbCode { get; set; }
	public int TextLength { get; set; }
	public DateTime GeneratedAt { get; set; }
}

internal class Options
{
	public string? ConnectionString { get; init; }
}
