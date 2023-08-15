using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ToDoBot;

public class Program
{
    private readonly IServiceProvider _serviceProvider = CreateProvider();
    private readonly IConfigurationRoot _configuration = CreateConfiguration();

    static void Main()
        => new Program().RunAsync().GetAwaiter().GetResult();
    
    private static IServiceProvider CreateProvider()
    {
        var collection = new ServiceCollection();
        collection.AddScoped<ICalendarService, CalendarService>();
        
        return collection.BuildServiceProvider();
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
        var calendarService = _serviceProvider.GetRequiredService<ICalendarService>();
        
        // Read values from the configuration
        var token = _configuration.GetSection("AppSettings").GetSection("BotToken").Value;
        var channelId = Convert.ToUInt64(_configuration.GetSection("AppSettings").GetSection("ChannelId").Value);
        var calendarUrl = _configuration.GetSection("AppSettings").GetSection("CalendarUrl").Value;

        if (token == null || calendarUrl == null)
        {
            Console.WriteLine("Could not read values from appsettings.json");
            return;
        }
        
        var bot = new Bot(token, channelId, calendarUrl, calendarService);
        await bot.Start();
    }
}