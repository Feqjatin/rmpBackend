using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rmpBackend.Models;

namespace rmpBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FirstController : ControllerBase
    {
        [HttpGet]
        public List<string> getDummy()
        {
            List<string> s1 = new List<string>() { "jatin", "jatin1", "jatin2" };
            return s1;
        }
        [HttpGet("{2}")]
        public List<string> getDummy2()
        {
            List<string> s1 = new List<string>() { "use2" };
            return s1;
        }
    }
    [Route("api/[controller]")]
    [ApiController]
    public class dbController : ControllerBase
    {
        private readonly jatinYYContext db;

        public dbController(jatinYYContext db)
        {
            this.db = db;
        }
        [HttpGet]
        public async Task<ActionResult<List<Person>>> read()
        {
            var data = await db.People.ToListAsync();
            return Ok(data);
        }
    }
}
