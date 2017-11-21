using System.Windows;
using CornellBox.ViewModels;

namespace CornellBox.Views
{
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; } =new MainWindowViewModel();

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
