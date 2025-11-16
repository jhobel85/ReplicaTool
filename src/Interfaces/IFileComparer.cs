namespace ReplicaTool.Interfaces
{
    public interface IFileComparer
    {
        /**
         * Return false when files differ 
         * Return true when files are equal (so caller should copy)
         */
        bool AreFilesEqual(string sourcePath, string replicaPath);
    }
}
