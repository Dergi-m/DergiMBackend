using AutoMapper;
using DergiMBackend.DbContext;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Models;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace DergiMBackend.Services
{
	public class UserService : IUserService
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IMapper _mapper;
		private string secretKey;
		private string issuer;
		private string audience;

		public UserService(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, IMapper mapper)
		{
			_db = db;
			_userManager = userManager;
			_roleManager = roleManager;
			secretKey = configuration.GetValue<string>("ApiSettings:Secret");
			issuer = configuration.GetValue<string>("ApiSettings:Issuer");
			audience = configuration.GetValue<string>("ApiSettings:Audience");
			_mapper = mapper;
		}

		public bool IsUniqueUser(string username)
		{
			var user = _db.Users.FirstOrDefault(u => u.UserName == username);
			if (user == null)
			{
				return true;
			}
			return false;
		}

		public async Task<TokenDto> Login(LoginRequestDto loginRequestDto)
		{
			var user = _db.Users.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDto.UserName.ToLower());

			bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);

			if (user == null || !isValid)
			{
				return new TokenDto
				{
					AccessToken = "",
				};
			}

			var tokenid = $"JWI{Guid.NewGuid()}";
			var accessToken = await GetAcessTokenAsync(user, tokenid);


			TokenDto tokendtoDto = new TokenDto()
			{
				AccessToken = accessToken,
				User = _mapper.Map<UserDto>(user),
			};
			return tokendtoDto;
		}

		private async Task<string> GetAcessTokenAsync(ApplicationUser applicationUser, string tokenid)
		{
			var roles = await _userManager.GetRolesAsync(applicationUser);
			var tokenhandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes(secretKey);
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new Claim[]
				{
					new Claim(ClaimTypes.Name,applicationUser.Id.ToString()),
					new Claim(ClaimTypes.Role,roles.FirstOrDefault()),
					new Claim(JwtRegisteredClaimNames.Jti, tokenid),
					new Claim(JwtRegisteredClaimNames.Sub,applicationUser.Id)
				}),
				Expires = DateTime.UtcNow.AddHours(2),
				SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
				Issuer = issuer,
				Audience = audience
			};

			var token = tokenhandler.CreateToken(tokenDescriptor);
			return tokenhandler.WriteToken(token);
		}

		public async Task<UserDto> Register(RegistrationRequestDto registrationRequestDto)
		{
			ApplicationUser user = new()
			{
				Name = registrationRequestDto.Name,
				Email = registrationRequestDto.UserName,
				NormalizedEmail = registrationRequestDto.UserName.ToUpper(),
				UserName = registrationRequestDto.UserName,
			};
			try
			{
				var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);
				if (result.Succeeded)
				{
					if (!await _roleManager.RoleExistsAsync(SD.RoleUSER))
					{
						await _roleManager.CreateAsync(new IdentityRole(SD.RoleADMIN));
						await _roleManager.CreateAsync(new IdentityRole(SD.RoleUSER));
					}
					await _userManager.AddToRoleAsync(user, SD.RoleUSER);
					var userToReturn = _db.Users.FirstOrDefault(u => u.UserName == registrationRequestDto.UserName);
					return _mapper.Map<UserDto>(userToReturn);
				}
			}
			catch (Exception ex)
			{

			}
			return null;
		}
		public async Task<UserDto> AssignUserToRole(RegistrationRequestDto registrationRequestDto)
		{
			try
			{
				var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == registrationRequestDto.UserName);
				if (user != null)
				{
					if (!await _roleManager.RoleExistsAsync(registrationRequestDto.Role))
					{
						await _roleManager.CreateAsync(new IdentityRole(registrationRequestDto.Role));
					}

					var roles = await _userManager.GetRolesAsync(user);
					await _userManager.RemoveFromRolesAsync(user, roles);

					await _userManager.AddToRoleAsync(user, registrationRequestDto.Role);

					var userToReturn = _db.Users.FirstOrDefault(u => u.UserName == registrationRequestDto.UserName);
					return _mapper.Map<UserDto>(userToReturn);
				}
			}
			catch (Exception ex)
			{

			}
			return null;
		}

		public async Task<IEnumerable<UserDto>> GetUsersAsync(int? organisationId = null)
		{
			var users = await _db.Users.ToListAsync();
			var result = new List<UserDto>();

			if (organisationId != null)
			{
				users = users.Where(u => u.OrganisationId == organisationId.Value).ToList();
			}

			foreach (var user in users)
			{
				var userDto = _mapper.Map<UserDto>(user);
				var roles = await _userManager.GetRolesAsync(user);
				userDto.Role = roles.FirstOrDefault();
				result.Add(userDto);
			}

			return result;
		}


		public async Task<UserDto> GetUserAsync(string username)
		{
			var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == username);
			var result = _mapper.Map<UserDto>(user);
			result.Role = (await _userManager.GetRolesAsync(user))[0];
			return result;
		}

		public async Task<bool> AssignUserToOrganisation(ApplicationUser user)
		{
			try
			{
				var userFromDb = await _db.Users.FirstOrDefaultAsync(u => u.UserName == user.UserName);

				if(userFromDb.OrganisationId != null)
				{
					throw new InvalidOperationException("Already assigned to organisation");
				}

				userFromDb.OrganisationId = user.OrganisationId;

				_db.Update(userFromDb);
				await _db.SaveChangesAsync();

				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
	}
}
