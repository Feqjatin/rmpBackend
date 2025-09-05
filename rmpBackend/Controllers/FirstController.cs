using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using rmpBackend.Models;

namespace rmpBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class firstController(AppDbContext db,IConfiguration _configuration) : ControllerBase
    {
        [HttpGet("checkServer")]
        public List<string> getDummy()
        {
            List<string> s1 = new List<string>() { "jatin", "jatin1", "jatin2" };
            User user= db.Users.First();
            s1.Add(user.Username);
            return s1;
        }
        [HttpPost("login")]
        public async Task<IActionResult> login([FromBody] LoginDto req)
        {
            User user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user == null)
            { return BadRequest("user not found");
            }
            if ( user.PasswordHash != req.Password)
            {
                return BadRequest("wrong password");
            }
            else
            {
                 var authClaims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Username), // store username only
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                  var token = new JwtSecurityToken(
                     issuer: _configuration["Jwt:Issuer"],
                     expires: DateTime.Now.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"]!)),
                     claims: authClaims,
                     signingCredentials: new SigningCredentials(
                     new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
                     SecurityAlgorithms.HmacSha256)
                     );

               return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
            }
        }
        [HttpPost("signUp")]
        public async Task<IActionResult> signUp([FromBody] NewUserDto req)
        {   
            var user1 = await db.Users.FirstOrDefaultAsync(u=>u.Username == req.Username);
            if (user1 != null)
            {
                return BadRequest("User_name is taken");
            }
            var user2 = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
            if (user2 != null)
            {
                return BadRequest("Email is taken");
            }
            var user = new User
            {
                Username = req.Username,
                Email = req.Email,
                PasswordHash = req.PasswordHash,
                Phone = req.Phone,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return Ok(user);
        }
        

    }
    
}
