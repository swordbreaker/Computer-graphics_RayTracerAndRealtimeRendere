using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CornellBox.Scenes;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CornellBox.Annotations;
using CornellBox.Commands;
using CornellBox.Models;
using CornellBox.Views;

namespace CornellBox.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private Scene _scene;
        private string _sceneName;

        public string SceneName
        {
            get => _sceneName;
            set
            {
                _sceneName = value;
                _scene = SceneManager.Scenes[value];
            }
        }

        public int ImgSize { get; set; } = 600;
        public bool UseAccelerationStructures { get; set; } = true;

        public IEnumerable<string> SceneNames => SceneManager.Scenes.Keys;
        public SimpleCommand ChooseCommand { get; set; }

        public MainWindowViewModel()
        {
            ChooseCommand = new SimpleCommand(Choose);
            ChooseCommand.Disable();

            LoadScenes();
            //Test();
        }

        public WriteableBitmap TestImage { get; set; }

        private async void Test()
        {
            //var hdr = new HdrImage((Bitmap)Image.FromFile(@"Textures\stone.png"));
            var hdr = new HdrImage(@"SkyMaps\grace_probe.float");

            TestImage = await hdr.CreateBitmapImage();
            OnPropertyChanged(nameof(TestImage));
        }

        private async void LoadScenes()
        {
            await Task.Run(() => SceneManager.LoadScenes());
            _scene = SceneManager.Scenes.Values.First();
            ChooseCommand.Enable();
        }

        private void Choose()
        {
            _scene.UseAccelerationStructures = UseAccelerationStructures;
            var w = new DrawWindow(new Scene( _scene), ImgSize);
            w.Show();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}