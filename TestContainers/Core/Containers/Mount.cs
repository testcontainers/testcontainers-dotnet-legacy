namespace TestContainers.Core.Containers
{
    public class Mount
    {
        public string SourcePath { get; set; }

        public string TargetPath { get; set; }

        public string Type { get; set; }

        public Mount(string sourcePath, string targetPath, string type)
        {
            SourcePath = sourcePath;
            TargetPath = targetPath;
            Type = type;
        }
    }
}
