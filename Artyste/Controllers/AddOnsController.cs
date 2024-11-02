using Artyste.DTO;
using Artyste.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Artyste.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AddOnsController : ControllerBase
	{
		private readonly ApplicationDbContext _dbcontext;

		public AddOnsController(ApplicationDbContext context)
		{
			_dbcontext = context;
		}
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> AddOnsDetails([FromBody] AddOnsDto addOnDto)
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

			
				var existingAddOn = await _dbcontext.AddOns
					.Where(a => a.name==addOnDto.name.ToLower() && a.UserId == userId
					&& a.Id != addOnDto.Id)
					.FirstOrDefaultAsync();

				if (existingAddOn != null)
				{
					return Ok(new { success = false, duplicatemessage = "An AddOn with the same name already exists." });
				}
			

			var addOn = new AddOns
			{
				Id = addOnDto.Id,
				name = addOnDto.name,
				price = addOnDto.price,
				UserId = userId
			};

			var existingAddOnEntity = await _dbcontext.AddOns
				.Where(a => a.Id == addOn.Id && a.UserId == userId)
				.FirstOrDefaultAsync();

			if (existingAddOnEntity != null)
			{
				existingAddOnEntity.name = addOn.name;
				existingAddOnEntity.price=addOn.price;
				_dbcontext.AddOns.Update(existingAddOnEntity);
				await _dbcontext.SaveChangesAsync();

				return Ok(new { success = true, message = "AddOn updated successfully." });
			}
			else
			{
				await _dbcontext.AddOns.AddAsync(addOn);
				await _dbcontext.SaveChangesAsync();

				return Ok(new { success = true, message = "AddOn added successfully." });
			}
		}

		[HttpGet]
		[Authorize]
		public async Task<ActionResult<IEnumerable<AddOns>>> GetAddOns()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { success = false, message = "User is not authenticated." });
			}
			var addOns = await _dbcontext.AddOns.Where(a => a.UserId == userId).ToListAsync();
			return Ok(new
			{
				success = true,
				data = addOns,
				message = "AddOns fetched successfully."
			});
		}

		[HttpDelete("{id}")]
		[Authorize]
		public async Task<IActionResult> DeleteAddOn(int id)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var addOn = await _dbcontext.AddOns.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

			if (addOn == null)
			{
				return NotFound(new { message = "AddOn not found" });
			}

			_dbcontext.AddOns.Remove(addOn);
			await _dbcontext.SaveChangesAsync();

			return NoContent();
		}
	}
}
