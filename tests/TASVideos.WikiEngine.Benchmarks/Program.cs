using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using TASVideos.WikiEngine.AST;
using TASVideos.WikiEngine.Benchmarks;

BenchmarkRunner.Run<WikiEngineBenchmarks>();

namespace TASVideos.WikiEngine.Benchmarks
{
	[MemoryDiagnoser]
	[SimpleJob]
	public class WikiEngineBenchmarks
	{
		private readonly IWriterHelper _writerHelper = NullWriterHelper.Instance;

		[Benchmark]
		public List<INode> ParseMarkup()
		{
			return NewParser.Parse(TestPages.EncoderGuidelines);
		}

		[Benchmark]
		public async Task<string> RenderToHtml()
		{
			using var writer = new StringWriter();
			await Util.RenderHtmlAsync(TestPages.EncoderGuidelines, writer, _writerHelper);
			return writer.ToString();
		}

		[Benchmark]
		public async Task<string> RenderToText()
		{
			using var writer = new StringWriter();
			await Util.RenderTextAsync(TestPages.EncoderGuidelines, writer, _writerHelper);
			return writer.ToString();
		}
	}
}
