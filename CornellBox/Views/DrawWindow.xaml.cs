using System.Windows;
using CornellBox.Scenes;
using CornellBox.ViewModels;

namespace CornellBox.Views
{
    /// <summary>
    /// Interaction logic for DrawWindow.xaml
    /// </summary>
    public partial class DrawWindow : Window
    {
        public DrawWindowViewModel ViewModel { get; }

        public DrawWindow(Scene scene, int imgSize)
        {
            ViewModel = new DrawWindowViewModel(scene, imgSize);
            InitializeComponent();

            KeyDown += ViewModel.OnKeyDown;
            Closed += (sender, args) => ViewModel.CancelDrawCommand.Execute(null);
        }
    }
}
