using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TextSpider.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _inputFilePath = "";
        public string InputFilePath
        {
            get { return _inputFilePath; }
            set
            {
                if (_inputFilePath != value)
                {
                    _inputFilePath = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _findValue;
        public string FindValue
        {
            get { return _findValue; }
            set
            {
                if (_findValue != value)
                {
                    _findValue = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _replaceValue;
        public string ReplaceValue
        {
            get { return _replaceValue; }
            set
            {
                if (_replaceValue != value)
                {
                    _replaceValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
