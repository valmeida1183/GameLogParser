using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class GameLogController : ControllerBase
{
    private readonly IGameLogParser _gameLogParser;

    public GameLogController(IGameLogParser gameLogParser)
    {
        _gameLogParser = gameLogParser;
    }

    [HttpGet]
    public IActionResult GetGames()
    {
        var parsedGames = _gameLogParser.ParseLogFile();

        return Ok(parsedGames);
    }
}
