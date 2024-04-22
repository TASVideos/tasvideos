namespace TASVideos.Api.Validators;

internal class SubmissionsRequestValidator : AbstractValidator<SubmissionsRequest>
{
	public SubmissionsRequestValidator()
	{
		RuleFor(s => s.Statuses).MaximumLength(5);
		RuleFor(s => s.StartYear).Must(st => st is null or >= 2000).WithMessage("Year must be 2000 or greater");
		RuleFor(s => s.EndYear).Must(st => st is null or >= 2000).WithMessage("Year must be 2000 or greater");
		this.ValidateApiRequest(typeof(SubmissionsResponse));
	}
}
