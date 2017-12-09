using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Triangles.Annotations;
using Triangles.Commands;
using Triangles.Helpers;
using Triangles.Models;
using Triangles.Scenes;
using Triangles.Views;

namespace Triangles.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public enum Scenes
        {
            Cube, CubeNotRota, ManyCubes
        }

        private readonly CameraHelper _cameraHelper = new CameraHelper(radius:5f);
        private string _title = "Rendere";
        public Renderer Renderer { get; }
        public Settings Settings { get; set; }

        public ICommand OpenFileCommand { get; }
        public ICommand OpenSceneCommand { get; }
        public ICommand StartStopCommand { get; }

        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        private const int W = 500;
        private const int H = 500;

        public int Width => W;
        public int Height => H;

        public MainWindowViewModel() : this(new CubeScene(true))
        {
        }

        public MainWindowViewModel(IScene scene)
        {
            Settings = new Settings()
            {
                Specular = true,
                ZPlane = 0.1f,
                BilinearFiltering = true
            };

            Renderer = new Renderer(scene, W, H, Settings, OnUpdate);
            Start();
            App.Current.Exit += (sender, args) => Renderer.StopRendering();
            OpenFileCommand = new SimpleCommand(OpenFile);
            OpenSceneCommand = new SimpleCommand(OpenScene);
            StartStopCommand = new SimpleCommand(StartStop);
        }

        private void StartStop()
        {
            if(Renderer.IsActive) Stop();
            else Start();
        }

        private void OpenScene(object o)
        {
            IScene scene;
            switch ((Scenes)o)
            {
                case Scenes.Cube:
                    scene = new CubeScene(true);
                    break;
                case Scenes.ManyCubes:
                    scene = new ManyCubesScene();
                    break;
                case Scenes.CubeNotRota:
                    scene = new CubeScene(false);
                    break;
                default:
                    scene = new CubeScene(true);
                    break;
            }

            var window = new MainWindow(scene);
            window.Show();
            Stop();
        }

        private void OpenFile()
        {
            Stop();

            var fileDialog = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = @"Wavefront File(*.OBJ)|*.OBJ"
            };
            var result = fileDialog.ShowDialog();

            string objPath;

            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    objPath = fileDialog.FileName;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    return;
            }

            var window = new MainWindow(new CustomMeshScene(objPath));
            window.Show();
        }

        public void KeyDown(object sender, KeyEventArgs e)
        {
            _cameraHelper.OnKeyDown(sender, e);
        }

        public void DeltaMouse(Vector2 delta)
        {
            _cameraHelper.Azimut += delta.X / W;
            _cameraHelper.Elevation += delta.Y / H;
        }

        public void MouseWheel(int delta)
        {
            _cameraHelper.Forward -= delta * 0.001f;
        }

        private void OnUpdate(Renderer renderer)
        {
            renderer.ViewModel = _cameraHelper.CameraMatrix;
        }

        private void Start()
        {
            Renderer.StartRendering();
            Title = $"Renderer | running";
        }

        private void Stop()
        {
            Renderer.StopRendering();
            Title = $"Renderer | stopped";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
