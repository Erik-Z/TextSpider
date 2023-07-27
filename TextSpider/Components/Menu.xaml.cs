using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using TextSpider.ViewModels;
using TextSpider.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TextSpider.Components
{
    public sealed partial class Menu : UserControl
    {
        public MainViewModel BindingContext { get; set; }
        DialogService DialogService { get; set; }
        public Menu()
        {
            this.InitializeComponent();
        }

        private void HandleMenuLoaded(object sender, RoutedEventArgs e)
        {
            DialogService = new DialogService(MainMenu.XamlRoot);
        }

        private async void HandleExitClicked(object sender, RoutedEventArgs e)
        {
            Action closeApplication = () => Application.Current.Exit();
            await DialogService.ShowConfirmationDialogAsync("Warning", 
                "Are you sure you want to exit? All unsaved changes will be deleted.",
                "Yes", closeApplication);
        }
    }
}
