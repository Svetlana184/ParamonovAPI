using Microsoft.EntityFrameworkCore;
using ParamonovAPI.Models;
using Task = System.Threading.Tasks.Task;

namespace ParamonovAPI.Services
{
    public class CompanyService : IService<Company>
    {
        private readonly PostgresContext _context;

        public CompanyService(PostgresContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Company>> GetAll()
            => await _context.Companies.ToListAsync();

        public async Task<Company> GetById(int id)
            => await _context.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.IdCompany == id);

        public async Task Create(Company item)
        {
            await _context.Companies.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Company item)
        {
            _context.Entry(item).State = EntityState.Modified;
            _context.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var item = await _context.Companies.FindAsync(id);
            if (item != null)
            {
                _context.Companies.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}
