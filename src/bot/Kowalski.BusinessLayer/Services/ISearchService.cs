using System.Threading.Tasks;

namespace Kowalski.BusinessLayer.Services
{
    public interface ISearchService
    {
        Task<string> SearchAsync(string query);
    }
}