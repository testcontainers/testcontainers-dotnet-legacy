using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using Polly;
using System;
using System.Threading.Tasks;
using TestContainers.Core.Builders;

namespace TestContainers.Core.Containers
{
    public class BrowserWebDriverContainer : Container
    {
        const int SELENIUM_PORT = 4444;
        const int VNC_PORT = 5900;

        RemoteWebDriver Driver;

        public VncRecordingMode VncRecordingMode { get; set; }

        public string GetSeleniumAddress() => "http://" + GetDockerHostIpAddress() + ":" + SELENIUM_PORT + "/wd/hub";

        public string GetVncAddress() => "vnc://" + GetDockerHostIpAddress() + ":" + VNC_PORT;

        public RemoteWebDriver GetWebDriver() => Driver;

        protected override async Task WaitUntilContainerStarted()
        {
            await base.WaitUntilContainerStarted();

            var options = new ChromeOptions();
            options.AddArgument("--disable-plugins");

            var result = Policy
                .Timeout(TimeSpan.FromMinutes(2))
                .Wrap(Policy
                    .Handle<Exception>()
                    .WaitAndRetryForever(
                        iteration => TimeSpan.FromSeconds(10)))
                .ExecuteAndCapture(() =>
                {
                    Driver = new RemoteWebDriver(new Uri(GetSeleniumAddress()), options.ToCapabilities());
                });

            if (result.Outcome == OutcomeType.Failure)
            {
                throw new Exception(result.FinalException.Message);
            }
        }
    }
}
