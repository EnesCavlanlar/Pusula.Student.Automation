using System.Threading.Tasks;

namespace Pusula.Student.Automation.Tests
{
    public interface IRedisCacheTestAppService
    {
        Task<string> GetOrSetAsync(string key);
    }
}
