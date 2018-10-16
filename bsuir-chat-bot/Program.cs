using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace bsuir_chat_bot
{
    internal static class Program
    {
        private static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(theme: SystemConsoleTheme.Grayscale)
                .WriteTo.File(@"../logs/log.txt",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true)
                .CreateLogger();
            
            Log.Information("Marvin started!");
            
            var bot = new Bot("botconfig.json");
            bot.Start();
            
            Log.Information("System Halt! Bye.");
            Log.CloseAndFlush();
        }
    }
}