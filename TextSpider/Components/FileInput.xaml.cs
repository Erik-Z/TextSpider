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
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using TextSpider.Services;
using TextSpider.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using WinRT;
using ABI.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TextSpider.Components
{
    public sealed partial class FileInput : UserControl
    {
        public MainViewModel BindingContext;
        public MainWindow window;
        FileService FileService;
        
        public FileInput()
        {
            this.InitializeComponent();
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
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            StorageFile file = await FileService.PickSingleFileAsync(hwnd);
            if (file == null) return;
            BindingContext.InputFilePath = file.Path;
        }

        private async void LoadFolderFromFileExplorer(object sender, RoutedEventArgs e)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            StorageFolder folder = await FileService.PickSingleFolderAsync(hwnd);
            if (folder == null) return;
            BindingContext.InputFilePath = folder.Path;
        }
    }
}
