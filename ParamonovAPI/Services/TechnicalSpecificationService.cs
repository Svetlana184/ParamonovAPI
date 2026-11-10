using Microsoft.EntityFrameworkCore;
using ParamonovAPI.Models;
using Task = System.Threading.Tasks.Task;

namespace ParamonovAPI.Services
{
    public class TechnicalSpecificationService : IService<TechnicalSpecification>
    {
        private readonly PostgresContext _context;

        public TechnicalSpecificationService(PostgresContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TechnicalSpecification>> GetAll()
            => await _context.TechnicalSpecifications.ToListAsync();

        public async Task<TechnicalSpecification> GetById(int id)
             => await _context.TechnicalSpecifications
                 .AsNoTracking()
                 .FirstOrDefaultAsync(s => s.IdSpec == id);

        public async Task Create(TechnicalSpecification item)
        {
            await _context.TechnicalSpecifications.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task Update(TechnicalSpecification item)
        {
            _context.Entry(item).State = EntityState.Modified;
            _context.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var item = await _context.TechnicalSpecifications.FindAsync(id);
            if (item != null)
            {
                _context.TechnicalSpecifications.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}
