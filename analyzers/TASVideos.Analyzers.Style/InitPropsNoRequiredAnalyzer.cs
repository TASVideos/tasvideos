using System.Collections.Immutable;

namespace TASVideos.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InitPropsNoRequiredAnalyzer : DiagnosticAnalyzer
{
	private static readonly DiagnosticDescriptor DiagInitPropsNoRequired = new(
		id: "TVO1002",
		title: "Properties with an `init` accessor should not be `required`",
		messageFormat: "Remove `required` keyword",
		category: "Usage",
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(DiagInitPropsNoRequired);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(
			snac =>
			{
				var pds = (PropertyDeclarationSyntax)snac.Node;
				var requiredTkns = pds.Modifiers.Where(mod => mod.IsKind(SyntaxKind.RequiredKeyword)).ToArray();
				if (requiredTkns.Length is not 0)
				{
					snac.ReportDiagnostic(Diagnostic.Create(DiagInitPropsNoRequired, requiredTkns[0].GetLocation()));
				}
			},
			SyntaxKind.PropertyDeclaration);
	}
}
