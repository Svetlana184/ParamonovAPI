namespace ParamonovAPI.Services
{
    public interface IService<T>
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> GetById(int id);
        Task Create(T product);
        Task Update(T product);
        Task Delete(int id);
    }
}
