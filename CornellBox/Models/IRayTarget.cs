using System.Runtime.CompilerServices;
using CornellBox.Models.Objects;

namespace CornellBox.Models
{
    public interface IRayTarget
    {
        (float lambda, IObject obj) ClosestIntersectionLambda(Ray ray);
        BoundingSphere GetBounds();
    }
}
