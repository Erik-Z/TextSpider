using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT;

namespace TextSpider.Services
{
    internal class FileService
    {
        public async Task<StorageFile> PickSingleFileAsync(IntPtr hwnd)
        {
            var filePicker = new FileOpenPicker();
            var initializeWithWindow = filePicker.As<IInitializeWithWindow>();
            initializeWithWindow.Initialize(hwnd);
            filePicker.FileTypeFilter.Add("*");
            filePicker.FileTypeFilter.Add(".txt");
            filePicker.ViewMode = PickerViewMode.List;
            filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            return await filePicker.PickSingleFileAsync();
        }

        public async Task<StorageFolder> PickSingleFolderAsync(IntPtr hwnd)
        {
            var folderPicker = new FolderPicker();
            var initializeWithWindow = folderPicker.As<IInitializeWithWindow>();
            initializeWithWindow.Initialize(hwnd);
            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;

            return await folderPicker.PickSingleFolderAsync();
        }
    }
}
