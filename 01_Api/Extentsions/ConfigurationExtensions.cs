using Domain;
using Domain.Interfaces;

namespace Api.Extentsions;

public static class ConfigurationExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GameLogParserSettings>(configuration.GetSection("GameLogParser"));
        services.AddScoped<IGameLogParser, GameLogParser>();
        services.AddScoped<IFileReader, FileReader>();

        return services;
    }


}
