# TASVideos ForumEngine Benchmarks

This project contains performance benchmarks for the TASVideos ForumEngine using BenchmarkDotNet.

## Running the Benchmarks

```bash
dotnet run --project tests/TASVideos.ForumEngine.Benchmarks --configuration Release
```

## Current Benchmarks

- **ParseBbCode**: Benchmarks parsing BBCode markup into an AST
- **RenderToHtml**: Benchmarks rendering parsed BBCode to HTML output
- **RenderToMetaDescription**: Benchmarks extracting meta description text from parsed BBCode

## Test Data

The benchmarks use a comprehensive test post that exercises all major ForumEngine features:

- **Text formatting**: Bold, italic, underline, strikethrough, subscript, superscript, typewriter
- **Layout**: Left/center/right alignment, quotes, warnings, notes, spoilers  
- **Colors and sizing**: Text color, background color, font size
- **Links**: URLs, emails, TASVideos-specific links (movies, submissions, games, etc.)
- **Media**: Images, videos with size parameters
- **Code blocks**: With syntax highlighting and download links
- **Lists**: Unordered and ordered lists with nested formatting
- **Tables**: Multi-row tables with headers and formatted content
- **Special features**: Frame timing, Google search, horizontal rules, noparse blocks
- **HTML tags**: All supported HTML elements (when HTML parsing is enabled)

The test content includes ~100 lines of varied markup to provide realistic performance measurements.
