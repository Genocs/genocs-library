﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Genocs.Core.Demo.Infrastructure.SecurityAuthentication;

public sealed class TokenProvider(IConfiguration configuration)
{
    public string Issuer => configuration["JwtSettings:Issuer"];
    public string Audience => configuration["JwtSettings:Audience"];
    public string Key => configuration["JwtSettings:Key"];
    public int Expiration => int.Parse(configuration["JwtSettings:Expiration"]);

    /// <summary>
    /// This method creates a token using the JwtSecurityTokenHandler class.
    /// </summary>
    /// <returns></returns>
    public string CreateToken()
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = Encoding.ASCII.GetBytes(Key);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.Role, "role")
            }),
            Expires = DateTime.UtcNow.AddMinutes(Expiration),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// This method creates a token using the JsonWebTokenHandler class.
    /// </summary>
    /// <returns></returns>
    public string CreateTokenWithJsonWebTokenHandler()
    {
        var tokenHandler = new JsonWebTokenHandler();
        byte[] key = Encoding.ASCII.GetBytes(Key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.Role, "role")
            }),
            Expires = DateTime.UtcNow.AddMinutes(Expiration),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        string token = tokenHandler.CreateToken(tokenDescriptor);
        return token;
    }
}
