using System.ComponentModel.DataAnnotations.Schema;

namespace Artyste.Models
{
	public class BookingHasAddOns
	{
		public int Id { get; set; }
		public int BookingId { get; set; }
		[ForeignKey("BookingId")]
		public Booking Booking { get; set; }

		public int AddOnId { get; set; }
		[ForeignKey("AddOnId")]
		public AddOns AddOn { get; set; }

		public decimal Price { get; set; }
	}
}
