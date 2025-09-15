using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using TASVideos.Common;
using TASVideos.ForumEngine.Benchmarks;

BenchmarkRunner.Run<ForumEngineBenchmarks>();

namespace TASVideos.ForumEngine.Benchmarks
{
	[MemoryDiagnoser]
	[SimpleJob]
	public class ForumEngineBenchmarks
	{
		private readonly IWriterHelper _writerHelper = NullWriterHelper.Instance;

		[Benchmark]
		public Element ParseBbCode()
		{
			return PostParser.Parse(TestPosts.BasicFormatting, enableBbCode: true, enableHtml: false);
		}

		[Benchmark]
		public async Task<string> RenderToHtml()
		{
			var element = PostParser.Parse(TestPosts.BasicFormatting, enableBbCode: true, enableHtml: false);
			using var writer = new StringWriter();
			var htmlWriter = new HtmlWriter(writer);
			await element.WriteHtml(htmlWriter, _writerHelper);
			return writer.ToString();
		}

		[Benchmark]
		public async Task<string> RenderToMetaDescription()
		{
			var element = PostParser.Parse(TestPosts.BasicFormatting, enableBbCode: true, enableHtml: false);
			var sb = new StringBuilder();
			await element.WriteMetaDescription(sb, _writerHelper);
			return sb.ToString();
		}
	}
}
