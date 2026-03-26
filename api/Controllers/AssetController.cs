using data;
using data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetController(AppDbContext context) : ControllerBase
    {
        private readonly AppDbContext _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductionUnit>>> GetAllAssets()
        {
            var assets = await _context.ProductionUnits
                .AsNoTracking()
                .ToListAsync();

            return Ok(assets);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductionUnit>> GetAssetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Asset ID must be a positive integer.");
            }

            var asset = await _context.ProductionUnits
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asset == null)
            {
                return NotFound($"Asset with ID {id} not found.");
            }

            return Ok(asset);
        }

        [HttpPost]
        public async Task<ActionResult<ProductionUnit>> CreateAsset([FromBody] ProductionUnit asset)
        {
            if (asset == null)
            {
                return BadRequest("Asset data is required.");
            }

            if (asset.Data == null)
            {
                return BadRequest("Asset data properties are required.");
            }

            if (string.IsNullOrWhiteSpace(asset.Data.Name))
            {
                return BadRequest("Asset name is required.");
            }

            if (string.IsNullOrWhiteSpace(asset.Data.ImageUrl))
            {
                return BadRequest("Asset image URL is required.");
            }

            _context.ProductionUnits.Add(asset);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAssetById), new { id = asset.Id }, asset);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsset(int id, [FromBody] ProductionUnit asset)
        {
            if (id <= 0)
            {
                return BadRequest("Asset ID must be a positive integer.");
            }

            if (asset == null)
            {
                return BadRequest("Asset data is required.");
            }

            if (asset.Data == null)
            {
                return BadRequest("Asset data properties are required.");
            }

            if (string.IsNullOrWhiteSpace(asset.Data.Name))
            {
                return BadRequest("Asset name is required.");
            }

            if (string.IsNullOrWhiteSpace(asset.Data.ImageUrl))
            {
                return BadRequest("Asset image URL is required.");
            }

            var existingAsset = await _context.ProductionUnits.FindAsync(id);

            if (existingAsset == null)
            {
                return NotFound($"Asset with ID {id} not found.");
            }

            existingAsset.Data = asset.Data;
            existingAsset.Type = asset.Type;

            _context.ProductionUnits.Update(existingAsset);
            await _context.SaveChangesAsync();

            return Ok(existingAsset);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Asset ID must be a positive integer.");
            }

            var asset = await _context.ProductionUnits.FindAsync(id);

            if (asset == null)
            {
                return NotFound($"Asset with ID {id} not found.");
            }

            _context.ProductionUnits.Remove(asset);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}