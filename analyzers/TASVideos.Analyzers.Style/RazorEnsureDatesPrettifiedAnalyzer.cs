using System.Collections.Immutable;

using Microsoft.CodeAnalysis.Operations;

namespace TASVideos.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RazorEnsureDatesPrettifiedAnalyzer : DiagnosticAnalyzer
{
	private static readonly DiagnosticDescriptor DiagEnsureDatesPrettified = new(
		id: "TVO1003",
		title: "DateTime values should not be written out directly",
		messageFormat: "Wrap in <timezone-convert asp-for=\"@expr\" />",
		category: "Usage",
		defaultSeverity: DiagnosticSeverity.Warning,
		isEnabledByDefault: true);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
		= ImmutableArray.Create(DiagEnsureDatesPrettified);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
		context.EnableConcurrentExecution();
		context.RegisterCompilationStartAction(initContext =>
		{
			var razorWriteSym = initContext.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Mvc.Razor.RazorPageBase")
				?.GetMembers("Write").Cast<IMethodSymbol>()
				.FirstOrDefault(sym => sym.Parameters[0].Type.SpecialType is SpecialType.System_Object);
			if (razorWriteSym is null)
			{
				return;
			}

			initContext.RegisterOperationAction(
				oac =>
				{
					var callOp = (IInvocationOperation)oac.Operation;
					if (callOp.Arguments.Length is 1 && callOp.Arguments[0].Value is IConversionOperation convOp // doing this before symbol match since that's presumably slower, haven't measured
						&& SymbolEqualityComparer.Default.Equals(razorWriteSym, callOp.TargetMethod)
						&& convOp.Operand.Type?.SpecialType is SpecialType.System_DateTime)
					{
						oac.ReportDiagnostic(Diagnostic.Create(DiagEnsureDatesPrettified, convOp.Syntax.GetLocation()));
					}
				},
				OperationKind.Invocation);
		});
	}
}
