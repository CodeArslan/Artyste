using System.ComponentModel.DataAnnotations;

namespace Artyste.DTO
{
	public class ServicesDTO
	{
		public int ServiceId { get; set; }
		[Required(ErrorMessage = "Service Name is required.")]
		public string ServiceName { get; set; }

		[Required(ErrorMessage = "Price is required.")]
		public decimal price { get; set; }

	}
}
