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
using Windows.Foundation.Diagnostics;
using Windows.Storage.Streams;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
// TODO: Handle find for unicode and other encodings
// TODO: Add file filter for files.
// TODO: Refactor code to different files.
// TODO: Add error handling for null find value or null regex.
// TODO: Add error handling for null replace.
// TODO: Implement sorting for datagrid.
// TODO: Add more file filters for pick single file.
// TODO: Implement multiple selected. (This includes replacing values at the same time.)
// TODO: Do a find then replace if only replace is clicked. (Find is not clicked beforehand)
// TODO: Fix how it handles the content in rich text box after replacement.
// TODO: Make file replacement multi process.
// TODO: Automatically select first file after Find on folder.
// TODO: In folder filter, allow users to create and delete file types.
// TODO: Add unit tests.
// TODO: Auto select in datagrid when find happens. Just use selected when replacing.

namespace TextSpider
{
    public partial class MainWindow : Window
    {
        MainViewModel BindingContext { get; set; }
        private DialogService DialogService;
        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "TextSpider";
            
            this.BindingContext = new MainViewModel();
            Menu.BindingContext = BindingContext;
            Sidebar.BindingContext = BindingContext;
            FileInput.BindingContext = BindingContext;
            FileInput.window = this;
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
                        await FindValueInFile(file, new Regex(FindReplaceViewModel.Instance.RegexValue));
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
            catch (Exception ex)
            {
                await DialogService.ShowDialogAsync(
                    "Invalid File Path",
                    "File is not found at current file path. Please select a new one and try again.",
                    "Ok");
            }
        }

        private void HandleSelectedResultChange(object sender, RoutedEventArgs e)
        {
            if (sender is not DataGrid dataGrid) return;
            
            FileInformation selectedItem = (FileInformation)dataGrid.SelectedItem;
            if (selectedItem == null) return;
            ResultsRichEditBox.Document.SetText(TextSetOptions.FormatRtf, selectedItem.Results);
        }

        private async void ReplaceValueInFilePath(object sender, RoutedEventArgs e)
        {
            try { 
                string originalValue = FindReplaceViewModel.Instance.IsFindByRegex ? 
                    FindReplaceViewModel.Instance.RegexValue : FindReplaceViewModel.Instance.FindValue;

                foreach (FileInformation fileInfo in ResultsDataGrid.SelectedItems.Count == 0 ? ResultsDataGrid.ItemsSource : ResultsDataGrid.SelectedItems)
                {
                    StorageFile file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(fileInfo.AccessStorageToken);
                    StorageFile backupFile = await file.CopyAsync(await file.GetParentAsync(), file.Name + ".backup", NameCollisionOption.ReplaceExisting);

                    string text = await FileIO.ReadTextAsync(file);
                    if (FindReplaceViewModel.Instance.IsFindByRegex)
                    {
                        text = new Regex(originalValue).Replace(text, FindReplaceViewModel.Instance.ReplaceValue);
                        UpdateResultsWithNewValue(new Regex(originalValue), FindReplaceViewModel.Instance.ReplaceValue);
                    }
                    else
                    {
                        text = text.Replace(originalValue, FindReplaceViewModel.Instance.ReplaceValue);
                        UpdateResultsWithNewValue(originalValue, FindReplaceViewModel.Instance.ReplaceValue);
                    }

                    try
                    {
                        await FileIO.WriteTextAsync(file, text);
                    } 
                    catch (Exception)
                    {
                        await backupFile.CopyAsync(await file.GetParentAsync(), file.Name, NameCollisionOption.ReplaceExisting);
                        await DialogService.ShowGenericErrorDialogAsync();
                    }
                }
                await DialogService.ShowDialogAsync("Confirmation", "Successfully replaced text in selected file(s).", "Ok");
            } 
            catch (Exception)
            {
                await DialogService.ShowGenericErrorDialogAsync();
            }
        }

        #region Helpers
        private async Task GetFilesFromFolder(StorageFolder folder)
        {
            var files = await folder.GetFilesAsync();
            foreach (var file in files)
            {
                if (FindReplaceViewModel.Instance.IsFindByRegex)
                {
                    await FindValueInFile(file, new Regex(FindReplaceViewModel.Instance.RegexValue));
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

                str.Append(@"\tab\cf5\highlight4 ");
                str.Append((i + 1).ToString());
                str.Append("\t");
                str.Append(@"\cf1\highlight0 ");

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
                Results = str.ToString(),
                AccessStorageToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file)
        };
            BindingContext.SearchResults.Add(fileInformation);
        }

        private async Task FindValueInFile(StorageFile file, Regex pattern)
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

        private void UpdateResultsWithNewValue(string oldValue, string newValue)
        {
            foreach (FileInformation fileInfo in BindingContext.SearchResults)
            {
                fileInfo.Results = fileInfo.Results.Replace(oldValue, newValue);
            }
        }

        private void UpdateResultsWithNewValue(Regex oldRegex, string newValue)
        {
            foreach (FileInformation fileInfo in BindingContext.SearchResults)
            {
                fileInfo.Results = oldRegex.Replace(fileInfo.Results, newValue);
            }
        }
        #endregion
    }
}
