using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

namespace Triangles.Models
{
    public class TextureMaterial : IMaterial
    {
        private readonly TextureMode _mode;

        private readonly int _width;
        private readonly int _height;

        private readonly Vector3[,] _data;

        public enum TextureMode
        {
            Repeate,
            Clamp
        }

        public TextureMaterial(Bitmap texture, TextureMode mode)
        {
            _mode = mode;
            _width = texture.Width;
            _height = texture.Height;

            _data = new Vector3[_width, _height];

            var data = texture.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* p = (byte*)data.Scan0;
                for (int y = 0; y < _height; y++)
                {
                    var i = 0;
                    for (int x = 0; x < _width * 3; x += 3, i++)
                    {
                        var r = *(p++);
                        var g = *(p++);
                        var b = *(p++);

                        _data[i, y] = new Vector3(r / 255f, g / 255f, b / 255f);
                    }
                }

                texture.UnlockBits(data);
            }
        }

        public Vector3 GetColor(float u, float v, bool bilinearFiltering)
        {
            switch (_mode)
            {
                case TextureMode.Repeate:
                    u = Math.Abs(u % 1);
                    v = Math.Abs(v % 1);
                    break;
                case TextureMode.Clamp:
                    u = u.Clamp(0, 1f);
                    v = v.Clamp(0, 1f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (bilinearFiltering)
            {
                var x = u * (_width - 1);
                var y = v * (_height - 1);

                var (ix, iy) = ((int) x, (int) y);

                var tv = x - ix;
                var tu = y - iy;

                var c1 = Vector3.Lerp(_data[ix, iy], _data[ix, iy + 1], tv);
                var c2 = Vector3.Lerp(_data[ix + 1, iy], _data[ix + 1, iy + 1], tv);
                return Vector3.Lerp(c1, c2, tu);
            }
            else
            {
                var x = (int)(u * _width);
                var y = (int)(v * _height);
                return _data[x, y];
            }
        }
    }
}
