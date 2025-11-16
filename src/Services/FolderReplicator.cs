using System;
using Serilog;
using ReplicaTool.Interfaces;
using ReplicaTool.Common;

namespace ReplicaTool.Services
{
    public class FolderReplicator(IReplicatorOptions options, FileManager fileMgr) : IReplicator
    {
        public FileManager FileMgr { get; private set; } = fileMgr;
        private readonly string _sourcePath = options.SourcePath;
        private readonly string _replicaPath = options.ReplicaPath;

        public void Replicate()
        {
            SyncNewPaths(); // Create or Copy new directories and files
            CleanupReplicaDirectories();
            CleanupReplicaFiles();
        }

        private void SyncNewPaths()
        {
            //Get the list of directories and files
            var sourceDirs = Directory.GetDirectories(_sourcePath, "*", SearchOption.AllDirectories);
            var sourceFiles = Directory.GetFiles(_sourcePath, "*", SearchOption.AllDirectories);

            //Ensure directories exists
            FileMgr.CreateDir(_replicaPath);
            foreach (string sourceDirPath in sourceDirs)
            {
                string relativePath = Path.GetRelativePath(_sourcePath, sourceDirPath);
                string destinationDir = Path.Combine(_replicaPath, relativePath);
                FileMgr.CreateDir(destinationDir);
            }

            //Copy each source file
            foreach (string sourcefile in sourceFiles)
            {
                string relativePath = Path.GetRelativePath(_sourcePath, sourcefile);
                string destinationFile = Path.Combine(_replicaPath, relativePath);
                FileMgr.CopyFile(sourcefile, destinationFile);
            }
        }

        private void CleanupReplicaDirectories()
        {
            var replicaDirs = Directory.GetDirectories(_sourcePath, "*", SearchOption.AllDirectories);
            foreach (string replicaDir in replicaDirs)
            {
                string relativePath = Path.GetRelativePath(_replicaPath, replicaDir);
                string sourceDir = Path.Combine(_sourcePath, relativePath);
                FileMgr.DeleteDir(sourceDir, replicaDir);
            }
        }

        private void CleanupReplicaFiles()
        {
            var replicaFiles = Directory.GetFiles(_replicaPath, "*", SearchOption.AllDirectories);
            foreach (string replicafile in replicaFiles)
            {
                string relativePath = Path.GetRelativePath(_replicaPath, replicafile);
                string sourceFile = Path.Combine(_sourcePath, relativePath);
                FileMgr.DeleteFile(sourceFile, replicafile);
            }
        }
    }
}