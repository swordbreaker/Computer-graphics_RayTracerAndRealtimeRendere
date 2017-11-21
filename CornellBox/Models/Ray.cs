using System.Numerics;

namespace CornellBox.Models
{
    public class Ray
    {
        public Vector3 Direction { get; }
        public Vector3 StartPos { get; }

        public Ray(Vector3 startPos, Vector3 direction)
        {
            Direction = Vector3.Normalize(direction);
            StartPos = startPos;
        }

        public override string ToString()
        {
            return $"{StartPos} + λ * {Direction}";
        }
    }
}
