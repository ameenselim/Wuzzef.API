using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Wuzzef.Application.DTOs.Request;
using Wuzzef.Application.DTOs.Response;
using Wuzzef.Application.Interfaces.IServices;
using Wuzzef.Infrastructure.Identity;

namespace Wuzzef.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<ApiResponse<string>> RegisterAsync(RegisterRequest request)
        {
            if (request == null)
            {
                return ApiResponse<string>.FailureResult("Request cannot be null.");
            }

            var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingUserByEmail != null)
            {
                return ApiResponse<string>.FailureResult("Email is already registered.", new List<string> { "A user with this email address already exists." });
            }

            var existingUserByName = await _userManager.FindByNameAsync(request.UserName);
            if (existingUserByName != null)
            {
                return ApiResponse<string>.FailureResult("Username is already taken.", new List<string> { "A user with this username already exists." });
            }

            var user = new ApplicationUser
            {
                FullName = request.FullName,
                UserName = request.UserName,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<string>.FailureResult("User registration failed.", errors);
            }

            return ApiResponse<string>.SuccessResult("User registered successfully.", "Registration completed successfully.");
        }

        public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
        {
            if (request == null)
            {
                return ApiResponse<AuthResponse>.FailureResult("Request cannot be null.");
            }

            var user = await _userManager.FindByEmailAsync(request.Email)
                       ?? await _userManager.FindByNameAsync(request.Email);

            if (user == null)
            {
                return ApiResponse<AuthResponse>.FailureResult("Invalid email or password.");
            }

            var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

            if (signInResult.IsLockedOut)
            {
                return ApiResponse<AuthResponse>.FailureResult("Account is locked out. Please try again later.");
            }

            if (signInResult.IsNotAllowed)
            {
                return ApiResponse<AuthResponse>.FailureResult("User sign-in is not allowed.");
            }

            if (!signInResult.Succeeded)
            {
                return ApiResponse<AuthResponse>.FailureResult("Invalid email or password.");
            }

            var (tokenString, expiresOn) = await GenerateJwtTokenAsync(user);

            var authResponse = new AuthResponse
            {
                Id = user.Id,
                FullName = user.FullName ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Token = tokenString,
                ExpiresOn = expiresOn
            };

            return ApiResponse<AuthResponse>.SuccessResult(authResponse, "Login successful.");
        }

        private async Task<(string Token, DateTime ExpiresOn)> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Secret Key 'Jwt:Key' is not configured.");
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "WuzzefAPI";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "WuzzefClient";
            var durationInMinutes = int.TryParse(_configuration["Jwt:DurationInMinutes"], out var minutes) ? minutes : 60;

            var expiresOn = DateTime.UtcNow.AddMinutes(durationInMinutes);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (!string.IsNullOrWhiteSpace(user.FullName))
            {
                claims.Add(new Claim("fullName", user.FullName));
            }

            if (_userManager.SupportsUserRole)
            {
                var roles = await _userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresOn,
                Issuer = jwtIssuer,
                Audience = jwtAudience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return (tokenString, expiresOn);
        }
    }
}
