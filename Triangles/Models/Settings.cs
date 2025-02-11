﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using Triangles.Annotations;

namespace Triangles.Models
{
    public class Settings : INotifyPropertyChanged
    {
        private volatile bool _bilinearFiltering;
        private bool _specular;
        private float _zPlane;
        private bool _staticLight;
        private bool _useGpu;

        public bool BilinearFiltering
        {
            get => _bilinearFiltering;
            set
            {
                if (value == _bilinearFiltering) return;
                _bilinearFiltering = value;
                OnPropertyChanged();
            }
        }

        public bool Specular
        {
            get => _specular;
            set
            {
                if (value == _specular) return;
                _specular = value;
                OnPropertyChanged();
            }
        }

        public float ZPlane
        {
            get => _zPlane;
            set
            {
                if (value.Equals(_zPlane)) return;
                _zPlane = value;
                OnPropertyChanged();
            }
        }

        public bool StaticLight
        {
            get => _staticLight;
            set
            {
                if (value == _staticLight) return;
                _staticLight = value;
                OnPropertyChanged();
            }
        }

        public bool UseGpu
        {
            get => _useGpu;
            set
            {
                if (value == _useGpu) return;
                _useGpu = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
