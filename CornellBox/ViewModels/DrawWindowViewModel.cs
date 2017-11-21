using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CornellBox.Annotations;
using CornellBox.Commands;
using CornellBox.Models;
using CornellBox.Scenes;

namespace CornellBox.ViewModels
{
    public class DrawWindowViewModel : INotifyPropertyChanged
    {
        private readonly int _imgWidth;
        private readonly int _imgHeight;
        private readonly Scene _scene;
        private volatile bool _isDrawing = false;
        private string _message;

        private float _elevation = 0;
        private float _azimut = 0;
        private readonly Vector3 _startLookAt;

        #region Properties
        public RayTracing RayTracing { get; }
        public WriteableBitmap Bitmap { get; }

        public string Message
        {
            get => _message;
            set
            {
                if (value == _message) return;
                _message = value;
                OnPropertyChanged();
            }
        }

        public SimpleCommand DrawCommand { get; }
        public SimpleCommand CancelDrawCommand { get; set; }
        public SimpleCommand ClearCommand { get; set; }
        #endregion

        public DrawWindowViewModel(Scene scene, int size)
        {
            (_imgWidth, _imgHeight) = (size, size);
            _scene = scene;
            Bitmap = new WriteableBitmap(_imgWidth, _imgHeight, 8, 8, PixelFormats.Bgr24, null);
            RayTracing = new RayTracing(scene, _imgWidth, _imgHeight);

            var stopwatch = Stopwatch.StartNew();
            scene.Init();
            stopwatch.Stop();

            Message = $"Creating Tree: {stopwatch.ElapsedMilliseconds}ms";
            stopwatch.Stop();

            DrawCommand = new SimpleCommand(Draw);
            CancelDrawCommand = new SimpleCommand(CancelDraw);
            ClearCommand = new SimpleCommand(Clear);
            CancelDrawCommand.Disable();

            _startLookAt = scene.LookAt;
        }

        private async void Clear()
        {
            var pixels = new byte[_imgWidth * 3 * _imgHeight];
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Bitmap.Lock();
                Bitmap.WritePixels(new Int32Rect(0, 0, _imgWidth, _imgHeight), pixels, _imgWidth * 3, 0);
                Bitmap.Unlock();
            });
        }

        private void CancelDraw()
        {
            CancelDrawCommand.Disable();
            _isDrawing = false;
        }

        public async void Draw()
        {
            DrawCommand.Disable();
            CancelDrawCommand.Enable();
            _isDrawing = true;
            await Task.Run(() =>
            {
                var stopwatch = Stopwatch.StartNew();

                Parallel.For(0, _imgHeight, async y =>
                {
                    var pixels = new byte[_imgWidth * 3];
                    var i = 0;

                    for (int x = 0; x < _imgWidth; x++, i += 3)
                    {
                        if(!_isDrawing) return;
                        var c = RayTracing.CalculateColor(new Vector2(x, y));

                        pixels[i + 0] = c.B;
                        pixels[i + 1] = c.G;
                        pixels[i + 2] = c.R;
                    }

                    await DrawCol(pixels, y);
                });

                stopwatch.Stop();
                Message = $" Ray Tracing: {stopwatch.ElapsedMilliseconds}ms";

            });
            _isDrawing = false;
            CancelDrawCommand.Disable();
            DrawCommand.Enable();
        }

        private async Task DrawCol(byte[] pixels, int y)
        {
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Bitmap.Lock();
                Bitmap.WritePixels(new Int32Rect(0, y, _imgWidth, 1), pixels, _imgWidth * 3, 0);
                Bitmap.Unlock();
            });
        }

        public void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if (_isDrawing) return;

            switch (keyEventArgs.Key)
            {
                case Key.W:
                    _elevation += 0.1f;
                    UpdateLookAt();
                    Draw();
                    break;
                case Key.S:
                    _elevation -= 0.1f;
                    UpdateLookAt();
                    Draw();
                    break;
                case Key.A:
                    _azimut -= 0.1f;
                    UpdateLookAt();
                    Draw();
                    break;
                case Key.D:
                    _azimut += 0.1f;
                    UpdateLookAt();
                    Draw();
                    break;
            }
        }

        private void UpdateLookAt()
        {
            var r1 = Matrix4x4.CreateRotationX(_elevation);
            var r2 = Matrix4x4.CreateRotationY(_azimut);
            var r = r1 * r2;;

            var f = Vector3.Transform(_startLookAt - _scene.Eye, r);

            _scene.LookAt = _scene.Eye + f;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
