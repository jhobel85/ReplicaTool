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
            //Ensure directories exists and get list of sub-directories
            FileMgr.CreateDir(_sourcePath);            
            FileMgr.CreateDir(_replicaPath);
            var sourceDirs = Directory.GetDirectories(_sourcePath, "*", SearchOption.AllDirectories);
            foreach (string sourceDirPath in sourceDirs)
            {
                string relativePath = Path.GetRelativePath(_sourcePath, sourceDirPath);
                string destinationDir = Path.Combine(_replicaPath, relativePath);
                FileMgr.CreateDir(destinationDir);
            }

            //Get the list of files in source directory and copy them to replica
            var sourceFiles = Directory.GetFiles(_sourcePath, "*", SearchOption.AllDirectories);
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