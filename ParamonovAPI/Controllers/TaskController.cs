using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParamonovAPI.Services;
using TaskModel = ParamonovAPI.Models.Task;
using TaskStatus = ParamonovAPI.Models.TaskStatus;

namespace ParamonovAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly IService<TaskModel> _service;
        private readonly PostgresContext _context;

        public TaskController(IService<TaskModel> service, PostgresContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskModel>>> GetAll()
            => Ok(await _service.GetAll());

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskModel>> GetById(int id)
        {
            var item = await _service.GetById(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TaskModel item)
        {
            await _service.Create(item);
            await RecalculateList(item.IdTaskList);
            return CreatedAtAction(nameof(GetById), new { id = item.IdTask }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TaskModel item)
        {
            if (id != item.IdTask) return BadRequest();
            var existing = await _service.GetById(id);
            if (existing == null) return NotFound();

            await _service.Update(item);
            await RecalculateList(item.IdTaskList);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.Tasks.FindAsync(id);
            if (existing == null) return NotFound();

            int listId = existing.IdTaskList;
            _context.Tasks.Remove(existing);
            await _context.SaveChangesAsync();

            await RecalculateList(listId);
            return NoContent();
        }

        private async Task RecalculateList(int taskListId)
        {
            var list = await _context.TaskLists
                .Include(t => t.Tasks)
                .FirstOrDefaultAsync(t => t.IdTaskList == taskListId);

            if (list == null) return;

            var all = list.Tasks?.ToList() ?? new List<TaskModel>();
            list.CompletionPercent = all.Count == 0
                ? 0m
                : Math.Round(100m * all.Count(t => t.Status == TaskStatus.done) / all.Count, 2);

            await _context.SaveChangesAsync();
        }
    }
}
