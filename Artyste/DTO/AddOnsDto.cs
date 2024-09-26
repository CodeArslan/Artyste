using System.ComponentModel.DataAnnotations;

namespace Artyste.DTO
{
	public class AddOnsDto
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "AddOn Name is required.")]
		public required string name { get; set; }

	}

}
