using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SKP.OS.Backend.Controllers;

[AllowAnonymous]
public class AnonymousController(IWebHostEnvironment env) : Controller
{
    [HttpGet("/login")]
    public IActionResult Login() => SpaIndex();

    [HttpGet("/register")]
    public IActionResult Register() => SpaIndex();

    private IActionResult SpaIndex()
    {
        var filePath = Path.Combine(env.ContentRootPath, "wwwroot", "index.html");
        return PhysicalFile(filePath, "text/html");
    }
}
