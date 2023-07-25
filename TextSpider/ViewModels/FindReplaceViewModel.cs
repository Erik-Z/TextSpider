using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TextSpider.ViewModels
{
    internal partial class FindReplaceViewModel : ObservableObject
    {
        private static FindReplaceViewModel instance = null;
        private static readonly object padlock = new object();

        public static FindReplaceViewModel Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new FindReplaceViewModel();
                    }
                    return instance;
                }
            }
        }

        [ObservableProperty]
        private bool _isFindByRegex = false;

        [ObservableProperty]
        private bool _isNotFindByRegex = true;

        [ObservableProperty]
        private string _findValue = "";

        [ObservableProperty]
        private string _regexValue = new Regex("([A-Z])\\w+").ToString();

        [ObservableProperty]
        private string _replaceValue = "";

    }
}
