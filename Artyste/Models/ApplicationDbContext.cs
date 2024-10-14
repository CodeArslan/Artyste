using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Artyste.Models
{
	public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
	   : base(options)
		{
		}

		public DbSet<Services> Services { get; set; }
		public DbSet<AddOns> AddOns { get; set; }
		public DbSet<Booking> Bookings { get; set; }
		public DbSet<BookingHasAddOns> BookingHasAddOns { get; set; }
		public DbSet<BookingHasServices> BookingHasServices { get; set; }
	}
}
