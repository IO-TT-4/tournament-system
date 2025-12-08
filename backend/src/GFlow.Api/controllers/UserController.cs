using Microsoft.AspNetCore.Mvc;

namespace GFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        [HttpGet("{reqId}")]
        public IActionResult Get(int reqId)
        {
            return Ok(new
            {
                id=reqId,
                username="piotr",
                email="piotr@maj.pl"
            });
        }
    }
}