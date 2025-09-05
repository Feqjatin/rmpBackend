using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rmpBackend.Models;

namespace rmpBackend.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController(AppDbContext db) : ControllerBase
    {
       
        [HttpPost("assign-role")]
        public async Task<IActionResult> assignRole([FromBody] AssignRoleDto req)
        {
            var user = await db.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.UserId == req.UserId);

            var role = await db.Roles.FindAsync(req.RoleId);

            if (user != null && role != null)
            {
                if (!user.Roles.Any(r => r.RoleId == req.RoleId))
                {
                    user.Roles.Add(role);
                    await db.SaveChangesAsync();
                    return Ok("Role assigned successfully!");
                }
                else
                {
                    return Ok("User already has this role.");
                }
            }

            return BadRequest("User or Role not found.");

        }
        [HttpPost("addNewRole")]
        public async Task<IActionResult> addNewRole([FromBody] NewRoleDto req)
        {
            if(req.RoleName==null||req.Description==null)
            {
                return BadRequest("null parameter found");
            }

            var role = new Role
                {
                RoleName = req.RoleName,
                Description = req.Description,
            };
            Console.WriteLine(role);
            var a = await db.Roles.AddAsync(role);
            await db.SaveChangesAsync();
            return Ok(role);
        }

        [HttpDelete("removeRole")]
        public async Task<IActionResult> removeRole([FromBody] removeRoleDto req)
        {   
             
            Role role = await db.Roles.FirstAsync(u => u.RoleId == req.RoleId);
            if (role == null)
            {
                return BadRequest("role not found");
            }
            else
            {
                db.Roles.Remove(role);
                await db.SaveChangesAsync();
            }
                return Ok("removed role ");
        }
        [HttpDelete("removeUser")]
        public async Task<IActionResult> removeUser([FromBody] removeUserDto req)
        {
          
            User user = await db.Users.FirstAsync(u => u.UserId == req.UserId);
            if (user == null)
            {
                return BadRequest("user not found");
            }
            else
            {   
                db.Users.Remove(user);
                await db.SaveChangesAsync();
            }
            return Ok("removed user");
        }
        [HttpDelete("dischargeUserToRole")]
        public async Task<IActionResult> dischargeUserToRole([FromBody] AssignRoleDto req)
        {
            var user = await db.Users
                               .Include(u => u.Roles)
                               .FirstOrDefaultAsync(u => u.UserId == req.UserId);

            var role = await db.Roles.FindAsync(req.RoleId);

            if (user != null && role != null)
            {
                var userRole = user.Roles.FirstOrDefault(r => r.RoleId == req.RoleId);
                if (userRole != null)
                {
                    user.Roles.Remove(userRole);
                    await db.SaveChangesAsync();
                    return Ok("Role removed successfully!");
                }
                else
                {
                    return Ok("User does not have this role.");
                }
            }

            return BadRequest("User or Role not found.");
        }
    }
}
