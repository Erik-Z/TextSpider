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

        private Regex _regexValue = new Regex("([A-Z])\\w+");
        public string RegexValue
        {
            get { return _regexValue.ToString(); }
            set
            {
                try { 
                    if (_regexValue.ToString() != value)
                    {
                        _regexValue = new Regex(value);
                        OnPropertyChanged();
                    }
                } catch(Exception) 
                {
                    return;
                }
            }
        }

        public Regex GetRegexValue()
        {
            return _regexValue;
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

        private bool _isFindByRegex;
        public bool IsFindByRegex
        {
            get { return _isFindByRegex; }
            set
            {
                if (_isFindByRegex != value)
                {
                    _isFindByRegex = value;
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
