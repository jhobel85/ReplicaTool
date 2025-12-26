# Replica Tool

**Assignment**

The program should maintain a full, identical copy of source

folder at replica folder. Synchronization must be one-way: after the synchronization content of the replica folder should be modified to exactly match content of the sourc folder;

\[x] Synchronization should be performed periodically;

\[x] File creation/copying/removal operations should be logged to a file and to the console output;

\[x] Folder paths, synchronization interval and log file path should be provided using the command line arguments;

\[x] It is undesirable to use third-party libraries that implement folder synchronization;

\[x] It is recommended to use external libraries implementing other well-known algorithms. For example, there is no point in implementing yet  another function that calculates MD5 if you need it for the task – it is perfectly acceptable to use a third-party (or built-in) library.



**Requirements**

* Linux or Windows
* VisualStudio Code or VisualStudio
* .NET 8.0
* C# Dev Kit
* Git, Github
* .Nuget Packages: Serilog, System.IO.Hashing, XUnit, Moq



**Brief overview of console application *ReplicaTool***

App requries input arguments (source, replice, logPath, syncInterval).
If user start app in console (*dotnet run*) wihout any arguments, app will print message into console and suggest example how to start app again correctly under predefined *data* folder.

When app is started from VS Code and VS there are already predefined default input arguments to start the application. It should work either under Linux or Windows.

In the *data/source/* folder there are already predefined sample files and directories.
In the *data/replica/* folder there are also contained some files and directories. Some of them are exact copies of *source* folder, which means they will NOT be changed. Equality of files is compared by Addaptive file comparer which can decide best performance option either for small or large files (uses caching mechanism).

Synchronzation (replication) itself is solved by file operations CREATE, COPY, DELETE.
CREATE - Root directories (data/source/ and data/replica/) which are deleted will be automtically re-created.
COPY - Files or direcotries which exists in source and NOT exists in replica will be copied there.
DELETE - Files or directories which exists in *replica* but not in *source* will be removed with next replication call.

There could happen situation that copying of large files or copying many files will take long time.
Therefore FolderReplicator.ReplicateAsync() operation (and especially the COPY operation) has been created asynchronously. It make the copy much faster in these cases. If operation is NOT finished until next repliation call will NOT block it.

Additionally app is prepared to interrrupt/cancel any long runnnig operation. To verify this functionality there has been created unittest in separate project ReplicaTool.Tests.
Simulation is assured using Mock object of CopyFileAsync() set to infinite timeout.

In the projce there were use Dependency Injection principle which make the app more flexible, testable and extensible.

