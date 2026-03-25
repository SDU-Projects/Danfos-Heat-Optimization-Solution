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
        // I return async here because EF ToListAsync() is asynchronous.
        public async Task<IEnumerable<ProductionUnit>> Get()
        {
            return await _context.ProductionUnits.AsNoTracking().ToListAsync();
        }
    }
}
