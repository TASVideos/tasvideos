﻿//namespace TASVideos.Api.Controllers;

///// <summary>
///// The publications of TASVideos.
///// </summary>
//[AllowAnonymous]
//[Route("api/v1/[controller]")]
//public class PublicationsController(ApplicationDbContext db) : Controller
//{
//	// /// <summary>
//	// /// Returns a publication with the given id.
//	// /// </summary>
//	// /// <response code="200">Returns a publication.</response>
//	// /// <response code="400">The request parameters are invalid.</response>
//	// /// <response code="404">A publication with the given id was not found.</response>
//	// [HttpGet("{id}")]
//	// [ProducesResponseType(typeof(PublicationsResponse), 200)]
//	// public async Task<IActionResult> Get(int id)
//	// {
//	// 	var pub = await db.Publications
//	// 		.ToPublicationsResponse()
//	// 		.SingleOrDefaultAsync(p => p.Id == id);
//	//
//	// 	return pub is null
//	// 		? NotFound()
//	// 		: Ok(pub);
//	// }

//	/// <summary>
//	/// Returns a list of publications, filtered by the given criteria.
//	/// </summary>
//	/// <response code="200">Returns the list of publications.</response>
//	/// <response code="400">The request parameters are invalid.</response>
//	[HttpGet]
//	[Validate]
//	[ProducesResponseType(typeof(IEnumerable<PublicationsResponse>), 200)]
//	public async Task<IActionResult> GetAll([FromQuery] PublicationsRequest request)
//	{
//		var pubs = (await db.Publications
//			.FilterByTokens(request)
//			.ToPublicationsResponse()
//			.SortBy(request)
//			.Paginate(request)
//			.ToListAsync())
//			.FieldSelect(request);
//		return Ok(pubs);
//	}
//}
