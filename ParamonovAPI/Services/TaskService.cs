using Microsoft.EntityFrameworkCore;
using TaskModel = ParamonovAPI.Models.Task;

namespace ParamonovAPI.Services
{
    public class TaskService : IService<TaskModel>
    {
        private readonly PostgresContext _context;

        public TaskService(PostgresContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskModel>> GetAll()
            => await _context.Tasks.ToListAsync();

        public async Task<TaskModel> GetById(int id)
            => await _context.Tasks.FindAsync(id);

        public async Task Create(TaskModel item)
        {
            await _context.Tasks.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task Update(TaskModel item)
        {
            _context.Entry(item).State = EntityState.Modified;
            _context.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var item = await _context.Tasks.FindAsync(id);
            if (item != null)
            {
                _context.Tasks.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}
