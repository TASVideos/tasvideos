using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TASVideos.WikiEngine.AST;

public enum NodeType
{
	Text,
	Element,
	IfModule,
	Module
}

public interface INode
{
	[JsonConverter(typeof(StringEnumConverter))]
	NodeType Type { get; }
	int CharStart { get; }
	int CharEnd { get; set; }
	INode Clone();
	Task WriteHtmlAsync(TextWriter w, WriterContext h);

	/// <summary>
	/// Similar to WriteHtmlAsync, but writes plain text stripping formatting.
	/// TODO: Modules do not run at all.
	/// </summary>
	Task WriteTextAsync(TextWriter writer, WriterContext ctx);

	/// <summary>
	/// Get the combined text content of this Node.  May not return useful values for foreign components (Modules).
	/// </summary>
	string InnerText(IWriterHelper h);

	/// <summary>
	/// Debugging output of all of the data in this node.
	/// </summary>
	void DumpContentDescriptive(TextWriter w, string padding);

	/// <summary>
	/// Clones this node for use in a TOC.  Some things like anchors are removed.
	/// </summary>
	IEnumerable<INode> CloneForToc();
}

public interface INodeWithChildren : INode
{
	List<INode> Children { get; }
}
