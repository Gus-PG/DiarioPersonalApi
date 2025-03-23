namespace DiarioPersonalApi.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);  // Es síncrono.
        void Delete(T entity);  // Es síncrono.
        Task SaveChangesAsync();

    }
}
