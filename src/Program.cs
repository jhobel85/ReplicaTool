using ReplicaTool;
using ReplicaTool.Interfaces;
using ReplicaTool.Common;
using ReplicaTool.Configuration;
using ReplicaTool.Services;

var _log = Logger.CLI_LOGGER;
_log.Information("App started.");

//var options = new ReplicatorOptions();
var options = ReplicatorOptionsCmd.Parse(args);

if (!options.ArgumentsProvided())
{
    _log.Error("App exited due to invalid arguments.");
    return;
}

var comparer = new Md5FileComparer();
var fileMgr = new FileManager(options.LogFilePath, comparer);
FolderReplicator replicator = new FolderReplicator(options, fileMgr);
string tmpfilePath = options.ReplicaPath + Path.Combine("tmp.txt");
string tmpContent = "File is created for test purposes to demonstrate deletion operation in replicat folder.";
replicator.FileMgr.CreateFile(tmpfilePath, tmpContent);

var scheduler = new Scheduler(replicator, options.SyncInterval);
scheduler.Start();
Console.CancelKeyPress += scheduler.OnExit;
_log.Information("Scheduler started. Press Ctrl+C to exit.");

// Block until exit is requested
using var waitHandle = new ManualResetEvent(false);
waitHandle.WaitOne(); 



