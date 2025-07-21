using Domain.Interfaces;
using Domain.Models;
using System.Text.RegularExpressions;

namespace _03_Domain;

public class GameLogParser : IParser
{
    private string _filePath;
    private GameInfo _currentGameInfo;
    private List<Dictionary<string, GameInfo>> _games;
    private Dictionary<int, string> _playerIdsToNames;
    private int _gameCount;

    public GameLogParser()
    {
        LoadLogFile();
    }

    public IEnumerable<Dictionary<string, GameInfo>> ParseLogFile()
    {
        ResetProperties();

        var lines = File.ReadAllLines(_filePath);

        foreach (var line in lines)
        {
            if (line.Contains("InitGame:") || _currentGameInfo == null)
            {
                InitNewGameInfo();

                continue;
            }

            if (line.Contains("ClientUserinfoChanged:"))
            {
                AddGamePlayer(line);
            }
            else if (line.Contains("Kill:"))
            {
                AddPlayerKills(line);
            }
            else if (line.Contains("ShutdownGame"))
            {
                FinishGameInfo();
            }
        }

        return _games;
    }

    private void LoadLogFile()
    {
        _filePath = Path.Combine("..", "GameLog.txt");

        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException($"Log file not found at path: {_filePath}");
        }
    }

    private void ResetProperties()
    {
        _currentGameInfo = null;
        _games = new List<Dictionary<string, GameInfo>>();
        _playerIdsToNames = new Dictionary<int, string>();
        _gameCount = 0;
    }

    private void InitNewGameInfo()
    {
        _currentGameInfo = new GameInfo();
        _playerIdsToNames.Clear();
        _gameCount++;
    }

    private void AddGamePlayer(string line)
    {
        var match = Regex.Match(line, @"ClientUserinfoChanged:\s*(\d+)\s+n\\([^\\]+)");

        if (match.Success)
        {
            int playerId = int.Parse(match.Groups[1].Value);
            string playerName = match.Groups[2].Value;

            _playerIdsToNames[playerId] = playerName;

            if (!_currentGameInfo.Players.Contains(playerName))
            {
                _currentGameInfo.Players.Add(playerName);
                _currentGameInfo.Kills[playerName] = 0;
            }
        }
    }

    private void AddPlayerKills(string line)
    {
        _currentGameInfo.TotalKills++;
        var match = Regex.Match(line, @"Kill:\s*\d+\s+\d+\s+\d+:\s+(.+?) killed (.+?) by");

        if (match.Success)
        {
            var killer = match.Groups[1].Value;
            var victim = match.Groups[2].Value;

            if (killer == "<world>")
            {
                if (_currentGameInfo.Kills.ContainsKey(victim))
                {
                    _currentGameInfo.Kills[victim]--;
                }
            }
            else if (killer != victim && _currentGameInfo.Kills.ContainsKey(killer))
            {
                _currentGameInfo.Kills[killer]++;
            }
        }
    }

    private void FinishGameInfo()
    {
        _games.Add(new Dictionary<string, GameInfo>
        {
            { $"game_{_gameCount}", _currentGameInfo }
        });

        _currentGameInfo = null;
    }
}
