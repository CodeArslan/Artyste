using System.ComponentModel.DataAnnotations;

namespace Artyste.DTO
{
	public class ServicesDTO
	{
		public int ServiceId { get; set; }
		[Required(ErrorMessage = "Service Name is required.")]
		public string ServiceName { get; set; }

		[Required(ErrorMessage = "From Price is required.")]
		public decimal FromPrice { get; set; }

		[Required(ErrorMessage = "To Price is required.")]
		public decimal ToPrice { get; set; }
	}
}
