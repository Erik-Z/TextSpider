using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;

namespace TextSpider.Tests
{
    public class Test_Base
    {
        private const string WindowsApplicationDriverUrl = "http://127.0.0.1:4723";
        public static WindowsDriver<WindowsElement>? session = null;

        public static void Setup(TestContext context)
        {

            if (session == null)
            {
                AppiumOptions appCapabilities = new AppiumOptions();
                appCapabilities.AddAdditionalCapability("app", @"C:\Users\erikz\Desktop\Development\C#\WinUI\TextSpider\TextSpider\bin\Debug\net6.0-windows10.0.19041.0\win10-x86\publish\TextSpider.exe");
                appCapabilities.AddAdditionalCapability("deviceName", "WindowsPC");
                session = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), appCapabilities);
                if (session == null)
                {
                    session = new WindowsDriver<WindowsElement>(new Uri(WindowsApplicationDriverUrl), appCapabilities);
                }
                Assert.IsNotNull(session);
                Assert.IsNotNull(session.SessionId);
                
            }
        }

        public static void TearDown()
        {
            if (session != null)
            {
                session.Quit();
                session = null;
            }
        }
    }
}
