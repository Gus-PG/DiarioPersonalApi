using DiarioPersonalApi.Models;

namespace DiarioPersonalApi.Data.Repositories
{
    public interface IEntradaRepository : IRepository<Entrada>
    {
        Task<IEnumerable<Entrada>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Entrada>> SearchByHashtagAsync(int userId, string hashtag);
        Task<IEnumerable<Entrada>> SearchByHashtagAdminAsync(string hashtag);
    }
}
