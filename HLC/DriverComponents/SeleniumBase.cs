using DocumentFormat.OpenXml.Bibliography;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HLC.DriverComponents
{
    class SeleniumBase
    {
        private IWebDriver Driver;
        private SeleniumConfig Config = new SeleniumConfig();

        public void SetUp()
        {
            Driver = Config.StartDriver();
        }

        public void Restart()
        {
            TearDown();
            SetUp();
        }

        public void Navigate(string url)
        {
            Driver.Navigate().GoToUrl(url);
        }

        public void TryNavigate(string url)
        {
            bool flag = true;
            int retry = 0;
            while (flag && retry <= 3)
            {
                    Navigate(url);
                    WaitTime(3000);
                    WaitFullLoad();
                    bool captcha = (bool)ExecuteJS("return document.querySelector('#challenge-running') === null;");
                    if (!captcha)
                    {
                        AddCookieCloudflare();
                    }
                    flag = !WaitFullLoad();
            }       
        }

        public void AddCookieCloudflare()
        {
                Cookie cookie = new Cookie(name: "cf_chl_rc_m",
                                           value: "1",
                                           domain: "hltv.org",
                                           path: "/",
                                           expiry: DateTime.Today.AddHours(1),
                                           secure: false,
                                           isHttpOnly: false,
                                           sameSite: null);
                Driver.Manage().Cookies.AddCookie(cookie);
                Driver.Navigate().Refresh();
        }

        public bool WaitFullLoad()
        {
            WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
            return wait.Until((x) =>
                {
                      return ExecuteJS("return document.readyState").Equals("complete");
                });
        }

        public object ExecuteJS(string command)
        {
            return ((IJavaScriptExecutor)Driver).ExecuteScript(command);
        }

        public IWebElement SearchByCssSelector(string cssSelector)
        {
            return Driver.FindElement(By.CssSelector(cssSelector));
        }

        public IWebElement SearchByCssSelector(string cssSelector, IWebElement element)
        {
            return element.FindElement(By.CssSelector(cssSelector));
        }

        public List<IWebElement> SearchByCssSelectorAll(string cssSelector)
        {
            return Driver.FindElements(By.CssSelector(cssSelector)).ToList();
        }

        public List<IWebElement> SearchByCssSelectorAll(string cssSelector, IWebElement element)
        {
            return element.FindElements(By.CssSelector(cssSelector)).ToList();
        }

        public string GetTextFromDOM(string parameter)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(parameter))
            {
                if (parameter.ToLower().StartsWith("javascript:"))
                {
                    result = ExecuteJS(parameter).ToString();
                }
                else
                {
                    result = SearchByCssSelector(parameter).Text;
                }
            }
            return result;
        }

        public bool IsDisplayed(string tag)
        {
            try
            {
                return SearchByCssSelector(tag).Displayed ? true : false;
            }
            catch { return false; }
        }

        public bool IsDisplayed(string tag, IWebElement element)
        {
            try
            {
                return SearchByCssSelector(tag, element).Displayed ? true : false;
            }
            catch { return false; }
        }

        public void TearDown()
        {
            if(Driver != null)
            {
                Driver.Close();
                Driver.Quit();
                Driver.Dispose();
            }
        }

        public void WaitTime(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }

    }
}
