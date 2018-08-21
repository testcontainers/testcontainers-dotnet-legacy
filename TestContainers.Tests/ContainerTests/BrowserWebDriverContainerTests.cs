using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Threading.Tasks;
using TestContainers.Core.Builders;
using TestContainers.Core.Containers;
using Xunit;

namespace TestContainers.Tests.ContainerTests
{
    public class BrowserWebDriverFixture : IAsyncLifetime
    {
        BrowserWebDriverContainer Container { get; }

        public RemoteWebDriver Driver => Container.GetWebDriver();

        public BrowserWebDriverFixture()
        {
            Container = new BrowserWebDriverContainerBuilder<BrowserWebDriverContainer>()
                .Begin()
                .WithImage("selenium/standalone-chrome-debug:3.14.0-arsenic")
                .WithExposedPorts(4444, 5900)
                .WithEnv(("VNC_NO_PASSWORD", "1"))
                .Build();
        }
        public Task DisposeAsync() => Container.Stop();

        public Task InitializeAsync() => Container.Start();
    }

    public class BrowserWebDriverContainerTests : IClassFixture<BrowserWebDriverFixture>
    {
        RemoteWebDriver Driver { get; }
        public BrowserWebDriverContainerTests(BrowserWebDriverFixture fixture)
        {
            Driver = fixture.Driver;
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public void SimpleTest()
        {
            IWebElement findElementQ() => Driver.FindElement(By.Name("q"));

            Driver.Navigate().GoToUrl("http://www.google.com");

            findElementQ().SendKeys("testcontainers");
            findElementQ().Submit();

            Assert.Equal("testcontainers", findElementQ().GetAttribute("value"));
        }
    }
}
