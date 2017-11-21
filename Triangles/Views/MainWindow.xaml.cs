using System.Numerics;
using System.Windows;
using System.Windows.Input;
using Triangles.Models;
using Triangles.ViewModel;

namespace Triangles.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; } = new MainWindowViewModel();

        public MainWindow()
        {
            InitializeComponent();
            KeyDown += ViewModel.KeyDown;
        }

        public MainWindow(IScene scene)
        {
            ViewModel = new MainWindowViewModel(scene);
            InitializeComponent();
            KeyDown += ViewModel.KeyDown;
        }

        private Point pos;
        private bool _rightMouseDown = false;

        private void UIElement_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_rightMouseDown)
            {
                var p = e.GetPosition((IInputElement)sender);
                var deltaX = (float)(p.X - pos.X);
                var deltaY = (float)(p.Y - pos.Y);

                ViewModel.DeltaMouse(new Vector2(deltaX, deltaY));
                pos = p;
            }
        }

        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            _rightMouseDown = false;
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _rightMouseDown = true;
            pos = e.GetPosition((IInputElement)sender);
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _rightMouseDown = false;
        }

        private void UIElement_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ViewModel.MouseWheel(e.Delta);
        }
    }
}
