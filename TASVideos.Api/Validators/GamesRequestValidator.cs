namespace TASVideos.Api.Validators;

internal class GamesRequestValidator : AbstractValidator<GamesRequest>
{
	public GamesRequestValidator()
	{
		RuleFor(g => g.Systems).MaximumLength(200);
		this.ValidateApiRequest(typeof(GamesResponse));
	}
}
