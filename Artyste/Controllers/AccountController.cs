using Artyste.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Artyste.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly IConfiguration _configuration;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly string _uploadFolder;
		public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_configuration = configuration;
			_roleManager = roleManager;
			_uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

			if (!Directory.Exists(_uploadFolder))
			{
				Directory.CreateDirectory(_uploadFolder);
			}
		}

		[HttpPost("Login")]
		public async Task<IActionResult> Login([FromBody] Login model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			// Fetch the user by phone number
			var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.phone);

			if (user == null)
			{
				return Unauthorized("Invalid phone number.");
			}

			// Check if the password is correct
			var result = await _signInManager.CheckPasswordSignInAsync(user, model.password, lockoutOnFailure: false);

			if (result.Succeeded)
			{
				// Generate the JWT token
				var token = GenerateJwtToken(user);

				// Return the token along with user information
				return Ok(new
				{
					token,
					userData = new
					{
						id = user.Id,
						firstName = user.FirstName, 
						lastName = user.LastName  
					}
				});
			}

			return Unauthorized("Invalid password.");
		}


		private string GenerateJwtToken(IdentityUser user)
		{
			var claims = new[]
			{
		new Claim(JwtRegisteredClaimNames.Sub, user.Id),
		new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
		new Claim(ClaimTypes.Name, user.UserName)
	};

			var secretKey = _configuration["Jwt:SecretKey"];
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			// Set the expiration time to a very long duration (e.g., 100 years)
			var expirationTime = DateTime.UtcNow.AddYears(100); // Change as needed

			var token = new JwtSecurityToken(
				issuer: "artyste.versel.app",
				audience: "artyste.versel.app",
				claims: claims,
				expires: expirationTime,
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		[HttpPost("Register")]
		public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var existingUser = await _userManager.Users
				.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber || u.Email == model.Email);

			if (existingUser != null)
			{
				return BadRequest(new { errors = new[] { "User already exists." } });
			}


			var user = new ApplicationUser
			{
				PhoneNumber = model.PhoneNumber,
				Email = model.Email,
				FirstName=model.FirstName,
				LastName=model.LastName,
				AccountType = model.AccountType,
				UserName=model.FirstName
			};

			var result = await _userManager.CreateAsync(user, model.Password);

			if (result.Succeeded)
			{
				if (!await _roleManager.RoleExistsAsync("Artist"))
				{
					await _roleManager.CreateAsync(new IdentityRole("Artist"));
				}

				if (!await _roleManager.RoleExistsAsync("User"))
				{
					await _roleManager.CreateAsync(new IdentityRole("User"));
				}

				var normalizedAccountType = model.AccountType?.Trim().ToLower();

				if (normalizedAccountType == "artist")
				{
					await _userManager.AddToRoleAsync(user, "Artist");
				}
				else if (normalizedAccountType == "user")
				{
					await _userManager.AddToRoleAsync(user, "User");
				}
				return Ok("User registered successfully.");
			}

			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			return BadRequest(ModelState);
		}

		[HttpGet("me")]
		[Authorize]
		public async Task<ActionResult<dynamic>> GetUserById()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user = await _userManager.Users
	   .Where(u => u.Id == userId)
	   .Select(u => new
	   {
		   u.FirstName,
		   u.LastName,
		   u.AccountType,
		   u.PhoneNumber,
		   u.Email,
		   u.Gender,
		   u.Description,
		   u.Id,
		   u.userAvatarUrl
	   })
	   .FirstOrDefaultAsync();

			if (user == null)
			{
				return NotFound();
			}

			return Ok(user);
		}
		[HttpPut("user")]
		[Authorize]
		public async Task<IActionResult> UpdateUser([FromBody] RegisterViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound(new { errors = new[] { "User doesn't found." } });
			}

			user.FirstName = model.FirstName; 
			user.LastName = model.LastName;
			user.PhoneNumber = model.PhoneNumber;
			user.Gender = model.Gender;
			user.Email = model.Email;
			user.Description = model.Description;

			var result = await _userManager.UpdateAsync(user);
			if (result.Succeeded)
			{
				return Ok(new { errors = new[] { "Details Updated Succesfully" } }); 
			}

			return BadRequest(result.Errors);
		}
		[HttpPost("logout")]
		[Authorize]
		public async Task<ActionResult> Logout()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			await _signInManager.SignOutAsync();

			
			return NoContent();
		}

		[HttpPost("image")]
		[Authorize]
		public async Task<IActionResult> UploadImage([FromForm] IFormFile avatar)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (avatar == null || avatar.Length == 0)
			{
				return BadRequest("No file uploaded.");
			}

			var fileExtension = Path.GetExtension(avatar.FileName);

			// Create the file name using the user ID and original extension
			var fileName = $"{userId}avatar{fileExtension}";

			// Define the directory path for user avatars
			var userAvatarDirectory = Path.Combine(_uploadFolder, "userAvatar");

			// Ensure the directory exists
			if (!Directory.Exists(userAvatarDirectory))
			{
				Directory.CreateDirectory(userAvatarDirectory);
			}

			// Combine the directory and file name to create the full file path
			var filePath = Path.Combine(userAvatarDirectory, fileName);

			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await avatar.CopyToAsync(stream);
			}

			var imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/userAvatar/{fileName}";

			var user = await _userManager.FindByIdAsync(userId);
			if (user != null)
			{
				user.userAvatarUrl = imageUrl;
				var result = await _userManager.UpdateAsync(user);

				if (!result.Succeeded)
				{
					return BadRequest("Failed to update user avatar URL.");
				}
			}
			else
			{
				return NotFound("User not found.");
			}

			return Ok(new { url = imageUrl });
		}


	}
}
