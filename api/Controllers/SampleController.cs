using data;
using data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SampleController(
        AppDbContext _context
        )
        : ControllerBase
    {
        [HttpGet(Name = "Test")]
        public IEnumerable<Sample> Get()
        {
            return _context.Samples.AsNoTracking().ToListAsync();
        }
    }
}
