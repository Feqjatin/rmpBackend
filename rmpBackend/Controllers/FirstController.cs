using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using rmpBackend.Models;
using rmpBackend.Models.DTOs;
using rmpBackend.Services.Email;
using rmpBackend.Services.Ranking;
using rmpBackend.Services.Upload;
using static System.Net.WebRequestMethods;

namespace rmpBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class firstController(AppDbContext db, IConfiguration _configuration, RankingService rankingService,ICloudinaryService _cloudinaryService,IEmailService emailService) : ControllerBase
    {
        
        [HttpPost("login")]
        public async Task<IActionResult> login([FromBody] LoginDto req)
        {
            User user = await db.Users
                 .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == req.Email);

            if (user == null)
            {
                return BadRequest("user or password not match ");
            }
            var hasher = new PasswordHasher<object>();
            var result = hasher.VerifyHashedPassword(
                null,
                user.PasswordHash,
                req.Password
            ); 
 
           if(result == PasswordVerificationResult.Success)
            {
                var authClaims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.Username),  
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

                var roleDtos = user.Roles.Select(r => new
                {
                    r.RoleId,
                    r.RoleName
                });

                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token), username = user.Username, userRoles = roleDtos });
            }
            else
            {
                return BadRequest("user or password not match");
            }
        }
        [HttpPost("signUp")]
        public async Task<IActionResult> signUp([FromBody] NewUserDto req)
        {
            var user1 = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user1 != null)
            {
                return BadRequest("User_name or Email is taken or System Password is wrong ");
            }
            var user2 = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
            if (user2 != null)
            {
                return BadRequest("User_name or Email is taken or System Password is wrong ");
            }
            if (req.SystemPassword != "PUWNAUE*(") {
                return BadRequest("User_name or Email is taken or System Password is wrong " );

            }

            var hasher = new PasswordHasher<object>();

            string hashedPassword = hasher.HashPassword(null, req.PasswordHash);

            var user = new User
            {
                Username = req.Username,
                Email = req.Email,
                PasswordHash = hashedPassword,
                Phone = req.Phone,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPost("signUpCandidate")]
        public async Task<IActionResult> signUpCandidate([FromBody] CandidateProfileCreateDto req)
        {
            var candidate = await db.Candidates.FirstOrDefaultAsync(u => u.Email == req.Email);
            if (candidate != null)
            {
                return BadRequest("Email is taken");
            }
            var hasher = new PasswordHasher<object>();

            string hashedPassword = hasher.HashPassword(null, req.PasswordHash);


            var newCandidate = new Candidate
            {
                Name = req.Name,
                Email = req.Email,
                PasswordHash= hashedPassword,
                Phone = req.Phone,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Address = req.Address,
                City = req.City,
                State = req.State,
                ZipCode = req.ZipCode,
                LinkedinUrl = req.LinkedinUrl,
                GithubUrl = req.GithubUrl,  
                PortfolioUrl = req.PortfolioUrl,
                ProfileSummary = req.ProfileSummary,
            };
            db.Candidates.Add(newCandidate);
            await db.SaveChangesAsync();
            await rankingService.UpdateForExistingCandidate(newCandidate.CandidateId);
            return Ok(newCandidate);

        }
        [HttpPost("loginCandidate")]
        public async Task<IActionResult> loginCandidate([FromBody] CandidateLoginDto req)
        {
            var candidate = await db.Candidates.FirstOrDefaultAsync(c => c.Email == req.Email);
            if (candidate== null)
            {
                return BadRequest("email or password not match");
            }
            var hasher = new PasswordHasher<object>();

            var result = hasher.VerifyHashedPassword(
                        null,
                        candidate.PasswordHash,
                        req.Password
                        );
            
            if(result == PasswordVerificationResult.Success)
            {
                var authClaims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, candidate.CandidateId.ToString()),
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

                Role[] roles = {
                                new Role { RoleId = 1000, RoleName = "candidate" }
                            };


                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token), username = candidate.Name, userRoles = roles,candidateId=candidate.CandidateId});
            }
            else
            {
                return BadRequest(candidate);
            }

        }

        [HttpGet("job-all")]
        public async Task<IActionResult> GetAllJobs()
        {
            var jobs = await db.JobOpenings
                .Include(j => j.JobSkillMaps)
                .ThenInclude(js => js.Skill)
                .Select(j => new
                {
                    j.JobId,
                    j.Title,
                    j.Description,
                    j.Location,
                    j.Status,
                    j.MinExperience,
                    j.CreatedBy,
                    j.CreatedAt,
                    j.UpdatedAt,
                    j.ClosedReason,
                    Skills = j.JobSkillMaps.Select(js => new
                    {
                        js.SkillId,
                        js.Skill.SkillName,
                        js.SkillType
                    }).ToList()
                })
                .ToListAsync();

            return Ok(jobs);
        }
        [HttpPost("sendOTP")]
        public async Task<IActionResult> SendOTP([FromBody] RecoverAccountDto req)
        {    
            bool found=false;
            if(req.IsCandidate==true)
            {
                var candidate=await db.Candidates.Where(i=>i.Email == req.Email).FirstOrDefaultAsync();
                if (candidate != null) { 
                found=true;
                }
            }
            else
            {
                var User= await db.Users.Where(i=>i.Email==req.Email).FirstOrDefaultAsync();
                if (User != null)
                {
                    found = true;
                }
            }
            var hasher = new PasswordHasher<object>();

            if (found)
            {
                string otp = RandomNumberGenerator
                                        .GetInt32(100000, 999999)
                                        .ToString();
                string otpHashed = hasher.HashPassword(null, otp);
                RecoverAccount newRecord=new RecoverAccount()
                 {

                     Email = req.Email,
                     IsCandidate = req.IsCandidate,
                     OtpHash = otpHashed,
                     CreatedAt = DateTime.UtcNow,
                     Expiry = DateTime.UtcNow.AddMinutes(10),


                };
                db.RecoverAccounts.Add(newRecord);
                 await db.SaveChangesAsync();

                await emailService.SendAsync(new EmailRequest
                {
                    EventType = EmailEventType.SendOTP,
                    ToEmails = new List<String> { req.Email},
                    Data = new()
                    {
                        ["otp"] = otp
                    }
                });

            }
            
            return Ok("opt send");
        }

        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] RecoverAccountDto req)
        {
            var record = await db.RecoverAccounts
                .Where(i => i.Email == req.Email)
                .OrderByDescending(i => i.Expiry)
                .FirstOrDefaultAsync();
            var hasher = new PasswordHasher<object>();
            var result = hasher.VerifyHashedPassword(
               null,
               record.OtpHash,
               req.Otp
           );

            if (result == PasswordVerificationResult.Success && record.Expiry > DateTime.UtcNow)
            {
                string passwordHashed = hasher.HashPassword(null, req.NewPassword);
                if (req.IsCandidate == false)
                {
                    var User = await db.Users.Where(u => u.Email == req.Email).FirstOrDefaultAsync();
                    if (User == null) { return BadRequest("no user found"); }
                    User.PasswordHash = passwordHashed;
                    await db.SaveChangesAsync();

                }
                else
                {
                    var candidate = await db.Candidates.Where(c => c.Email == req.Email).FirstOrDefaultAsync();
                    if (candidate == null) { return BadRequest("no candidate found"); }
                    candidate.PasswordHash = passwordHashed;
                    await db.SaveChangesAsync();

                }
                await db.RecoverAccounts
                     .Where(i => i.Email == req.Email)
                     .ExecuteDeleteAsync();

                await db.SaveChangesAsync();
                return Ok("password updated");

            }
            else
            {
                return BadRequest("wrong Otp");
            }
         }

     }
 }
