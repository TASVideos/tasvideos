using System.Collections.Immutable;
using System.Text;

namespace TASVideos.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SampleAnalyzer : DiagnosticAnalyzer
{
	private static readonly DiagnosticDescriptor DiagSample = new(
		id: "TVO1000",
		title: "Sample rule: Flags all #if blocks",
		messageFormat: "this is the condition `{0}`",
		category: "Usage",
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(DiagSample);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(
			snac =>
			{
				var idts = (IfDirectiveTriviaSyntax)snac.Node;
				snac.ReportDiagnostic(Diagnostic.Create(DiagSample, idts.GetLocation(), idts.Condition.ToString()));
			},
			SyntaxKind.IfDirectiveTrivia);
	}
}
