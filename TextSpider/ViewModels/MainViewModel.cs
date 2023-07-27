using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using TextSpider.Models;
using TextSpider.Services;

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

        private ObservableCollection<FileInformation> _searchResults = new ObservableCollection<FileInformation>();
        public ObservableCollection<FileInformation> SearchResults
        {
            get { return _searchResults; }
            set
            {
                if (_searchResults != value)
                {
                    _searchResults = value;
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
