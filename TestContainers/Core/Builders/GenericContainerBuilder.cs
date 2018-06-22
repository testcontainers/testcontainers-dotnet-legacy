using TestContainers.Core.Containers;

namespace TestContainers.Core.Builders
{
    public class GenericContainerBuilder<TContainer> : ContainerBuilder<TContainer, GenericContainerBuilder<TContainer>> where TContainer : Container ,new()
    {
        
    }
}