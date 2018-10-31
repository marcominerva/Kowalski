namespace Kowalski.BusinessLayer.Services
{
    public interface IDateTimeService
    {
        string GetDate(string day);

        string GetTime();
    }
}