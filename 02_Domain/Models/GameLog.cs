namespace Domain.Models;
public class GameInfo
{
    public int TotalKills { get; set; }
    public ICollection<string> Players { get; set; } = [];
    public Dictionary<string, int> Kills { get; set; } = [];
}
