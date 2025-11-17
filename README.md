# Veeam Assesment task in C# based on Veeam_tesk_task_С_sharp_QA.pdf

Requirements 
- Linux or Windows
- VisualStudio Code or VisualStudio
- .NET 8.0
- C# Dev Kit
- Git, Github

Brief overview of console application *ReplicaTool*

App requries input arguments (source, replice, logPath, syncInterval).
If user start app in console (*dotnet run*) wihout any arguments, app will print message into console and suggest example how to start app again correctly under predefined *data* folder.

When app is started from VS Code and VS there are already predefined default input arguments to start the application. It should work either under Linux or Windows.

In the *data/source/* folder there are already predefined sample files and directories.
In the *data/replica/* folder there are also contained some files and directories. Some of them are exact copies of *source* folder, which means they will NOT be changed. Equality of files is compared by Md5 hashes.

Synchronzation (replication) itself is solved by file operations CREATE, COPY, DELETE. 
CREATE - Root directories (data/source/ and data/replica/) which are deleted will be automtically re-created.
COPY - Files or direcotries which exists in source and NOT exists in replica will be copied there.
DELETE - Files or directories which exists in *replica* but not in *source* will be removed with next replication call.

There could happen situation that copying of large files or copying many files will take long time. 
Therefore FolderReplicator.ReplicateAsync() operation (and especially the COPY operation) has been created asynchronously. It make the copy much faster in these cases. If operation is NOT finished until next repliation call will NOT block it. 

Additionally app is prepared to interrrupt/cancel any long runnnig operation. To verify this functionality there has been created unittest in separate project ReplicaTool.Tests. 
Simulation is assured using Mock object of CopyFileAsync() set to infinite timeout. 

In the projce there were use Dependency Injection principle which make the app more flexible, testable and extensible.
