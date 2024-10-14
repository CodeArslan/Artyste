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
	public class BookingController : ControllerBase
	{
		private readonly ApplicationDbContext _dbcontext;


		public BookingController(ApplicationDbContext context)
		{
			_dbcontext = context;
		}
		[HttpPost]
		public async Task<IActionResult> CreateBooking([FromBody] BookingDTO bookingDto)
		{
			if (bookingDto == null)
			{
				return BadRequest("Booking data is required.");
			}
			var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(customerId))
			{
				return Unauthorized(new { success = false, message = "User is not authenticated." });
			}
			var lastBooking = await _dbcontext.Bookings.OrderByDescending(b => b.BookingId).FirstOrDefaultAsync();
			int newBookingId;

			if (lastBooking != null)
			{
				newBookingId = int.Parse(lastBooking.BookingId) + 1;
			}
			else
			{
				newBookingId = 1;
			}
			var booking = new Booking
			{
				BookingId = newBookingId.ToString("D4"),
				Date = bookingDto.Date,
				Time = bookingDto.Time,
				Notes = bookingDto.Notes,
				CustomerId = customerId,
				ArtistId = bookingDto.ArtistId,
				IsApproved = false
			};

			// Add Services
			if (bookingDto.ServiceId != null && bookingDto.ServiceId.Count > 0)
			{
				foreach (var serviceId in bookingDto.ServiceId)
				{
					var service = new BookingHasServices
					{
						ServiceId = serviceId,
						Booking = booking,
						Price = 0
					};
					_dbcontext.BookingHasServices.Add(service);
				}
			}

			if (bookingDto.AddOnId != null && bookingDto.AddOnId.Count > 0)
			{
				foreach (var addOnId in bookingDto.AddOnId)
				{
					var addOn = new BookingHasAddOns
					{
						AddOnId = addOnId,
						Booking = booking,
						Price = 0
					};
					_dbcontext.BookingHasAddOns.Add(addOn);
				}
			}

			_dbcontext.Bookings.Add(booking);
			await _dbcontext.SaveChangesAsync();

			return Ok(new { success = true, message = "Booking made successfully." });
		}
		[HttpGet("check")]
		public async Task<IActionResult> CheckBookedTimes([FromQuery] string artistId, [FromQuery] DateTime date)
		{
			var bookedTimes = await _dbcontext.Bookings
				.Where(b => b.ArtistId == artistId && b.Date.Date == date.Date)
				.Select(b => b.Time.TimeOfDay)
				.ToListAsync();

			var bookedTimesFormatted = bookedTimes.Select(t => t.ToString(@"hh\:mm")).ToList();

			return Ok(new { bookedTimes = bookedTimesFormatted });
		}
		[HttpGet("GetRequests")]
		[Authorize]
		public async Task<ActionResult<List<object>>> GetRequests()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { success = false, message = "User is not authenticated." });
			}

			var bookings = await _dbcontext.Bookings
	   .Include(b => b.Customer)
	   .Include(b => b.Artist)
	   .Where(b => b.ArtistId == userId && !b.IsApproved)
	   .Select(b => new
	   {
		   BookingId = b.BookingId,
		   Name = b.Customer.FirstName + " " + b.Customer.LastName,
		   Email = b.Customer.Email,
		   Date = b.Date.ToString("yyyy-MM-dd"),
		   Location = b.Customer.location,
		   Time = b.Time.ToString("HH:mm"),
		   IsApproved = b.IsApproved,
		   PicUrl = b.Customer.userAvatarUrl
	   })
	   .ToListAsync();

			return Ok(bookings);
		}

		[HttpGet("GetBookingById/{bookingId}")]
		[Authorize]
		public async Task<ActionResult<List<object>>> GetBookingById(string bookingId)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized(new { success = false, message = "User is not authenticated." });
			}
			var booking = await _dbcontext.Bookings
		  .Include(b => b.Customer)
		  .Include(b => b.Artist)
		  .Include(b => b.BookingHasServices)  
		  .Include(b => b.BookingHasAddOns)    
		  .Where(b => b.BookingId == bookingId)
		  .Select(b => new
		  {
			  BookingId = b.BookingId,
			  CustomerName = b.Customer.FirstName + " " + b.Customer.LastName,
			  CustomerId = b.Customer.Id,
			  Email = b.Customer.Email,
			  Date = b.Date.ToString("yyyy-MM-dd"),
			  Time = b.Time.ToString("HH:mm"),
			  CustomerProfileUrl = b.Customer.userAvatarUrl,
			  Location = b.Artist.location,
			  Notes = b.Notes,
			  Services = b.BookingHasServices.Select(bs => new
			  {
				  ServiceId = bs.ServiceId,
				  ServiceName = bs.Service.ServiceName,  
			  }).ToList(),
			  AddOns = b.BookingHasAddOns.Select(ba => new
			  {
				  AddOnId = ba.AddOnId,
				  AddOnName = ba.AddOn.name,  
			  }).ToList()
		  })
		  .FirstOrDefaultAsync();

			if (booking == null)
			{
				return NotFound(new { success = false, message = "Booking not found." });
			}

			return Ok(new { success = true, data = booking });
		}
	
}

	
	// GET: api/bookings/{id}
	//[HttpGet("{id}")]
	//public async Task<ActionResult<BookingDTO>> GetBookingById(int id)
	//{
	//	var booking = await _dbcontext.Bookings.FindAsync(id);
	//	if (booking == null)
	//	{
	//		return NotFound();
	//	}

	//	// Create the BookingDTO to return
	//	var bookingDto = new BookingDTO
	//	{
	//		Id = booking.Id,
	//		Date = booking.Date,
	//		Time = booking.Time,
	//		Notes = booking.Notes,
	//		ArtistId = booking.ArtistId,
	//		// Populate Services and AddOns if needed
	//		Services = _dbcontext.BookingHasServices
	//			.Where(bs => bs.BookingId == booking.Id)
	//			.Select(bs => new ServicesDTO
	//			{
	//				ServiceId = bs.ServiceId,
	//				ServiceName = _dbcontext.Services.Find(bs.ServiceId).ServiceName,
	//				ToPrice = _dbcontext.Services.Find(bs.ServiceId).ToPrice
	//			}).ToList(),
	//		AddOns = _dbcontext.BookingHasAddOns
	//			.Where(ba => ba.BookingId == booking.Id)
	//			.Select(ba => new AddOnsDto
	//			{
	//				Id = ba.AddOnId,
	//				name = _dbcontext.AddOns.Find(ba.AddOnId).name, 
	//			}).ToList()
	//	}; 

	//	return bookingDto;
	//}
}

