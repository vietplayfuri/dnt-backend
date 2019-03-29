namespace dnt.api.Extensions
{
    using dnt.api.Security.Token;
    using dnt.core.Models.User;
    using dnt.core.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;

    public static class ModelExtensions
    {
        public static Account GenerateToken(this UserModel userModel)
        {
            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("mysite_supersecret_secretkey!8050");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, userModel.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var result = new Account {
                Email = userModel.Email,
                FullName = userModel.FullName,
                Username = userModel.Username,
                Disabled = userModel.Disabled,
                Id = userModel.Id,
                Token = tokenHandler.WriteToken(token)
            };

            return result;
        }
    }
}
