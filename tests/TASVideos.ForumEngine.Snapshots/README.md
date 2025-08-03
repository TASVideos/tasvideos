# TASVideos.ForumEngine.Snapshots

This tool generates snapshots of forum post rendering for regression testing of the ForumEngine. Due to the large number of forum posts (~500k), snapshots are stored locally and git-ignored.

## Setup

1. Create a `.params.json` file in this directory with your database connection string:
   ```json
   {
     "ConnectionString": "your_postgresql_connection_string_here"
   }
   ```

2. Build the project:
   ```bash
   dotnet build
   ```

## Usage

### Generate Initial Snapshots
```bash
dotnet run -- --generate --count 1000
```

Creates baseline snapshot files for 1000 forum posts. These files are used for comparison in future runs.

### Compare Against Snapshots
```bash
dotnet run -- --compare --count 1000
```

Compares current forum post rendering against existing snapshots and reports any differences. Exits with code 1 if differences are found.

### Update Snapshots
```bash
dotnet run -- --update --count 1000
```

Updates existing snapshots with current rendering results. Use this when you've made intentional changes to the ForumEngine.

## Arguments

- `--generate`: Generate initial snapshots (creates baseline files)
- `--compare`: Compare current rendering against snapshots (reports differences)
- `--update`: Update snapshots with current rendering results
- `--count N`: Number of posts to process (default: 1000)
- `--offset N`: Starting offset for post selection (default: 0)

## Output Structure

For each forum post, the tool creates:
- `snapshots/post_{id}/metadata.json`: Post metadata (title, poster, etc.)
- `snapshots/post_{id}/rendered.txt`: Parsed AST representation
- `snapshots/post_{id}/diff.txt`: Difference file (only created during comparison if differences found)

## Files

- All snapshot files are git-ignored via `.gitignore`
- The tool selects a diverse range of posts for testing various ForumEngine features
- Posts are processed in ID order for consistent results

## Integration

This tool is designed to be used in CI/CD pipelines to catch ForumEngine regressions:

1. Generate snapshots from a known-good version
2. Run comparisons after code changes
3. Update snapshots when changes are intentional