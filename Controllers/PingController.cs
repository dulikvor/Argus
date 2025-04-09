using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Argus.Controllers
{
    [AllowAnonymous]
    [ApiController]
    public class PingController : ControllerBase
    {
        private const string PingResponse = "Argus saying hello :)";

        [HttpGet]
        [Route("")]
        [Route("/favicon.ico")]
        public string GetHealthStatus()
        {
            return PingResponse;
        }
    }
}