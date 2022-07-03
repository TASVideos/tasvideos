using Refit;

namespace TASVideos.Api.Interfaces;

/// <summary>
/// Representation of the UsArchiveAPI.
/// </summary>
[Headers(
		"x-archive-ignore-preexisting-bucket:1",
		"authorization: LOW $accesskey:$secret",
		"x-archive-meta-mediatype: movies",
		"x-archive-meta-collection: opensource_movies",
		"x-archive-meta-creator: TASVideos",
		"x-archive-meta-licenseurl:http://creativecommons.org/licenses/by-nc/3.0/us/'")]
public interface IUsArchiveAPI
{
	/// <summary>
	/// Uploads Video to US Archives
	/// </summary>
	/// <param name="video">the video to be uploaded</param>
	/// <returns>Response representing the newly uploaded video</returns>
	[Multipart]
	[Post("UploadAsync")]
	Task<UsArchiveResponse> UploadAsync([Body] VideoUpload video);
}
