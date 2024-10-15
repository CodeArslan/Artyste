using System.ComponentModel.DataAnnotations.Schema;

namespace Artyste.Models
{
	public class Booking
	{
		public int Id { get; set; }

		public required string BookingId { get; set; }
		public required DateTime Date { get; set; }

		public required DateTime Time { get; set; }

		public string? NotesFromCustomer { get; set; }
		public string? NotesFromArtist { get; set; }

		public required string CustomerId { get; set; }

		[ForeignKey("CustomerId")]
		public virtual ApplicationUser? Customer { get; set; }
		public required string ArtistId { get; set; }

		[ForeignKey("ArtistId")]
		public virtual ApplicationUser? Artist { get; set; }
		public decimal totalPrice {  get; set; }
		public ICollection<Services> Services { get; set; }
		public ICollection<AddOns> AddOns { get; set; }
		public bool IsApproved { get; set; }
		public ICollection<BookingHasServices> BookingHasServices { get; set; } // For services
		public ICollection<BookingHasAddOns> BookingHasAddOns { get; set; }

	}
}
