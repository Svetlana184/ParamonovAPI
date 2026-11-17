using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParamonovAPI.Models;
using ParamonovAPI.Services;
using TaskModel = ParamonovAPI.Models.Task;
using TaskStatus = ParamonovAPI.Models.TaskStatus;

namespace ParamonovAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskListController : ControllerBase
    {
        private readonly IService<TaskList> _service;
        private readonly PostgresContext _context;

        public TaskListController(IService<TaskList> service, PostgresContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskList>>> GetAll()
            => Ok(await _service.GetAll());

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskList>> GetById(int id)
        {
            var item = await _service.GetById(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TaskList item)
        {
            await _service.Create(item);
            return CreatedAtAction(nameof(GetById), new { id = item.IdTaskList }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TaskList item)
        {
            if (id != item.IdTaskList) return BadRequest();
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

        /// <summary>Пересчитать CompletionPercent по задачам списка.</summary>
        [HttpPost("{id}/recalculate-completion")]
        public async Task<ActionResult<decimal>> RecalculateCompletion(int id)
        {
            var list = await _context.TaskLists
                .Include(t => t.Tasks)
                .FirstOrDefaultAsync(t => t.IdTaskList == id);

            if (list == null) return NotFound();

            list.CompletionPercent = CalculateCompletion(list.Tasks);
            await _context.SaveChangesAsync();
            return Ok(list.CompletionPercent);
        }

        /// <summary>Пересчитать CompletionPercent для всех списков счёта.</summary>
        [HttpPost("by-invoice/{invoiceId}/recalculate-completion")]
        public async Task<IActionResult> RecalculateByInvoice(int invoiceId)
        {
            var lists = await _context.TaskLists
                .Include(t => t.Tasks)
                .Where(t => t.IdInvoice == invoiceId)
                .ToListAsync();

            if (lists.Count == 0) return NotFound();

            foreach (var list in lists)
                list.CompletionPercent = CalculateCompletion(list.Tasks);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static decimal CalculateCompletion(IEnumerable<TaskModel> tasks)
        {
            var all = tasks?.ToList() ?? new List<TaskModel>();
            if (all.Count == 0) return 0m;
            int done = all.Count(t => t.Status == "done");
            return Math.Round(100m * done / all.Count, 2);
        }
    }
}
