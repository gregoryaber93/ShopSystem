using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShopService.Api.Controllers;

[ApiController]
[Route("test")]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class TestController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> Get()
    {
        return "test";
    }
}
