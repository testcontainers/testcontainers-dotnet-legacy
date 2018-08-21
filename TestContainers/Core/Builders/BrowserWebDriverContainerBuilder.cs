using TestContainers.Core.Containers;

namespace TestContainers.Core.Builders
{
    public class BrowserWebDriverContainerBuilder<TBrowserWebDriverContainer> : ContainerBuilder<TBrowserWebDriverContainer, BrowserWebDriverContainerBuilder<TBrowserWebDriverContainer>>
    where TBrowserWebDriverContainer : BrowserWebDriverContainer, new()
    {
        public BrowserWebDriverContainerBuilder<TBrowserWebDriverContainer> WithRecordingMode(VncRecordingMode recordingMode)
        {
            fn = FnUtils.Compose(fn, (container) =>
            {
                container.VncRecordingMode = recordingMode;
                return container;
            });

            return this;
        }
    }

    public enum VncRecordingMode
    {
        RECORD_ALL
    }
}
