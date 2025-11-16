namespace ReplicaTool.Interfaces
{
    public interface IReplicatorOptions
    {
        string SourcePath { get; }
        string ReplicaPath { get; }
        string LogFilePath { get; }
        TimeSpan SyncInterval { get; }
        bool ArgumentsProvided();
    }
}