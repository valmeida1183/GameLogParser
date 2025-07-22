using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Domain;

public class GameLogParser : IGameLogParser
{
    private readonly IFileReader _fileReader;

    private string _filePath;
    private GameInfo _currentGameInfo;
    private List<Dictionary<string, GameInfo>> _games;
    private Dictionary<int, string> _playerIdsToNames;
    private int _gameCount;

    public GameLogParser(IOptions<GameLogParserSettings> options, IFileReader fileReader)
    {
        _fileReader = fileReader;
        
        LoadFilePath(options?.Value?.FilePath);
    }

    public IEnumerable<Dictionary<string, GameInfo>> ParseLogFile()
    {
        ResetProperties();

        var lines = _fileReader.ReadLines(_filePath);

        foreach (var line in lines)
        {
            if (line.Contains("InitGame:"))
            {
                FinishCurrentGameInfo();
                InitNewGameInfo();

                continue;
            }

            if(_currentGameInfo == null)
                continue;

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
                FinishCurrentGameInfo();
            }
        }

        return _games;
    }

    private void LoadFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentNullException($"File path is cannot be empty");
        }
        else if (!_fileReader.FileExists(filePath))
        {
            throw new FileNotFoundException($"Log file not found at path: {filePath}");
        }

        _filePath = filePath;
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

    private void FinishCurrentGameInfo()
    {
        if (_currentGameInfo == null)
            return;

        _games.Add(new Dictionary<string, GameInfo>
        {
            { $"game_{_gameCount}", _currentGameInfo }
        });

        _currentGameInfo = null;
    }
}
