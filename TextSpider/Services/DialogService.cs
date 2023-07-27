using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextSpider.Interfaces;

namespace TextSpider.Services
{
    public class DialogService : IDialogService
    {
        private ContentDialog dialog;
        private XamlRoot xamlRoot;

        public DialogService(XamlRoot root)
        {
            xamlRoot = root;
        }

        public async Task ShowDialogAsync(string title, string message, string closeButtonText)
        {
            dialog = new ContentDialog()
            {
                Title = title,
                Content = message,
                CloseButtonText = closeButtonText,
                XamlRoot = xamlRoot
            };

            await dialog.ShowAsync();
        }

        public async Task ShowConfirmationDialogAsync(string title, string message, string confirmButtonText, 
            Action confirmationAction)
        {
            ContentDialog deleteFileDialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = confirmButtonText,
                CloseButtonText = "Cancel",
                XamlRoot = xamlRoot
            };

            ContentDialogResult result = await deleteFileDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                confirmationAction();
            }
        }
    }
}
