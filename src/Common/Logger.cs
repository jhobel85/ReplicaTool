using System.IO;
using Serilog;

namespace ReplicaTool.Common
{
    public class Logger
    {
        public static ILogger CLI_LOGGER { get; private set; } = CreateConsoleLogger();

        public static ILogger CreateConsoleLogger()
        {
            return new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "[{Level}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }
        
        public static ILogger CreateFileAndConsoleLogger(string logPath)
        {
            // Ensure the directory for the log file exists. 
            try
            {
                var dir = Path.GetDirectoryName(logPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            catch (Exception ex)
            {                
                CLI_LOGGER.Error(ex, $"Failed to create log directory: {logPath}, use console logger instead.");
                return CLI_LOGGER;
            }

            return new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "[{Level}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
        }
    };
}