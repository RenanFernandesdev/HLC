using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using System;
using System.Collections.Generic;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace HLC.DriverComponents
{
    sealed class SeleniumConfig
    {
        public SeleniumConfig() {
            new DriverManager().SetUpDriver(new ChromeConfig());
        }

        public IWebDriver StartDriver()
        {   ChromeDriverService driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            ChromeDriver driver = new ChromeDriver(driverService, (ChromeOptions)ConfigDriver());
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(180);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(180);
            return driver;
        }

        public DriverOptions ConfigDriver()
        {
            ChromeOptions driverOptions = new ChromeOptions();
            driverOptions.AcceptInsecureCertificates = true;
            List<string> argList = new List<string>
            {
                "no-sandbox",
                "--window-size=400,360",
                "disable-javascript",
                "disable-infobars",
                "disable-extensions",
                "disable-translate",
                "disable-notifications",
                "disable-extensions",
                "disable-media-source",
                "disable-gpu",

            };
            driverOptions.AddArguments(argList);
            
            return driverOptions;
        }
    }
}
