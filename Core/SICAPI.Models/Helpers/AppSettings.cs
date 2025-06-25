using Microsoft.Extensions.Configuration;

namespace SICAPI.Models.Helpers;

public class AppSettings
{
    private readonly IConfiguration Configuration;

    public AppSettings(IConfiguration configuration)
    {
        Configuration = configuration;
    }


    public static string GetAppSetting(string section,string key)
    {
        var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        IConfigurationRoot configuration = builder.Build();

        return configuration.GetSection(section).GetSection(key).Value!;
    }
    
    public static string GetAppSetting(string key)
    {
        var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        IConfigurationRoot configuration = builder.Build();

        return configuration.GetSection(key).Value!;
    }
}