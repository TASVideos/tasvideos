namespace TASVideos.Pages.Submissions;

public class SubmitPageModelBase : BasePageModel
{
	public bool CanEditSubmission(string? submitter, ICollection<string> authors)
	{
		// If the user cannot edit submissions, then they must be an author or the original submitter
		if (User.Has(PermissionTo.EditSubmissions))
		{
			return true;
		}

		var user = User.Name();
		var isAuthorOrSubmitter = !string.IsNullOrEmpty(user)
			&& (submitter == user || authors.Contains(user));

		return isAuthorOrSubmitter && User.Has(PermissionTo.SubmitMovies);
	}
}
