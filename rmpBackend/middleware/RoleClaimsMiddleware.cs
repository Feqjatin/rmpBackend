using Microsoft.EntityFrameworkCore;
using rmpBackend.Models;
using System.Security.Claims;

namespace rmpBackend.middleware
{
    public class RoleClaimsMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleClaimsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext db)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {

                var username = context.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")
                    ?.Value;

                if (!string.IsNullOrEmpty(username))
                {

                    var user = await db.Users
                        .Include(u => u.Roles)
                        .FirstOrDefaultAsync(u => u.Username == username);

                    if (user != null)
                    {

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.Username)
                        };

                        claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r.RoleName)));

                        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt-refresh"));
                    }
                }
            }
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var id = context.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")
                    ?.Value;
                if (!string.IsNullOrEmpty(id))
                {
                    var id2 = int.Parse(id);
                    var candidate = db.Candidates.Where(c => c.CandidateId == id2).FirstOrDefault();
                    if (candidate != null)
                    {

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Role, "candidate")
                        };

                        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt-refresh"));


                    }
                    
                    
                }
            }


                await _next(context);
        }
    }
}
