using System;
using System.Threading.Tasks;

namespace Kowalski.DataAccessLayer
{
    public interface IDbContext
    {
        Task<T> GetAsync<T>(string query, object param = null) where T : class;
    }
}