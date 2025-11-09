using Microsoft.AspNetCore.Mvc;
using Pusula.Student.Automation.Tests; // interface burada
using System.Threading.Tasks;

namespace Pusula.Student.Automation.Controllers
{
    [Route("api/redis-test")]
    public class RedisTestController : AutomationController
    {
        private readonly IRedisCacheTestAppService _service;
        public RedisTestController(IRedisCacheTestAppService service)
        {
            _service = service;
        }

        [HttpGet("{key}")]
        public async Task<string> GetAsync(string key) => await _service.GetOrSetAsync(key);
    }
}
