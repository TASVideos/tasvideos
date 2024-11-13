using System.Diagnostics.CodeAnalysis;

namespace TASVideos.Api.Validators;

internal class GamesRequestValidator : AbstractValidator<GamesRequest>
{
	[RequiresUnreferencedCode(nameof(ApiRequestValidatorExtensions.ValidateApiRequest))]
	public GamesRequestValidator()
	{
		RuleFor(g => g.Systems).MaximumLength(200);
		this.ValidateApiRequest(typeof(GamesResponse));
	}
}
