using _03_Domain;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameLogController : ControllerBase
{
    [HttpGet]
    public IActionResult GetGames()
    {
        var parser = new GameLogParser();
        var parsedGames = parser.ParseLogFile();

        return Ok(parsedGames);
    }
}
