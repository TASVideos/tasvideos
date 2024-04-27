namespace TASVideos.Api.Validators;

internal class PublicationsRequestValidator : AbstractValidator<PublicationsRequest>
{
	public PublicationsRequestValidator()
	{
		RuleFor(p => p.StartYear).Must(st => st is null or >= 2000).WithMessage("Year must be 2000 or greater");
		RuleFor(p => p.EndYear).Must(st => st is null or >= 2000).WithMessage("Year must be 2000 or greater");
		this.ValidateApiRequest(typeof(PublicationsResponse));
	}
}
