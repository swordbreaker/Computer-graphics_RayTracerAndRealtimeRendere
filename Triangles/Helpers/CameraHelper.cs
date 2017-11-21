using System.Numerics;
using System.Windows.Input;

namespace Triangles.Helpers
{
    public class CameraHelper
    {
        public float Elevation { get; set; }
        public float Azimut { get; set; }
        public float Radius { get; set; }

        public float Forward { get; set; } = 5f;
        private float _right = 0f;

        public CameraHelper(float elevation = 0f, float azimut = 0f, float radius = 20f)
        {
            Elevation = elevation;
            Azimut = azimut;
            Radius = radius;
            Forward = radius;
        }

        public void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            switch (keyEventArgs.Key)
            {
                case Key.Up:
                    Elevation += 0.1f;
                    keyEventArgs.Handled = true;
                    break;
                case Key.W:
                    Forward -= 0.1f;
                    keyEventArgs.Handled = true;
                    break;
                case Key.Down:
                    Elevation -= 0.1f;
                    keyEventArgs.Handled = true;
                    break;
                case Key.S:
                    Forward += 0.1f;
                    keyEventArgs.Handled = true;
                    break;
                case Key.Left:
                    Azimut -= 0.1f;
                    keyEventArgs.Handled = true;
                    break;
                case Key.A:
                    _right += 0.1f;
                    keyEventArgs.Handled = true;
                    break;
                case Key.Right:
                    Azimut += 0.1f;
                    keyEventArgs.Handled = true;
                    break;
                case Key.D:
                    _right -= 0.1f;
                    keyEventArgs.Handled = true;
                    break;
            }
        }

        public Matrix4x4 CameraMatrix
        {
            get
            {
                //var cameraPos = new Vector3(0, 0, -Radius);
                //var cameraTarget = new Vector3(0, 0, 0);
                //var up = Vector3.UnitY;

                //var r = Matrix4x4.CreateRotationX(Elevation) * Matrix4x4.CreateRotationY(-Azimut);

                return Matrix4x4.CreateRotationX(Elevation) * Matrix4x4.CreateRotationY(-Azimut) * Matrix4x4.CreateTranslation(_right, 0, Forward);
                //return Matrix4x4.CreateLookAt(Vector3.Transform(cameraPos, r), cameraTarget, Vector3.Transform(up, r));
            }
        }
    }
}
