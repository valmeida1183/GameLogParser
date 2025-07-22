using Domain.Models;

namespace Domain.Interfaces;
public interface IGameLogParser
{
    IEnumerable<Dictionary<string, GameInfo>> ParseLogFile();
}
