namespace TASVideos.Api.Validators;
public class GamesRequestValidator : AbstractValidator<GamesRequest>
{
	public GamesRequestValidator()
	{
		RuleFor(g => g.Systems).MaximumLength(200);

		// TODO: how to use ApiRequestValidator so these do not have to be copy/pasted?
		RuleFor(a => a.PageSize).Must(ps => ps is null or >= 1 and <= ApiConstants.MaxPageSize).WithMessage("Invalid page size");
		RuleFor(a => a.CurrentPage).Must(c => c is null or >= 1).WithMessage("Invalid current page");
		RuleFor(a => a.Sort).MaximumLength(200);
		RuleFor(a => a.Fields).MaximumLength(200);
	}
}
