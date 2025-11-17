using Serilog;
using ReplicaTool.Interfaces;

namespace ReplicaTool.Common
{
    public class SyncFileManager(string logPath, IFileComparer fileComparer)
    {
        private readonly ILogger _log = Logger.CreateFileAndConsoleLogger(logPath);
        private readonly IFileComparer _fileComparer = fileComparer;

        public void CreateDir(string? destination)
        {
            if (string.IsNullOrWhiteSpace(destination))
            {
                _log.Warning("CreateDir called with null or empty destination.");
                return;
            }

            try
            {
                if (!Directory.Exists(destination))
                {
                    Directory.CreateDirectory(destination);
                    _log.Information($"Directory created: {destination}");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"Failed to create directory: {destination}");
            }
        }

        public void CreateFile(string path, string content)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                _log.Warning("CreateFile called with null or empty path.");
                return;
            }

            try
            {
                string? directory = Path.GetDirectoryName(path);
                CreateDir(directory); // Ensure directory exists

                if (!File.Exists(path))
                {
                    File.WriteAllText(path, content);
                    _log.Information($"File created: {path}");
                }
                else
                {
                    _log.Information($"File already exists, skipping creation: {path}");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"Failed to create file: {path}");
            }
        }

        public void CopyFile(string source, string destination)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(destination))
            {
                _log.Warning($"CopyFile called with null or empty path(s): source='{source}', destination='{destination}'");
                return;
            }

            try
            {
                if (!File.Exists(source))
                {
                    _log.Warning($"Source file does not exist: {source}");
                    return;
                }

                string? destDir = Path.GetDirectoryName(destination);
                CreateDir(destDir); // Ensure directory exists

                if (!_fileComparer.AreFilesEqual(source, destination))
                {
                    File.Copy(source, destination, true);
                    _log.Information($"File copied: {source} → {destination}");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"Failed to copy file: {source} → {destination}");
            }
        }

        public void DeleteDir(string source, string destination)
        {
            if (string.IsNullOrWhiteSpace(destination))
            {
                _log.Warning("DeleteDir called with null or empty destination.");
                return;
            }

            try
            {
                if (!Directory.Exists(source) && Directory.Exists(destination))
                {

                    Directory.Delete(destination);
                    _log.Warning($"Directory deleted: {destination}");
                }
                else
                {
                    _log.Debug($"Skipped deletion: source exists or destination missing. Source='{source}', Destination='{destination}'");
                }

            }
            catch (Exception ex)
            {
                _log.Error(ex, $"Failed to delete directory: {destination}");
            }
        }

        public void DeleteFile(string source, string destination)
        {
            if (string.IsNullOrWhiteSpace(destination))
            {
                _log.Warning("DeleteFile called with null or empty destination.");
                return;
            }

            try
            {
                if (!File.Exists(source) && File.Exists(destination))
                {

                    File.Delete(destination);
                    _log.Warning($"File deleted: {destination}");
                }
                else
                {
                    _log.Debug($"Skipped deletion: source exists or destination missing. Source='{source}', Destination='{destination}'");
                }

            }
            catch (Exception ex)
            {
                _log.Error(ex, $"Failed to delete file: {destination}");
            }
        }

    }
}