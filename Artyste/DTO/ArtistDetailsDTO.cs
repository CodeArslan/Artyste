using Artyste.Models;

namespace Artyste.DTO
{
	public class ArtistDetailsDTO
	{
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string? AccountType { get; set; }
		public string? PhoneNumber { get; set; }
		public string? Email { get; set; }
		public string? Gender { get; set; }
		public string? Description { get; set; }
		public string? Id { get; set; }
		public string? UserAvatarUrl { get; set; }
		public string? Location { get; set; }
		public List<ServicesDTO> Services { get; set; }
		public List<AddOnsDto> AddOns { get; set; }
		public List<DateTime> BookedTimeSlots { get; set; }
	}
}
