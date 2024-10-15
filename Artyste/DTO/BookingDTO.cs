namespace Artyste.DTO
{
	public class BookingDTO
	{
		public int Id { get; set; }
		public DateTime Date { get; set; }
		public DateTime Time { get; set; }
		public string? NotesFromCustomer { get; set; }
		public string? NotesFromArtist { get; set; }
		public required string ArtistId { get; set; }

		public List<int> ServiceId { get; set; } = new List<int>();

		public List<int> AddOnId { get; set; } = new List<int>();




	}
}
