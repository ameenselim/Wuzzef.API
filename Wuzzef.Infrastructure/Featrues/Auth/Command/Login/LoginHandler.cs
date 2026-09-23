using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Wuzzef.Application.DTOs.Response;
using Wuzzef.Infrastructure.Identity;

namespace Wuzzef.Infrastructure.Featrues.Auth.Command.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, ApiResponse<AuthResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public LoginHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<ApiResponse<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
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
