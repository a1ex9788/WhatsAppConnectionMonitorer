using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace WhatsAppConnectionMonitorer
{
    public class Program
    {
        private readonly static string executionFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        private readonly static string logsFilePath = Path.Combine(executionFolder, "logsFile.txt");

        private readonly static string contactToMonitor = ConfigurationManager.AppSettings.Get("ContactToMonitor");

        public async static Task Main()
        {
            IWebDriver browser = null;

            try
            {
                ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;

                browser = new ChromeDriver(service)
                {
                    Url = "https://web.whatsapp.com/",
                };

                //browser.Manage().Window.Maximize();

                Console.WriteLine("WhatsAppConnectionMonitorer started.");

                Console.WriteLine("Waiting for sing in...");

                await WaitForSingIn(browser);

                Console.WriteLine("Sing in completed successfully. Entering into contact to monitor chat...");

                EnterIntoContactToMonitorChat(browser);

                Console.WriteLine("Entered into contact to monitor chat. Starting monitoring the connection...");

                await MonitorConnection(browser);
            }
            catch (Exception e)
            {
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
                string currentPageSource = browser?.PageSource;

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
                        Console.Error.WriteLine("Main page not reached yet. Please, sing in.");
                    }

                    lastPageSource = currentPageSource;
                }

                await Task.Delay(1000);
            }
            while (!mainPageReached);
        }

        private static void EnterIntoContactToMonitorChat(IWebDriver browser)
        {
            try
            {
                IWebElement chatButton = browser.FindElement(By.XPath($".//*[@title='{contactToMonitor}']"));

                chatButton.Click();
            }
            catch (Exception e)
            {
                throw new Exception($"The contact '{contactToMonitor}' chat was not found.", e);
            }
        }

        private static async Task MonitorConnection(IWebDriver browser)
        {
            int failsCount = 0, maxFailsCount = 3;

            File.AppendAllText(logsFilePath, $"----- Connection monitored for {contactToMonitor} -----\n");

            while (true)
            {
                try
                {
                    IWebElement connectionStatusLabel = browser.FindElement(By.XPath(".//*[@class='_2YPr_ i0jNr selectable-text copyable-text']"));

                    while (true)
                    {
                        string connectionStatus = connectionStatusLabel.Text;
                        string connectionStatusLog = $"{DateTime.Now}:\t{connectionStatus}\n";

                        Console.Write(connectionStatusLog);

                        File.AppendAllText(logsFilePath, connectionStatusLog);

                        await Task.Delay(1000);
                    }
                }
                catch (Exception e)
                {
                    if (failsCount == maxFailsCount)
                    {
                        throw new Exception($"The connection status of the contact '{contactToMonitor}' could not be definitely retrieved.", e);
                    }

                    Console.Error.WriteLine($"The connection status of the contact '{contactToMonitor}' could not be retrieved. Retrying {++failsCount}/{maxFailsCount}");

                    await Task.Delay(500);
                }
            }
        }
    }
}