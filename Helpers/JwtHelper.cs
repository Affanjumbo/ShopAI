using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ShopAI.Models; 

namespace ShopAI.Helpers
{
    public static class JwtHelper
    {
        public static string GenerateToken(string userId, string role, JwtSettings jwtSettings)
        {
            var secretKey = jwtSettings.SecretKey;
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);

            // Log the key length for debugging
            Console.WriteLine($"Key Length: {keyBytes.Length}"); // Should be 32 bytes (256 bits) or 16 bytes (128 bits)

            // Ensure key length is sufficient
            if (keyBytes.Length < 16)
            {
                throw new ArgumentException("The secret key length must be at least 128 bits (16 bytes).");
            }

            var securityKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(ClaimTypes.Role, role),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var token = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
