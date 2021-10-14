using System;

using VisualAssist.PublisherInterface.ViewModels;

using Windows.UI.Xaml.Controls;

namespace VisualAssist.PublisherInterface.Views
{
    public sealed partial class LogInPage : Page
    {
        public LogInViewModel ViewModel { get; } = new LogInViewModel();

        public LogInPage()
        {
            InitializeComponent();
        }
    }
}
