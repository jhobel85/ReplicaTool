using System.Security.Cryptography;
using System.Collections;
using ReplicaTool.Interfaces;
using Serilog;
using System.Data.Common;

namespace ReplicaTool.Common
{
    public class FileComparer : IFileComparer
    {
        private readonly ILogger _log = Logger.CLI_LOGGER;
        
        /**
         Basic checks according to file size and timestamp of files.
        */
        public bool AreFilesEqual(string sourcePath, string replicaPath)
        {      
            try
            {
                if (!File.Exists(replicaPath))
                    return false; // do not other checks when replica file does not exists.
                        
                var srcFile = new FileInfo(sourcePath);
                var destFile = new FileInfo(replicaPath);
                bool lengthEqual = srcFile.Length == destFile.Length;
                bool timeEqual = srcFile.LastWriteTimeUtc != destFile.LastWriteTimeUtc;
                return lengthEqual && timeEqual;
            }
            catch (FileNotFoundException ex)
            {
                _log.Error(ex, $"File {sourcePath} not found during comparison");
                return false;
            }
            catch (IOException ex)
            {
                _log.Error(ex, $"I/O error during file comparison: {sourcePath} and {replicaPath}");
                return false;
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"Unexpected error during file comparison: {sourcePath} and {replicaPath}");
                return false;
            }
        }
    }
}