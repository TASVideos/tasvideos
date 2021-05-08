using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TASVideos.Api.Requests;
using TASVideos.Api.Responses;
using TASVideos.Data;
using TASVideos.Data.Entity;
using TASVideos.Data.Entity.Game;

namespace TASVideos.Api.Controllers
{
	/// <summary>
	/// The games of TASVideos that can be associated with content.
	/// </summary>
	[AllowAnonymous]
	[Route("api/v1/[controller]")]
	public class GamesController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;

		/// <summary>
		/// Initializes a new instance of the <see cref="GamesController"/> class.
		/// </summary>
		public GamesController(ApplicationDbContext db, IMapper mapper)
		{
			_db = db;
			_mapper = mapper;
		}

		/// <summary>
		/// Returns a game with the given id.
		/// </summary>
		/// <response code="200">Returns a game.</response>
		/// <response code="400">The request parameters are invalid.</response>
		/// <response code="404">A publication with the given id was not found.</response>
		[HttpGet("{id}")]
		[ProducesResponseType(typeof(GamesResponse), 200)]
		public async Task<IActionResult> Get(int id)
		{
			var pub = await _mapper.ProjectTo<GamesResponse>(_db.Games)
				.SingleOrDefaultAsync(p => p.Id == id);
			if (pub == null)
			{
				return NotFound();
			}

			return Ok(pub);
		}

		/// <summary>
		/// Returns a list of available games.
		/// </summary>
		/// <response code="200">Returns the list of games.</response>
		/// /// <response code="400">The request parameters are invalid.</response>
		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<GamesResponse>), 200)]
		public async Task<IActionResult> GetAll([FromQuery]GamesRequest request)
		{
			var games = (await _mapper.ProjectTo<GamesResponse>(
				_db.Games.ForSystemCodes(request.SystemCodes))
				.SortBy(request)
				.Paginate(request)
				.ToListAsync())
				.FieldSelect(request);

			return Ok(games);
		}

		[Authorize]
		[HttpPost]
		public async Task<IActionResult> Create(GameCreateRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			if (!(await HasPermission(PermissionTo.CatalogMovies)))
			{
				return Unauthorized();
			}

			// TODO: all the logic
			return Ok();
		}

		// TODO: Move me into common code, copied from RequirePermissions
		private async Task<bool> HasPermission(PermissionTo permission)
		{
			var user = User;

			if (!IsLoggedIn(user))
			{
				return false;
			}

			var userPerms = await GetUserPermissionsById(GetUserId(user));

			return userPerms.Contains(permission);
		}

		// TODO: copied from ClaimsPrincipalExtensions
		private static bool IsLoggedIn(ClaimsPrincipal? user)
		{
			return user?.Identity?.IsAuthenticated ?? false;
		}

		private static int GetUserId(ClaimsPrincipal? user)
		{
			if (user == null || !IsLoggedIn(user))
			{
				return -1;
			}

			return int.Parse(user.Claims
				.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
		}

		// TODO: copy pasta from JwtAuthenticator and UserManager
		private async Task<IEnumerable<PermissionTo>> GetUserPermissionsById(int userId)
		{
			return await _db.Users
				.Where(u => u.Id == userId)
				.SelectMany(u => u.UserRoles)
				.SelectMany(ur => ur.Role!.RolePermission)
				.Select(rp => rp.PermissionId)
				.Distinct()
				.ToListAsync();
		}
	}
}
