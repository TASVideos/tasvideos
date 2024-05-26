namespace TASVideos.Data;

public enum SaveResult
{
	ConcurrencyFailure,
	UpdateFailure,
	Success
}

public static class SaveResultExtensions
{
	public static bool IsSuccess(this SaveResult result) => result == SaveResult.Success;
}
