using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParamonovAPI.Models;
using ParamonovAPI.Services;
using System.Security.Claims;

namespace ParamonovAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IService<Project> _service;
        private readonly PostgresContext _context;

        public ProjectController(IService<Project> service, PostgresContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetAll()
            => Ok(await _service.GetAll());

        [HttpGet("my/{userId}")]
        public async Task<ActionResult<IEnumerable<Project>>> GetMyProjects(int userId)
        {

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            IQueryable<Project> query = _context.Projects;

            if (user.Role == "customer")
            {
                query = query.Where(p => p.IdCustomer == userId);
            }
            else
            {
                if (user.IdTeam == null) return Ok(new List<Project>());
                int teamId = user.IdTeam.Value;
                query = query.Where(p => p.IdTeam == teamId);
            }

            return Ok(await query.AsNoTracking().ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetById(int id)
        {
            var item = await _service.GetById(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Project item)
        {
            await _service.Create(item);
            return CreatedAtAction(nameof(GetById), new { id = item.IdProject }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Project item)
        {
            if (id != item.IdProject) return BadRequest();
            var existing = await _service.GetById(id);
            if (existing == null) return NotFound();
            await _service.Update(item);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _service.GetById(id);
            if (existing == null) return NotFound();
            await _service.Delete(id);
            return NoContent();
        }
    }
}
