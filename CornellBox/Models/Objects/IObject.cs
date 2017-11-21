using System.Numerics;

namespace CornellBox.Models
{
    public interface IObject : IRayTarget
    {
        Material Material { get; }
        Vector3 GetNorm(Vector3 pos);
        Vector3 ToLocal(Vector3 pos);
        Vector3 ToLocalNormalized(Vector3 pos);
    }
}
