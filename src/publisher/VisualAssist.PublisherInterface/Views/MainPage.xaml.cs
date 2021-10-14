using System;

using VisualAssist.PublisherInterface.ViewModels;

using Windows.UI.Xaml.Controls;

namespace VisualAssist.PublisherInterface.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainPage()
        {
            InitializeComponent();
        }
    }
}
