using Microsoft.EntityFrameworkCore;
using ParamonovAPI.Models;
using Task = System.Threading.Tasks.Task;

namespace ParamonovAPI.Services
{
    public class TaskListService : IService<TaskList>
    {
        private readonly PostgresContext _context;

        public TaskListService(PostgresContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskList>> GetAll()
            => await _context.TaskLists.ToListAsync();

        public async Task<TaskList> GetById(int id)
            => await _context.TaskLists.AsNoTracking().FirstOrDefaultAsync(t => t.IdTaskList == id);

        public async Task Create(TaskList item)
        {
            await _context.TaskLists.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task Update(TaskList item)
        {
            _context.Entry(item).State = EntityState.Modified;
            _context.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var item = await _context.TaskLists.FindAsync(id);
            if (item != null)
            {
                _context.TaskLists.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}
