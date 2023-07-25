using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TextSpider.ViewModels
{
    internal partial class RadioButtonViewModel : ObservableObject
    {
        private static RadioButtonViewModel instance = null;
        private static readonly object padlock = new object();

        public static RadioButtonViewModel Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new RadioButtonViewModel();
                    }
                    return instance;
                }
            }
        }

        [ObservableProperty]
        private bool _isFindByRegex = false;

        [ObservableProperty]
        private bool _isNotFindByRegex = true;

    }
}
