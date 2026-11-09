using Microsoft.EntityFrameworkCore;
using ParamonovAPI.Models;
using Task = System.Threading.Tasks.Task;

namespace ParamonovAPI.Services
{
    public class SpecificationItemService : IService<SpecificationItem>
    {
        private readonly PostgresContext _context;

        public SpecificationItemService(PostgresContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SpecificationItem>> GetAll()
            => await _context.SpecificationItems.ToListAsync();

        public async Task<SpecificationItem> GetById(int id)
            => await _context.SpecificationItems.FindAsync(id);

        public async Task Create(SpecificationItem item)
        {
            await _context.SpecificationItems.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task Update(SpecificationItem item)
        {
            _context.Entry(item).State = EntityState.Modified;
            _context.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var item = await _context.SpecificationItems.FindAsync(id);
            if (item != null)
            {
                _context.SpecificationItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}
