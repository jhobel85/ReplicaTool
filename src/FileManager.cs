using System.IO;
using Serilog;

public class FileManager
{
    private readonly ILogger _log;

    public FileManager(string logPath)
    {
        _log = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss}] {Message}{NewLine}")
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day,
                          outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] {Message}{NewLine}")
            .CreateLogger();
    }

    public void CreateDir(string destination)
    {
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
        try
        {
            File.WriteAllText(path, content);
            _log.Information($"File created: {path}");
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Failed to create file: {path}");
        }
    }

    public void CopyFile(string source, string destination)
    {
        try
        {
            DateTime sourceTime = File.GetLastWriteTimeUtc(source);
            DateTime replicaTime = File.GetLastWriteTimeUtc(destination);

            if (!File.Exists(destination) || sourceTime > replicaTime)
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

    public void DeleteDir(string source, string path)
    {
        try
        {
            if (!Directory.Exists(source))
            {
                Directory.Delete(path);
                _log.Information($"Directory deleted: {path}");
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Failed to delete directory: {path}");
        }
    }

    public void DeleteFile(string source, string path)
    {
        try
        {
            if (!File.Exists(source))
            {
                File.Delete(path);
                _log.Information($"File deleted: {path}");
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, $"Failed to delete file: {path}");
        }
    }

}
