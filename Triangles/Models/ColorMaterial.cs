using System.Numerics;
using System.Windows.Media;

namespace Triangles.Models
{
    public class ColorMaterial : IMaterial
    {
        private readonly Vector3 _color;

        public ColorMaterial(Color color)
        {
            _color = color.ToVector3();
        }

        public Vector3 GetColor(float u, float v, bool bilinearFiltering)
        {
            return _color;
        }
    }
}
