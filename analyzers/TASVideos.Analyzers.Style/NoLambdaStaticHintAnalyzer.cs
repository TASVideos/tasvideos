using System.Collections.Immutable;

namespace TASVideos.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NoLambdaStaticHintAnalyzer : DiagnosticAnalyzer
{
	private static readonly DiagnosticDescriptor DiagNoLambdaStaticHint = new(
		id: "TVO1001",
		title: "Don't mark expression-bodied delegates (\"lambdas\") as `static`",
		messageFormat: "Remove `static` keyword",
		category: "Usage",
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(DiagNoLambdaStaticHint);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterSyntaxNodeAction(
			snac =>
			{
				var node = snac.Node;
				var mods = node switch
				{
					ParenthesizedLambdaExpressionSyntax ples => ples.Modifiers,
					SimpleLambdaExpressionSyntax sles => sles.Modifiers,
					_ => throw new InvalidOperationException()
				};
				var staticTkns = mods.Where(mod => mod.IsKind(SyntaxKind.StaticKeyword)).ToArray();
				if (staticTkns.Length is not 0)
				{
					snac.ReportDiagnostic(Diagnostic.Create(DiagNoLambdaStaticHint, staticTkns[0].GetLocation()));
				}
			},
			SyntaxKind.ParenthesizedLambdaExpression,
			SyntaxKind.SimpleLambdaExpression);
	}
}
