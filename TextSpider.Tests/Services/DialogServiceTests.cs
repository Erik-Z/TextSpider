using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextSpider.Services;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium;
using System.Threading;
using System;
using TextSpider.Tests;

namespace TextSpider.Services.Tests
{
    [TestClass()]
    public class DialogServiceTests
    {
        private static WindowsElement dialog = null;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Test_Base.Setup(context);
            var filePickerButton = Test_Base.session.FindElementByName("OpenFileButton");
            filePickerButton.Click();
        }

        [TestMethod()]
        public void ShowDialogAsyncTest()
        {
            
        }
    }
}