using System.Threading.Tasks;

namespace Kowalski.BusinessLayer.Services
{
    public interface IJokeService
    {
        Task<string> GetJokeAsync();
    }
}