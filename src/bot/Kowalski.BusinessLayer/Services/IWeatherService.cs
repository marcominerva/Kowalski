using System.Threading.Tasks;

namespace Kowalski.BusinessLayer.Services
{
    public interface IWeatherService
    {
        Task<string> GetWeatherAsync(string location);
    }
}