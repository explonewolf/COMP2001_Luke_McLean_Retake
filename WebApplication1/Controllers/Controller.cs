using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModerationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ModerationsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/Moderations
        [HttpPost]
        public async Task<ActionResult<Moderation>> PostModeration(Moderation moderation)
        {
            _context.Moderations.Add(moderation);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetModeration", new { id = moderation.ModerationID }, moderation);
        }

        // GET: api/Moderations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Moderation>> GetModeration(int id)
        {
            var moderation = await _context.Moderations.Include(m => m.User).Include(m => m.Action)
                                    .FirstOrDefaultAsync(m => m.ModerationID == id);

            if (moderation == null)
            {
                return NotFound();
            }

            return moderation;
        }

        // PUT: api/Moderations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutModeration(int id, Moderation moderation)
        {
            if (id != moderation.ModerationID)
            {
                return BadRequest();
            }

            _context.Entry(moderation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ModerationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Moderations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModeration(int id)
        {
            var moderation = await _context.Moderations.FindAsync(id);
            if (moderation == null)
            {
                return NotFound();
            }

            _context.Moderations.Remove(moderation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ModerationExists(int id)
        {
            return _context.Moderations.Any(e => e.ModerationID == id);
        }
    }
}
