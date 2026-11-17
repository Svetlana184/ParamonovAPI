using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParamonovAPI.Models;
using ParamonovAPI.Services;
using System.Globalization;
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

        /// <summary>Проекты текущего пользователя. userId — из заголовка X-User-Id.</summary>
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<Project>>> GetMyProjects()
        {
            if (!Request.Headers.TryGetValue("X-User-Id", out var h) ||
                !int.TryParse(h.ToString(), out int userId))
                return Unauthorized();

            return await GetMyProjectsInternal(userId);
        }

        [HttpGet("my/{userId}")]
        public async Task<ActionResult<IEnumerable<Project>>> GetMyProjects(int userId)
            => await GetMyProjectsInternal(userId);

        private async Task<ActionResult<IEnumerable<Project>>> GetMyProjectsInternal(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            IQueryable<Project> query = _context.Projects;

            if (string.Equals(user.Role, "customer", StringComparison.OrdinalIgnoreCase))
                query = query.Where(p => p.IdCustomer == userId);
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
            var item = await _context.Projects
                .AsNoTracking()
                .Include(p => p.IdInvoiceNavigation)
                    .ThenInclude(i => i!.TaskLists)
                        .ThenInclude(tl => tl.Tasks)
                .FirstOrDefaultAsync(p => p.IdProject == id);

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
            var existing = await _context.Projects.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = item.Name;
            existing.Description = item.Description;
            existing.IdTeam = item.IdTeam;
            existing.IdInvoice = item.IdInvoice;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.Projects.FindAsync(id);
            if (existing == null) return NotFound();
            _context.Projects.Remove(existing);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>draft → in_progress</summary>
        [HttpPost("{id}/submit")]
        public async Task<IActionResult> Submit(int id)
            => await ChangeStatus(id, ProjectStatus.draft, ProjectStatus.in_progress);

        /// <summary>
        /// in_progress → pending_approval.
        /// При отправке создаются ChangeRequest (status=created) по счёту, задачам и пунктам ТЗ.
        /// </summary>
        [HttpPost("{id}/send-invoice")]
        public async Task<IActionResult> SendInvoice(int id)
        {
            // 1. Загружаем проект со счётом, задачами и пунктами ТЗ
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.IdProject == id);

            if (project == null) return NotFound();

            // 2. Проверка статуса
            if (project.Status != ProjectStatus.in_progress)
                return BadRequest($"Ожидался статус in_progress, текущий {project.Status}");

            // 3. Проверка счёта
            if (project.IdInvoice == null)
                return BadRequest("У проекта нет счёта");

            var invoice = project.IdInvoiceNavigation;
            if (invoice == null)
                return BadRequest("Счёт проекта не найден");

            // 4. Автор change request — из заголовка X-User-Id
            if (!Request.Headers.TryGetValue("X-User-Id", out var h) ||
                !int.TryParse(h.ToString(), out int userId))
                return Unauthorized();

            var creator = await _context.Users.FindAsync(userId);
            if (creator == null) return Unauthorized();

            // 5. Транзакция: удаление старых created + создание новых + смена статуса
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow;

                // 5.1. Удаляем старые created-запросы этого счёта (защита от дублей)
                var oldCreated = await _context.ChangeRequests
                    .Where(c => c.IdInvoice == invoice.IdInvoice &&
                                c.Status == Models.ChangeStatus.created)
                    .ToListAsync();

                if (oldCreated.Count > 0)
                    _context.ChangeRequests.RemoveRange(oldCreated);

                // 5.2. Счёт — как согласуемый документ
                _context.ChangeRequests.Add(new ChangeRequest
                {
                    IdInvoice = invoice.IdInvoice,
                    Name = $"Согласование счёта «{invoice.Name}»",
                    Description =
                        $"Стоимость: {invoice.Cost.ToString("N2", CultureInfo.InvariantCulture)} ₽. " +
                        $"Сроки: {invoice.StartDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "—"} — " +
                        $"{invoice.EndDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "—"}",
                    CreatedAt = now,
                    Status = Models.ChangeStatus.created,
                    IdCreator = userId
                });

                // 5.3. Задачи — по одной на каждую
                var allTasks = invoice.TaskLists.SelectMany(tl => tl.Tasks).ToList();
                foreach (var t in allTasks)
                {
                    _context.ChangeRequests.Add(new ChangeRequest
                    {
                        IdInvoice = invoice.IdInvoice,
                        IdTask = t.IdTask,
                        Name = $"Задача #{t.IdTask}",
                        Description =
                            (t.Description ?? "Без описания") +
                            $" | План: {t.PlannedHours?.ToString("N2", CultureInfo.InvariantCulture) ?? "—"} ч" +
                            $" | Факт: {t.ActualHours?.ToString("N2", CultureInfo.InvariantCulture) ?? "—"} ч" +
                            $" | Стоимость: {t.Cost?.ToString("N2", CultureInfo.InvariantCulture) ?? "—"} ₽",
                        CreatedAt = now,
                        Status = Models.ChangeStatus.created,
                        IdCreator = userId
                    });
                }

                // 5.4. Пункты ТЗ — по одному на каждый
                var allSpecItems = project.TechnicalSpecifications
                    .SelectMany(s => s.SpecificationItems)
                    .ToList();

                foreach (var si in allSpecItems)
                {
                    _context.ChangeRequests.Add(new ChangeRequest
                    {
                        IdInvoice = invoice.IdInvoice,
                        IdSpecItem = si.IdItem,
                        Name = $"Пункт ТЗ «{si.Name}»",
                        Description = si.Description ?? "Без описания",
                        CreatedAt = now,
                        Status = Models.ChangeStatus.created,
                        IdCreator = userId
                    });
                }

                // 5.5. Fallback: если задач и пунктов нет — один сводный запрос
                if (allTasks.Count == 0 && allSpecItems.Count == 0)
                {
                    _context.ChangeRequests.Add(new ChangeRequest
                    {
                        IdInvoice = invoice.IdInvoice,
                        Name = "Согласование изменений по проекту",
                        Description = "Изменения по счёту и задачам",
                        CreatedAt = now,
                        Status = Models.ChangeStatus.created,
                        IdCreator = userId
                    });
                }

                // 5.6. Смена статуса
                project.Status = ProjectStatus.pending_approval;

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return StatusCode(500, $"Ошибка при создании change request: {ex.Message}");
            }
        }

        /// <summary>pending_approval → in_progress</summary>
        [HttpPost("{id}/finish-approval")]
        public async Task<IActionResult> FinishApproval(int id)
            => await ChangeStatus(id, ProjectStatus.pending_approval, ProjectStatus.in_progress);

        [HttpPost("{id}/freeze")]
        public async Task<IActionResult> Freeze(int id)
        {
            var p = await _context.Projects.FindAsync(id);
            if (p == null) return NotFound();
            if (p.Status == ProjectStatus.completed || p.Status == ProjectStatus.closed)
                return BadRequest("Нельзя заморозить завершённый/закрытый проект");
            p.Status = ProjectStatus.frozen;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/unfreeze")]
        public async Task<IActionResult> Unfreeze(int id)
        {
            var p = await _context.Projects.FindAsync(id);
            if (p == null) return NotFound();
            if (p.Status != ProjectStatus.frozen)
                return BadRequest("Проект не заморожен");
            p.Status = ProjectStatus.in_progress;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>permanent=true → closed, иначе → completed</summary>
        [HttpPost("{id}/close")]
        public async Task<IActionResult> Close(int id, [FromQuery] bool permanent = false)
        {
            var p = await _context.Projects.FindAsync(id);
            if (p == null) return NotFound();
            p.Status = permanent ? ProjectStatus.closed : ProjectStatus.completed;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<IActionResult> ChangeStatus(int id, ProjectStatus from, ProjectStatus to)
        {
            var p = await _context.Projects.FindAsync(id);
            if (p == null) return NotFound();
            if (p.Status != from)
                return BadRequest($"Ожидался статус {from}, текущий {p.Status}");
            p.Status = to;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("draft")]
        public async Task<IActionResult> CreateDraft([FromBody] CreateProjectDraftDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var customer = await _context.Users.FindAsync(dto.IdCustomer);
            if (customer == null)
                return BadRequest($"Пользователь с id={dto.IdCustomer} не найден.");

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var project = new Project
                {
                    IdCustomer = dto.IdCustomer,
                    IdTeam = null,
                    Name = string.IsNullOrWhiteSpace(dto.Name) ? "Новый проект" : dto.Name.Trim(),
                    Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                    Status = ProjectStatus.draft
                };
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                _context.TechnicalSpecifications.Add(new TechnicalSpecification
                {
                    IdProject = project.IdProject,
                    Name = $"ТЗ для проекта «{project.Name}»"
                });
                await _context.SaveChangesAsync();

                var invoice = new Invoice
                {
                    Name = $"Счет для проекта «{project.Name}»",
                    Cost = 0
                };
                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                project.IdInvoice = invoice.IdInvoice;

                _context.TaskLists.Add(new TaskList
                {
                    IdInvoice = invoice.IdInvoice,
                    Name = "Список задач",
                    CompletionPercent = 0
                });
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return CreatedAtAction(nameof(GetById), new { id = project.IdProject }, project);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Ошибка при создании черновика: {ex.Message}");
            }
        }
    }
}
