using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.IdentityModel;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Authentication.Models;

namespace Authentication.Controllers
{
    [Route("auth/[controller]")]
    public class UsersController : Controller
    {
        private readonly IDictionary<string, User> _users = new Dictionary<string, User> {
            { "test1", new User { Id = "test1", FirstName = "Test", LastName = "User", Name = "Test User", Title = "Tester", Phone = "x1234", Email = "test1@example.com", Employee = true } },
        };

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_users.Values);
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
                return BadRequest();

            User user;
            if (!_users.TryGetValue(id, out user))
                return NotFound();
            return Ok(user);
        }

        [HttpGet("{id}/token")]
        public IActionResult GetToken(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
                return BadRequest();

            User user;
            if (!_users.TryGetValue(id, out user))
                return NotFound();
            const string secret = "Test Authentication Security Key";
            var credentials = new SigningCredentials(new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret)), SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(ClaimTypes.Name, user.Id), // This is annoying
                new Claim("full_name", user.Name),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            };
            if (user.Employee)
            {
                claims.Add(new Claim("is_employee", "true"));
                claims.Add(new Claim("title", user.Title));
                claims.Add(new Claim("extension", user.Phone));
            }
            var expires = DateTime.UtcNow.AddHours(1);
            var jwt = new JwtSecurityToken("Test", "Test", claims, null, expires, credentials);

            var encoded = new JwtSecurityTokenHandler().WriteToken(jwt);

            return Ok(new { token = encoded, expires });
        }
    }
}
