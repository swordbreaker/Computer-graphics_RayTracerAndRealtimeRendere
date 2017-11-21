using System.Numerics;

namespace CornellBox.Models
{
    public class Light
    {
        public Vector3 Color { get; }
        public Vector3 Position { get; }
        public float Intensity { get; }
        public float LightAttenuationC { get; }

        public Light(Vector3 position, Vector3 color, float intensity, float lightAttenuationC = 0.5f)
        {
            Color = color;
            Position = position;
            Intensity = intensity;
            LightAttenuationC = lightAttenuationC;
        }
    }
}
