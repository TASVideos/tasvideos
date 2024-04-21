namespace TASVideos.Api.Validators;

public class GamesRequestValidator : AbstractValidator<GamesRequest>
{
	public GamesRequestValidator()
	{
		RuleFor(g => g.Systems).MaximumLength(200);
		this.ValidateApiRequest(typeof(GamesResponse));
	}
}
