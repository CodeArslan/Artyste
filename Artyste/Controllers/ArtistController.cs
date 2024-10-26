using Artyste.DTO;
using Artyste.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Artyste.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ArtistController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationDbContext _dbcontext;

	
		public ArtistController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
		{
			_userManager = userManager;
			_dbcontext = context;
		}
		[HttpGet]
		[AllowAnonymous]
		public async Task<ActionResult<dynamic>> GetArtists()
		{
			try
			{
				// Get the list of users with the "artist" role
				var roleName = "artist";
				var artists = new List<dynamic>();

				// Get all users
				var users = await _userManager.Users.ToListAsync();

				foreach (var user in users)
				{
					// Check if the user is in the "artist" role
					if (await _userManager.IsInRoleAsync(user, roleName))
					{
						artists.Add(new
						{
							user.FirstName,
							user.LastName,
							user.AccountType,
							user.PhoneNumber,
							user.Email,
							user.Gender,
							user.Description,
							user.Id,
							user.userAvatarUrl
						});
					}
				}

				// Check if there are any artists found
				if (artists == null || !artists.Any())
				{
					return NotFound();
				}

				return Ok(artists);
			}
			catch (Exception ex)
			{
				// Log the error details (you can use ILogger here)
				var errorMessage = $"An error occurred in GetArtists: {ex.Message}";

				// You can log the error to the console or use a logging framework
				Console.WriteLine(errorMessage);

				// Optionally, log the stack trace for more details
				Console.WriteLine(ex.StackTrace);

				// Return a generic error message
				return StatusCode(500, new { message = "An error occurred while processing your request.", error = ex.Message });
			}
		}


		[HttpGet("{id}")]
		public async Task<ActionResult<ArtistDetailsDTO>> GetArtistById(string id)
		{
			
			var user=_dbcontext.Users.Where(u=>u.Id == id).FirstOrDefault();
			if (user == null)
			{
				return NotFound();
			}

			var roleName = "artist";
			if (!await _userManager.IsInRoleAsync(user, roleName))
			{
				return Forbid();
			}

			var services = await _dbcontext.Services.Where(s => s.UserId == id).ToListAsync();
			var addOns = await _dbcontext.AddOns.Where(a => a.UserId == id).ToListAsync();
			var currentDate = DateTime.Now.Date;
			var bookings = await _dbcontext.Bookings
	   .Where(b => b.ArtistId == id && b.Date == currentDate)
	   .Select(b => b.Time)  
	   .ToListAsync();

			// Create the artist details object
			var artistDetails = new ArtistDetailsDTO
			{
				FirstName = user.FirstName,
				LastName = user.LastName,
				AccountType = user.AccountType,
				PhoneNumber = user.PhoneNumber,
				Email = user.Email,
				Gender = user.Gender,
				Description = user.Description,
				Id = user.Id,
				Location=user.location,
				UserAvatarUrl = user.userAvatarUrl,
				Services = services.Select(s => new ServicesDTO
				{
					ServiceName = s.ServiceName,
					ServiceId = s.ServiceId,
					FromPrice = s.FromPrice,
					ToPrice = s.ToPrice
				}).ToList(),
				AddOns = addOns.Select(a => new AddOnsDto
				{
					name = a.name,
					Id = a.Id
				}).ToList(),
				BookedTimeSlots = bookings
			};

			return Ok(artistDetails);
		}

	}
}
