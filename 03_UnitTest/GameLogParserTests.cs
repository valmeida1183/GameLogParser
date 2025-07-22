using Domain;
using Domain.Interfaces;
using Microsoft.Extensions.Options;
using Moq;
using System.Reflection;

namespace UnitTest;

public class GameLogParserTests
{
    private readonly Mock<IFileReader> _mockFileReader;
    private readonly IOptions<GameLogParserSettings> _mockOptions;

    public GameLogParserTests()
    {
        _mockFileReader = new Mock<IFileReader>();
        _mockOptions = Mock.Of<IOptions<GameLogParserSettings>>(options =>
            options.Value == new GameLogParserSettings { FilePath = "test.log" });
    }

    [Fact]
    public void Parse_ShouldReturnSingleGame_WhenOneInitGameExists()
    {
        // Ararnge
        _mockFileReader.Setup(r => r.FileExists(It.IsAny<string>())).Returns(true);
        _mockFileReader.Setup(r => r.ReadLines(It.IsAny<string>())).Returns(new[]
        {
            "0:00 InitGame:",
            "0:20 ClientUserinfoChanged: 2 n\\Isgalamido\\t\\0\\model\\some_model",
            "0:25 ClientUserinfoChanged: 3 n\\Zeh\\t\\0\\model\\some_model",
            "0:30 Kill: 2 3 10: Isgalamido killed Zeh by MOD_RAILGUN",
            "0:45 ShutdownGame:"
        });

        var parser = new GameLogParser(_mockOptions, _mockFileReader.Object);

        // Act
        var result = parser.ParseLogFile();
        var game = result.First().Values.First();

        // Assert
        Assert.Single(result);
        Assert.Equal(1, game.TotalKills);
        Assert.Contains("Isgalamido", game.Players);
        Assert.Contains("Zeh", game.Players);
        Assert.Equal(1, game.Kills["Isgalamido"]);
        Assert.Equal(0, game.Kills["Zeh"]);
    }

    [Fact]
    public void Parse_ShouldHandleKillByWorldFlagAsAValidTotalKills()
    {
        _mockFileReader.Setup(r => r.FileExists(It.IsAny<string>())).Returns(true);
        _mockFileReader.Setup(r => r.ReadLines(It.IsAny<string>())).Returns(new[]
        {
            "0:00 InitGame:",
            "0:20 ClientUserinfoChanged: 2 n\\Isgalamido\\t\\0\\model\\some_model",
            "0:25 ClientUserinfoChanged: 3 n\\Zeh\\t\\0\\model\\some_model",
            "0:30 Kill: 1022 3 22: <world> killed Isgalamido by MOD_TRIGGER_HURT",
            "0:45 ShutdownGame:"
        });

        var parser = new GameLogParser(_mockOptions, _mockFileReader.Object);

        // Act
        var result = parser.ParseLogFile();
        var game = result.First().Values.First();

        // Assert
        Assert.Equal(1, game.TotalKills);
        Assert.Contains("Isgalamido", game.Kills.Keys);
        Assert.Equal(-1, game.Kills["Isgalamido"]);
    }

    [Fact]
    public void Parse_ShouldHandleMultipleGames()
    {
        _mockFileReader.Setup(r => r.FileExists(It.IsAny<string>())).Returns(true);
        _mockFileReader.Setup(r => r.ReadLines(It.IsAny<string>())).Returns(new[]
        {
           "0:00 InitGame:",
           "0:20 ClientUserinfoChanged: 2 n\\Isgalamido\\t\\0\\model\\some_model",
           "0:25 ClientUserinfoChanged: 3 n\\Zeh\\t\\0\\model\\some_model",
           "0:30 Kill: 1022 3 22: <world> killed Isgalamido by MOD_TRIGGER_HURT",
           "0:45 ShutdownGame:",
           "0:00 InitGame:",
           "0:20 ClientUserinfoChanged: 2 n\\Dono da Bola\\t\\0\\model\\some_model",
           "0:25 ClientUserinfoChanged: 3 n\\Assasinu Credi\\t\\0\\model\\some_model",
           "0:30 Kill: 1022 3 22: Dono da Bola killed Assasinu Credi by MOD_RAILGUN",
           "0:45 ShutdownGame:",
        });

        var parser = new GameLogParser(_mockOptions, _mockFileReader.Object);

        // Act
        var result = parser.ParseLogFile().ToList();

        var firstGame = result[0].Values.First();
        var secondGame = result[1].Values.First();

        // Assert
        Assert.Equal(2, result.Count());

        Assert.Equal(1, firstGame.TotalKills);
        Assert.Contains("Isgalamido", firstGame.Kills.Keys);

        Assert.Equal(1, secondGame.TotalKills);
        Assert.Contains("Dono da Bola", secondGame.Kills.Keys);
    }

    [Fact]
    public void Parse_ShouldCountKillsOnlyIfKillerIsDifferentThenVictim()
    {
        _mockFileReader.Setup(r => r.FileExists(It.IsAny<string>())).Returns(true);
        _mockFileReader.Setup(r => r.ReadLines(It.IsAny<string>())).Returns(new[]
        {
            "0:00 InitGame:",
            "0:20 ClientUserinfoChanged: 2 n\\Isgalamido\\t\\0\\model\\some_model",
            "0:25 ClientUserinfoChanged: 3 n\\Zeh\\t\\0\\model\\some_model",
            "0:30 Kill: 1022 3 22: Isgalamido killed Isgalamido by MOD_TRIGGER_HURT",
            "0:45 ShutdownGame:"
        });

        var parser = new GameLogParser(_mockOptions, _mockFileReader.Object);

        // Act
        var result = parser.ParseLogFile();
        var game = result.First().Values.First();

        // Assert
        Assert.Contains("Isgalamido", game.Kills.Keys);
        Assert.Equal(0, game.Kills["Isgalamido"]);
    }
}
