using System.ComponentModel.DataAnnotations.Schema;

namespace Artyste.Models
{
	public class BookingHasServices
	{
		public int Id { get; set; }
		public int BookingId { get; set; }
		[ForeignKey("BookingId")]
		public Booking Booking { get; set; }

		public int ServiceId { get; set; }
		[ForeignKey("ServiceId")]
		public Services Service { get; set; }
		public decimal Price { get; set; }
	}
}
