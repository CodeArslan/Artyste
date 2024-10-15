namespace Artyste.DTO
{
	public class BookingConfirmationDTO
	{
		public string bookingId { get; set; }
		public string? notesFromArtist { get; set; }	
		public decimal? totalPrice { get; set; }
		public List<AddOn> addOns { get; set; } = new List<AddOn>(); // Note: initialize in constructor
		public List<BookingService> services { get; set; } = new List<BookingService>(); // Note: initialize in constructor
	}

	public class AddOn
	{
		public int id { get; set; }
		public decimal price { get; set; }
	}

	public class BookingService
	{
		public int id { get; set; }
		public decimal price { get; set; }
	}


}
