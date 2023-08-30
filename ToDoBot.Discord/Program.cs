using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ToDoBot.Discord;

public class Program
{
    private readonly IConfigurationRoot _configuration = CreateConfiguration();

    static void Main()
        => new Program().RunAsync().GetAwaiter().GetResult();
    
    private static IServiceProvider CreateProvider()
    {
        return new ServiceCollection().BuildServiceProvider();
    }
    
    private static IConfigurationRoot CreateConfiguration()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.json")
            .Build();

        return config;
    }
    
    private async Task RunAsync()
    {
        // Read values from the configuration
        var token = _configuration.GetSection("AppSettings").GetSection("BotToken").Value;

        if (token == null)
        {
            Console.WriteLine("Could not read values from appsettings.json");
            return;
        }
        
        var bot = new Bot(token);
        await bot.Start();
    }
}