using System.Numerics;

namespace Triangles.Models
{
    public interface IMaterial
    {
        Vector3 GetColor(float u, float v);
    }
}
