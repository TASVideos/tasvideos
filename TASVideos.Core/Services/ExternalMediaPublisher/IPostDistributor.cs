﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace TASVideos.Core.Services.ExternalMediaPublisher;

/// <summary>
/// Receives an <see cref="IPostable"/> and distributes it
/// to a specific media such as IRC, Discord, etc
/// Distributors are managed by the <seealso cref="ExternalMediaPublisher"/>
/// </summary>
public interface IPostDistributor
{
	/// <summary>
	/// Gets all of the post types that this distributor will respond to, if a type is not here it will not send messages of that type
	/// </summary>
	IEnumerable<PostType> Types { get; }

	/// <summary>
	/// Takes the given <see cref="post"/> and posts it to a
	/// single specific media
	/// </summary>
	Task Post(IPostable post);
}
