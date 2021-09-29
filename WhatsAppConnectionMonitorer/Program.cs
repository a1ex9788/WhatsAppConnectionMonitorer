using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading.Tasks;

namespace WhatsAppConnectionMonitorer
{
    public class Program
    {
        public async static Task Main()
        {
            IWebDriver browser = null;

            try
            {
                // TODO: Investigate the shown errors.
                browser = new ChromeDriver()
                {
                    Url = "https://web.whatsapp.com/",
                };

                Console.WriteLine("WhatsAppConnectionMonitorer started.");

                //browser.Manage().Window.Maximize();

                Console.WriteLine("Waiting for sing in...");

                await WaitForSingIn(browser);

                Console.WriteLine("Sing in completed successfully. Starting monitoring the connection...");

                MonitorConnection(browser);
            }
            catch (Exception e)
            {
                // TODO: Detect when the browser has been closed.
                Console.Error.WriteLine($"An unexpected error occurred: {e.Message}");
            }
            finally
            {
                browser?.Dispose();
            }
        }

        private static async Task WaitForSingIn(IWebDriver browser)
        {
            bool mainPageReached = false;
            string lastPageSource = null;

            do
            {
                string currentPageSource = browser.PageSource;

                if (lastPageSource != currentPageSource)
                {
                    try
                    {
                        IWebElement chatButton = browser.FindElement(By.XPath(".//*[@class='_26lC3']"));

                        mainPageReached = true;

                        break;
                    }
                    catch
                    {
                        Console.Error.WriteLine("Main page not reached yet.");
                    }

                    lastPageSource = currentPageSource;
                }

                await Task.Delay(1000);
            }
            while (!mainPageReached);
        }

        private static void MonitorConnection(IWebDriver browser)
        {
            // TODO: Extract the name of the contact to a configuration file.
            string contactToMonitor = "AaMama";

            EnterIntoContactToMonitorChat(browser, contactToMonitor);

            int failsCount = 0, maxFailsCount = 3;

            while (true)
            {
                try
                {
                    IWebElement lastConnectionLabel = browser.FindElement(By.XPath(".//*[@class='_2YPr_ i0jNr selectable-text copyable-text']"));

                    while (true)
                    {
                        Console.WriteLine(lastConnectionLabel.Text);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"The connection status of the contact '{contactToMonitor}' could not be retrieved. Retrying {++failsCount}/{maxFailsCount}");

                    if (failsCount == maxFailsCount)
                    {
                        throw new Exception($"The connection status of the contact '{contactToMonitor}' could not be definitely retrieved.", e);
                    }
                }
            }
        }

        private static void EnterIntoContactToMonitorChat(IWebDriver browser, string contactToMonitor)
        {
            try
            {
                IWebElement chatButton = browser.FindElement(By.XPath($".//*[@title='{contactToMonitor}']"));

                chatButton.Click();
            }
            catch (Exception e)
            {
                throw new Exception($"The contact '{contactToMonitor}' was not found.", e);
            }
        }
    }
}