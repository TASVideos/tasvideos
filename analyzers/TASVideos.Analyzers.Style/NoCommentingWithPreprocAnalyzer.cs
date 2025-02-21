using System.Collections.Immutable;

namespace TASVideos.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NoCommentingWithPreprocAnalyzer : DiagnosticAnalyzer
{
	private static readonly DiagnosticDescriptor DiagNoCommentingWithPreproc = new(
		id: "TVO1000",
		title: "Do not use `#if false`",
		messageFormat: "Do not use `#if false`",
		category: "Usage",
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(DiagNoCommentingWithPreproc);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(
			snac =>
			{
				var idts = (IfDirectiveTriviaSyntax)snac.Node;
				if (idts.Condition.ToString() is "false")
				{
					snac.ReportDiagnostic(Diagnostic.Create(DiagNoCommentingWithPreproc, idts.GetLocation()));
				}
			},
			SyntaxKind.IfDirectiveTrivia);
	}
}
