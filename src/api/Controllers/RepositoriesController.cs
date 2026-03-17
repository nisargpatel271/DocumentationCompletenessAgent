using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DocumentationCompleteness.Api.Data;
using DocumentationCompleteness.Api.Models;

namespace DocumentationCompleteness.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepositoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RepositoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Repositories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Repository>>> GetRepositories()
        {
            return await _context.Repositories.ToListAsync();
        }

        // GET: api/Repositories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Repository>> GetRepository(Guid id)
        {
            var repository = await _context.Repositories.FindAsync(id);

            if (repository == null)
            {
                return NotFound();
            }

            return repository;
        }

        // POST: api/Repositories
        [HttpPost]
        public async Task<ActionResult<Repository>> PostRepository(Repository repository)
        {
            if (repository.Id == Guid.Empty)
            {
                repository.Id = Guid.NewGuid();
            }

            // Set timestamps
            repository.CreatedAt = DateTime.UtcNow;
            repository.UpdatedAt = DateTime.UtcNow;

            _context.Repositories.Add(repository);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRepository", new { id = repository.Id }, repository);
        }
        // DELETE: api/Repositories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRepository(Guid id)
        {
            var repository = await _context.Repositories.FindAsync(id);
            if (repository == null)
            {
                return NotFound();
            }

            _context.Repositories.Remove(repository);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
