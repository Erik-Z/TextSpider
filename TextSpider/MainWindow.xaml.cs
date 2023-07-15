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
using Windows.Storage.Streams;
using TextSpider.ViewModels;
using static System.Net.Mime.MediaTypeNames;
using System.ComponentModel;
using Windows.Storage.FileProperties;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TextSpider
{
    public sealed partial class MainWindow : Window
    {
        MainViewModel BindingContext { get; set; }
        List<FileInformation> SearchResults = new List<FileInformation>();
        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "TextSpider";
            this.BindingContext = new MainViewModel();
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
            var filePicker = new FileOpenPicker();

            var hwnd = this.As<IWindowNative>().WindowHandle;

            var initializeWithWindow = filePicker.As<IInitializeWithWindow>();
            initializeWithWindow.Initialize(hwnd);
            filePicker.FileTypeFilter.Add("*");
            filePicker.FileTypeFilter.Add(".txt");
            filePicker.ViewMode = PickerViewMode.List;
            filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            
            StorageFile file = await filePicker.PickSingleFileAsync();
            if (file == null) return; 
            BindingContext.InputFilePath = file.Path;
        }

        private async void LoadFolderFromFileExplorer(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();

            var hwnd = this.As<IWindowNative>().WindowHandle;

            var initializeWithWindow = folderPicker.As<IInitializeWithWindow>();
            initializeWithWindow.Initialize(hwnd);
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder == null) return;
            BindingContext.InputFilePath = folder.Path;
        }

        private async void FindValueInFilePath(object sender, RoutedEventArgs e)
        {
            try
            {
                SearchResults.Clear();
                if (IsFolderPath(BindingContext.InputFilePath))
                {
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(BindingContext.InputFilePath);
                    await GetFilesFromFolder(folder);

                } else
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(BindingContext.InputFilePath);
                    await FindValueInFile(file, BindingContext.FindValue);
                }
                if (SearchResults.Count == 1)
                {
                    ResultsRichEditBox.Document.SetText(TextSetOptions.FormatRtf, SearchResults[0].Results);
                }
                Console.WriteLine(SearchResults.ToString());
            }
            catch (Exception ex)
            {
                ContentDialog noWifiDialog = new ContentDialog()
                {
                    Title = "No wifi connection",
                    Content = "Check connection and try again.",
                    CloseButtonText = "Ok"
                };

                await noWifiDialog.ShowAsync();
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
                FileSize = FormatFileSize(properties.Size),
                Created = properties.ItemDate,
                Modified = properties.DateModified,
                Attributes = file.Attributes.ToString(),
                Results = str.ToString()
            };
            SearchResults.Add(fileInformation);
            // ResultsRichEditBox.Document.SetText(TextSetOptions.FormatRtf, str.ToString());
        }

        public string FormatFileSize(ulong fileSize)
        {
            string[] sizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

            if (fileSize == 0)
                return "0 " + sizeSuffixes[0];

            int suffixIndex = (int)(Math.Log(fileSize, 1024));
            double normalizedSize = fileSize / Math.Pow(1024, suffixIndex);
            string formattedSize = $"{normalizedSize:0.##} {sizeSuffixes[suffixIndex]}";

            return formattedSize;
        }

        public bool IsFolderPath(string path)
        {
            System.IO.FileAttributes attributes = File.GetAttributes(path);
            return (attributes & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory;
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
