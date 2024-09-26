using Artyste.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Artyste.DTO;
namespace Artyste.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ServicesController : ControllerBase
	{
		private readonly ApplicationDbContext _dbcontext;

		public ServicesController(ApplicationDbContext context)
		{
			_dbcontext = context;
		}
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> ServiceDetails([FromBody] ServicesDTO serviceDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new { success = false, message = "Validation failed.", errors = ModelState });
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { success = false, message = "User is not authenticated." });
			}

				var existingServiceName = await _dbcontext.Services
	   .Where(s => s.ServiceName.Equals(serviceDto.ServiceName.ToLower()) && s.UserId == userId)
	   .FirstOrDefaultAsync();

				if (existingServiceName != null)
				{
					return Ok(new { success = false, duplicatemessage = "A service with the same name already exists." });
				}
			
			
			var service = new Services
			{
				ServiceId = serviceDto.ServiceId,
				ServiceName = serviceDto.ServiceName,
				FromPrice = serviceDto.FromPrice,
				ToPrice = serviceDto.ToPrice,
				UserId = userId
			};

			var existingService = await _dbcontext.Services
				.Where(s => s.ServiceId == service.ServiceId && s.UserId == userId)
				.FirstOrDefaultAsync();

			if (existingService != null)
			{
				existingService.ServiceName = service.ServiceName;
				existingService.FromPrice = service.FromPrice;
				existingService.ToPrice = service.ToPrice;

				_dbcontext.Services.Update(existingService);
				await _dbcontext.SaveChangesAsync();

				return Ok(new { success = true, message = "Service updated successfully." });
			}
			else
			{
				await _dbcontext.Services.AddAsync(service);
				await _dbcontext.SaveChangesAsync();

				return Ok(new { success = true, message = "Service added successfully." });
			}
		}

		[HttpGet]
		[Authorize]
		public async Task<ActionResult<IEnumerable<Services>>> GetServices()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var services= await _dbcontext.Services.Where(s => s.UserId == userId).ToListAsync();
			return Ok(new
			{
				success = true,
				data = services,
				message = "Services fetched successfully."
			});
		}
		[HttpDelete("{id}")]
		[Authorize]
		public async Task<IActionResult> DeleteService(int id)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var service = await _dbcontext.Services.FirstOrDefaultAsync(s => s.ServiceId == id && s.UserId == userId);

			if (service == null)
			{
				return NotFound(new { message = "Service not found" });
			}

			_dbcontext.Services.Remove(service);
			await _dbcontext.SaveChangesAsync();

			return NoContent(); 
		}


	}
}
