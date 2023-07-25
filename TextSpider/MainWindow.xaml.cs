using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using WinRT;
using System.Runtime.InteropServices;
using System.Text;
using TextSpider.ViewModels;
using Windows.Storage.FileProperties;
using CommunityToolkit.WinUI.UI.Controls;
using TextSpider.Utility;
using Windows.UI.Core;
using TextSpider.Services;
using TextSpider.Models;
using TextSpider.Interfaces;
using TextSpider.Components;
using System.Text.RegularExpressions;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
// TODO: Add file filter for files.
// TODO: Refactor code to different files.
// TODO: Add error handling for null find value or null regex.
// TODO: Add replace function.
// TODO: Implement sorting for datagrid.
// TODO: Add more file filters for pick single file.
namespace TextSpider
{
    public partial class MainWindow : Window
    {
        MainViewModel BindingContext { get; set; }
        FileService FileService { get; set; }
        private IDialogService DialogService;
        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "TextSpider";
            
            this.BindingContext = new MainViewModel();
            Menu.BindingContext = BindingContext;
            Sidebar.BindingContext = BindingContext;
            FileInput.BindingContext = BindingContext;
            FileInput.window = this;

            FileService = new FileService();
        }
        private void HandleMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            DialogService = new DialogService(MainWindowGrid.XamlRoot);
        }

        private async void FindValueInFilePath(object sender, RoutedEventArgs e)
        {
            try
            {
                BindingContext.SearchResults.Clear();
                if (FileHelper.IsFolderPath(BindingContext.InputFilePath))
                {
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(BindingContext.InputFilePath);
                    await GetFilesFromFolder(folder);

                } else
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(BindingContext.InputFilePath);
                    if (FindReplaceViewModel.Instance.IsFindByRegex)
                    {
                        await MatchRegexPatternInFile(file, new Regex(FindReplaceViewModel.Instance.RegexValue));
                    }
                    else
                    {
                        await FindValueInFile(file, FindReplaceViewModel.Instance.FindValue);
                    }
                }
                if (BindingContext.SearchResults.Count == 1)
                {
                    ResultsRichEditBox.Document.SetText(TextSetOptions.FormatRtf, BindingContext.SearchResults[0].Results);
                }
            }
            catch (Exception)
            {
                await DialogService.ShowDialogAsync(
                    "Invalid File Path",
                    "File is not found at current file path. Please select a new one and try again.",
                    "Ok");
            }
        }

        private void HandleSelectedResultChange(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                FileInformation selectedItem = (FileInformation)dataGrid.SelectedItem;
                if (selectedItem == null) return;
                ResultsRichEditBox.Document.SetText(TextSetOptions.FormatRtf, selectedItem.Results);
            }
        }

        private void ReplaceValueInFilePath(object sender, RoutedEventArgs e)
        {

        }

        #region Helpers
        private async Task GetFilesFromFolder(StorageFolder folder)
        {
            var files = await folder.GetFilesAsync();
            foreach (var file in files)
            {
                if (FindReplaceViewModel.Instance.IsFindByRegex)
                {
                    await MatchRegexPatternInFile(file, new Regex(FindReplaceViewModel.Instance.RegexValue));
                } else
                {
                    await FindValueInFile(file, FindReplaceViewModel.Instance.FindValue);
                }
                
            }
            foreach (StorageFolder subfolder in await folder.GetFoldersAsync())
            {
                await GetFilesFromFolder(subfolder);
            }
        }

        private async Task FindValueInFile(StorageFile file, string value)
        {
            IList<string> lines = await FileIO.ReadLinesAsync(file);
            StringBuilder str = new StringBuilder();
            int totalLines = lines.Count;
            int maxDigits = totalLines.ToString().Length;
            str.AppendLine(@"{\rtf1\ansi\deff0\tqr\tx" + (maxDigits * 10).ToString() + @"{\colortbl;\red0\green0\blue0;\red255\green255\blue0;\red255\green0\blue0;\red192\green192\blue192;}");

            int matches = 0;
            
            for (int i = 0; i < totalLines; i++)
            {
                string line = lines[i];
                if (!line.Contains(value) || value == "") continue;
                matches++;

                str.Append(@"\tab\cf1\highlight0 ");
                str.Append((i + 1).ToString());
                str.Append("\t");

                int start = 0;
                int found = -1;
                while ((found = line.IndexOf(value, start)) != -1)
                {
                    str.Append(line.Substring(start, found - start));
                    str.Append(@"\cf3\highlight2 ");
                    str.Append(line.Substring(found, value.Length));
                    str.Append(@"\cf1\highlight0 ");
                    start = found + value.Length;
                }
                str.Append(line.Substring(start));
                str.AppendLine(@"\par");
            }
            str.AppendLine("}");

            BasicProperties properties = await file.GetBasicPropertiesAsync();
            FileInformation fileInformation = new FileInformation
            {
                FileName = file.Name,
                FilePath = file.Path,
                FileType = file.DisplayType,
                Matches = matches,
                FileSize = FileHelper.FormatFileSize(properties.Size),
                Created = properties.ItemDate,
                Modified = properties.DateModified,
                Attributes = file.Attributes.ToString(),
                Results = str.ToString()
            };
            BindingContext.SearchResults.Add(fileInformation);
        }

        private async Task MatchRegexPatternInFile(StorageFile file, Regex pattern)
        {
            IList<string> lines = await FileIO.ReadLinesAsync(file);
            StringBuilder str = new StringBuilder();
            int totalLines = lines.Count;
            int maxDigits = totalLines.ToString().Length;
            str.AppendLine(@"{\rtf1\ansi\deff0\tqr\tx" + (maxDigits * 10).ToString() + @"{\colortbl;\red0\green0\blue0;\red255\green255\blue0;\red255\green0\blue0;\red192\green192\blue192;}");

            int matches = 0;

            for (int i = 0; i < totalLines; i++)
            {
                string line = lines[i];
                MatchCollection matchCollection = pattern.Matches(line);

                if (matchCollection.Count > 0)
                {
                    matches += matchCollection.Count;

                    str.Append(@"\tab\cf1\highlight0 ");
                    str.Append((i + 1).ToString());
                    str.Append("\t");

                    int start = 0;
                    foreach (Match match in matchCollection)
                    {
                        str.Append(line.Substring(start, match.Index - start));
                        str.Append(@"\cf3\highlight2 ");
                        str.Append(line.Substring(match.Index, match.Length));
                        str.Append(@"\cf1\highlight0 ");
                        start = match.Index + match.Length;
                    }
                    str.Append(line.Substring(start));
                    str.AppendLine(@"\par");
                }
            }
            str.AppendLine("}");

            BasicProperties properties = await file.GetBasicPropertiesAsync();
            FileInformation fileInformation = new FileInformation
            {
                FileName = file.Name,
                FilePath = file.Path,
                FileType = file.DisplayType,
                Matches = matches,
                FileSize = FileHelper.FormatFileSize(properties.Size),
                Created = properties.ItemDate,
                Modified = properties.DateModified,
                Attributes = file.Attributes.ToString(),
                Results = str.ToString()
            };
            BindingContext.SearchResults.Add(fileInformation);
        }
        #endregion
    }
}
