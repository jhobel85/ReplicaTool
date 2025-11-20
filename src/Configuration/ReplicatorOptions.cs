
using ReplicaTool.Interfaces;
using ReplicaTool.Common;
using Serilog;

namespace ReplicaTool.Configuration
{
    public class ReplicatorOptions : IReplicatorOptions
    {
        private readonly ILogger _log = Logger.CLI_LOGGER;
        public string SourcePath { get; set; } = "";
        public string ReplicaPath { get; set; } = "";
        public string LogFilePath { get; set; } = "";
        public TimeSpan SyncInterval { get; set; } = TimeSpan.Zero;

        public static ReplicatorOptions Parse(string[] args)
        {
            var options = new ReplicatorOptions();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--source":
                        options.SourcePath = args.ElementAtOrDefault(++i) ?? "";
                        break;
                    case "--replica":
                        options.ReplicaPath = args.ElementAtOrDefault(++i) ?? "";
                        break;
                    case "--log":
                        options.LogFilePath = args.ElementAtOrDefault(++i) ?? "";
                        break;
                    case "--interval":
                        if (int.TryParse(args.ElementAtOrDefault(++i), out int parsed))
                            options.SyncInterval = TimeSpan.FromSeconds(parsed);
                        break;
                }
            }
            return options;
        }

        public bool ArgumentsProvided()
        {
            if (!ValidateBasicArguments())
                return false;

            if (!ValidateIntervalBounds())
                return false;

            if (!ValidatePathsNotEmpty())
                return false;

            if (!TryGetFullPath(SourcePath, out string fullSourcePath) ||
                !TryGetFullPath(ReplicaPath, out string fullReplicaPath) ||
                !TryGetFullPath(LogFilePath, out string logPath))
                return false;

            if (!ValidatePathLength(fullSourcePath, 248) || !ValidatePathLength(fullReplicaPath, 248))
                return false;

            if (!ValidatePathCharacters(SourcePath) || !ValidatePathCharacters(ReplicaPath) ||
                !ValidatePathCharacters(LogFilePath))
                return false;

            if (!ValidateDriveExistsOnWin(fullSourcePath) || !ValidateDriveExistsOnWin(fullReplicaPath))
                return false;

            if (!VerifyLogExtension(logPath))
                return false;

            if (!ValidatePathsAreDifferent(fullSourcePath, fullReplicaPath))
                return false;

            if (!ValidateLogDirectory(logPath))
                return false;

            return true;
        }

        private bool ValidateBasicArguments()
        {
            bool ret = true;
            if (string.IsNullOrEmpty(SourcePath) || string.IsNullOrEmpty(ReplicaPath) ||
                string.IsNullOrEmpty(LogFilePath) || SyncInterval <= TimeSpan.Zero)
            {
                _log.Error("Not all arguments were provided.");
                PrintArguments();
                _log.Information("Usage: dotnet run --source <path> --replica <path> --log <path> --interval <seconds>");
                _log.Information("Example: dotnet run --source data/source/ --replica data/replica/ --log logs/app.log --interval 5");
                ret = false;
            }
            return ret;
        }

        private bool ValidateIntervalBounds()
        {
            bool ret = true;
            if (SyncInterval.TotalSeconds < 1 || SyncInterval.TotalHours > 24)
            {
                _log.Error($"Sync interval must be between 1 second and 24 hours. Current: {SyncInterval.TotalSeconds} seconds");
                ret = false;
            }
            return ret;
        }

        private bool ValidatePathsNotEmpty()
        {
            bool ret = true;
            SourcePath = SourcePath.Trim();
            ReplicaPath = ReplicaPath.Trim();
            LogFilePath = LogFilePath.Trim();

            if (string.IsNullOrWhiteSpace(SourcePath) || string.IsNullOrWhiteSpace(ReplicaPath) ||
                string.IsNullOrWhiteSpace(LogFilePath))
            {
                _log.Error("Paths cannot be empty or contain only whitespace.");
                ret = false;
            }
            return ret;
        }

        private bool TryGetFullPath(string path, out string fullPath)
        {
            bool ret = true;
            try
            {
                fullPath = Path.GetFullPath(path);
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"Invalid {path} path format detected.");
                fullPath = string.Empty;
                ret = false;
            }
            return ret;
        }

        private bool ValidatePathLength(string fullPath, int maxLength)
        {
            bool ret = true;
            if (fullPath.Length > maxLength)
            {
                _log.Error($"{fullPath} path length {fullPath.Length} exceeds safe limit ({maxLength} characters).");
                ret = false;
            }
            return ret;
        }

        private bool ValidatePathCharacters(string path)
        {
            bool ret = true;
            var invalidChars = Path.GetInvalidPathChars();
            if (path.IndexOfAny(invalidChars) >= 0)
            {
                _log.Error($"{path} path contains invalid characters.");
                ret = false;
            }
            return ret;
        }

        private bool ValidateDriveExistsOnWin(string fullPath)
        {
            bool ret = true;

            // Skip drive validation on non-Windows platforms
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                if (!fullPath.StartsWith(@"\\") && fullPath.Length >= 3 && fullPath[1] == ':')
                {
                    var drive = fullPath.Substring(0, 3);
                    if (!Directory.Exists(drive))
                    {
                        _log.Error($"{fullPath} drive does not exist or is not accessible: {drive}");
                        ret = false;
                    }
                }
            }

            return ret;
        }

        private bool VerifyLogExtension(string logPath)
        {
            bool ret = true;
            var logExtension = Path.GetExtension(logPath).ToLowerInvariant();

            // Check for binary/executable extensions
            string[] binaryExtensions = { ".exe", ".dll", ".so", ".dylib", ".bin", ".zip", ".rar", ".7z", ".tar", ".gz" };
            if (binaryExtensions.Contains(logExtension))
            {
                _log.Error($"Log file cannot be a binary/executable file. Extension '{logExtension}' is not allowed.");
                ret = false;
            }

            // Warn about unusual but allowed extensions
            if (!string.IsNullOrEmpty(logExtension) && logExtension != ".log" && logExtension != ".txt")
            {
                _log.Warning($"Log file extension '{logExtension}' is unusual. Consider using .log or .txt");
            }
            return ret;
        }

        private bool ValidatePathsAreDifferent(string fullSourcePath, string fullReplicaPath)
        {
            bool ret = true;
            // Check if paths are identical
            if (string.Equals(fullSourcePath, fullReplicaPath, StringComparison.OrdinalIgnoreCase))
            {
                _log.Error("Source and Replica paths must be different, both set to: {fullSourcePath}");
                ret = false;
            }

            // Check if replica is a subdirectory of source
            if (fullReplicaPath.StartsWith(fullSourcePath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                _log.Error("Replica path cannot be a subdirectory of Source path.");
                _log.Error($"Source: {fullSourcePath}");
                _log.Error($"Replica: {fullReplicaPath}");
                ret = false;
            }

            // Check if source is a subdirectory of replica
            if (fullSourcePath.StartsWith(fullReplicaPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                _log.Error("Source path cannot be a subdirectory of Replica path.");
                _log.Error($"Source: {fullSourcePath}");
                _log.Error($"Replica: {fullReplicaPath}");
                ret = false;
            }

            return ret;
        }

        private bool ValidateLogDirectory(string logPath)
        {
            bool ret = true;
            try
            {
                var logDir = Path.GetDirectoryName(logPath);
                // Log directory will be created in CreateFileAndConsoleLogger() if needed
                if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
                {
                    _log.Information($"Log directory does not exist. It will be created: {logDir}");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Invalid log file path.");
                ret = false;
            }
            return ret;
        }

        private void PrintArguments()
        {
            _log.Information($"Source: {SourcePath}");
            _log.Information($"Replica: {ReplicaPath}");
            _log.Information($"Log: {LogFilePath}");
            _log.Information($"Interval: {SyncInterval.Seconds} seconds (must be > 0)");
            _log.Information("");
        }
    }
}