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


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
// TODO: Fix Open File Error Alert.
// TODO: Add file filter for files.
// TODO: Refactor code to different files.
// TODO: Add regular expression.
// TODO: Add replace function.
// TODO: Implement sorting for datagrid.
// TODO: Add more file filters for pick single file.
namespace TextSpider
{
    public partial class MainWindow : Window
    {
        MainViewModel BindingContext { get; set; }
        FileService FileService { get; set; }
        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "TextSpider";
            this.BindingContext = new MainViewModel();

            FileService = new FileService();
        }

        private void HandleInputOptionChange(object sender, RoutedEventArgs e)
        {
            if (FolderInputGrid == null || FileInputGrid == null) return;

            RadioButton radioButton = (RadioButton)sender;
            string selectedOption = radioButton.Name.ToString();

            if (selectedOption == "InputFromFolder")
            {
                FolderInputGrid.Visibility = Visibility.Visible;
                FileInputGrid.Visibility = Visibility.Collapsed;
            }
            else if (selectedOption == "InputFromFile")
            {
                FolderInputGrid.Visibility = Visibility.Collapsed;
                FileInputGrid.Visibility = Visibility.Visible;
            }
        }

        private async void LoadFileFromFileExplorer(object sender, RoutedEventArgs e)
        {
            var hwnd = this.As<IWindowNative>().WindowHandle;   
            StorageFile file = await FileService.PickSingleFileAsync(hwnd);
            if (file == null) return; 
            BindingContext.InputFilePath = file.Path;
        }

        private async void LoadFolderFromFileExplorer(object sender, RoutedEventArgs e)
        {
            var hwnd = this.As<IWindowNative>().WindowHandle;
            StorageFolder folder = await FileService.PickSingleFolderAsync(hwnd);
            if (folder == null) return;
            BindingContext.InputFilePath = folder.Path;
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
                    await FindValueInFile(file, BindingContext.FindValue);
                }
                if (BindingContext.SearchResults.Count == 1)
                {
                    ResultsRichEditBox.Document.SetText(TextSetOptions.FormatRtf, BindingContext.SearchResults[0].Results);
                }
            }
            catch (Exception ex)
            {
                ContentDialog InvalidFilePathDialog = new ContentDialog()
                {
                    Title = "Invalid File Path",
                    Content = "File is not found at current file path. Please select a new one and try again.",
                    CloseButtonText = "Ok",
                    XamlRoot = ResultsRichEditBox.XamlRoot
                };

                await InvalidFilePathDialog.ShowAsync();
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

        private async void ReplaceValueInFilePath(object sender, RoutedEventArgs e)
        {

        }

        #region Helpers
        private async Task GetFilesFromFolder(StorageFolder folder)
        {
            var files = await folder.GetFilesAsync();
            foreach (var file in files)
            {
                await FindValueInFile(file, BindingContext.FindValue);
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
            str.AppendLine(@"{\rtf1\ansi\deff0{\colortbl;\red0\green0\blue0;\red255\green255\blue0;\red255\green0\blue0;}");
            int matches = 0;
            foreach (string line in lines)
            {
                if (!line.Contains(value) || value == "") continue;
                matches++;
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
        #endregion
    }

    [ComImport]
    [Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInitializeWithWindow
    {
        void Initialize(IntPtr hwnd);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
    internal interface IWindowNative
    {
        IntPtr WindowHandle { get; }
    }
}
