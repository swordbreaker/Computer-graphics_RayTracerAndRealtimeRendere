using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Triangles.Helpers
{
    public class FpsHelper : INotifyPropertyChanged
    {
        private readonly Stopwatch _stopwatch;
        private readonly float _smoothing;
        private float _fps;
        private readonly int _calculationStep;
        private int _step;
        private float _fpsAverage;
        private float _milisecodsForFrame;
        private float _milisecodsForFrameFpsAverage;

        private float _fpsSum;
        private uint _fpsCount;

        private float _miliSum;
        private uint _miliCount;

        public float DeltaTime;

        public float Fps
        {
            get => _fps;
            private set
            {
                if (value.Equals(_fps)) return;
                _fps = value;
                _fpsSum += value;
                FpsAverage = _fpsSum / ++_fpsCount;
                OnPropertyChanged();
            }
        }

        public float FpsAverage
        {
            get => _fpsAverage;
            set
            {
                if (value.Equals(_fpsAverage)) return;
                _fpsAverage = value;
                OnPropertyChanged();
            }
        }

        public float MilisecodsForFrame
        {
            get => _milisecodsForFrame;
            set
            {
                if (value.Equals(_milisecodsForFrame)) return;
                _milisecodsForFrame = value;
                _miliSum += value;
                MilisecodsForFrameFpsAverage = _miliSum / ++_miliCount;
                OnPropertyChanged();
            }
        }

        public float MilisecodsForFrameFpsAverage
        {
            get => _milisecodsForFrameFpsAverage;
            set
            {
                if (value.Equals(_milisecodsForFrameFpsAverage)) return;
                _milisecodsForFrameFpsAverage = value;

                OnPropertyChanged();
            }
        }

        public FpsHelper(int calculationStep = 3, float smoothing = 0.9f)
        {
            _stopwatch = new Stopwatch();
            _smoothing = smoothing;
            _calculationStep = calculationStep;
        }

        public void Start() => _stopwatch.Start();

        public void Stop() => _stopwatch.Stop();

        public void BeginOfFrame() => _stopwatch.Restart();

        public void EndOfFrame()
        {
            if (++_step >= _calculationStep)
            {
                _step = 0;
                var current = 1 / (float)_stopwatch.Elapsed.TotalSeconds;
                Fps = (Fps * _smoothing) + (current * (1.0f - _smoothing));
                MilisecodsForFrame = (float)_stopwatch.Elapsed.TotalMilliseconds;
                DeltaTime = (float) _stopwatch.Elapsed.TotalSeconds;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

 
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
