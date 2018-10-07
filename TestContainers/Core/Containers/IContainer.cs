using System.Threading.Tasks;

namespace TestContainers.Core.Containers
{
    public interface IContainer
    {
        void SetImage(string image);

        void AddExposedPort(int port);

        void AddPortBinding(int hostPort, int containerPort);

        void AddEnv(string key, string value);

        void AddLabel(string key, string value);

        void AddMountPoint(string sourcePath, string targetPath, string type);

        void SetCommand(string cmd);

        int GetMappedPort(int originalPort);

        Task StartAsync();

        Task StopAsync();
    }
}
