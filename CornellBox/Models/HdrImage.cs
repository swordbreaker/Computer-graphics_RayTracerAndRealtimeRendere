using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CornellBox.Models
{
    public class HdrImage
    {
        private readonly float[,] _data;

        public readonly int Width;
        public readonly int Height;

        public HdrImage(Bitmap image)
        {
            Width = image.Width;
            Height = image.Height;

            _data = new float[Width * 3, Height];

            var data = image.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            unsafe
            {
                int i = 0;
                byte* p = (byte*)data.Scan0;

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width * 3; x += 3)
                    {
                        var r = *(p++);
                        var g = *(p++);
                        var b = *(p++);
                        _data[x + 0, y] = b / 255f;
                        _data[x + 1, y] = g / 255f;
                        _data[x + 2, y] = r / 255f;
                    }
                }

                image.UnlockBits(data);
            }
        }

        public HdrImage(string hdrImagepath)
        {
            float[] fs;

            using(var stream = File.OpenRead(hdrImagepath))
            using (var reader = new BinaryReader(stream))
            {
                fs = new float[stream.Length / 4];

                var k = 0;
                while (stream.Position < stream.Length)
                {
                    fs[k++] = reader.ReadSingle();
                }
            }


            Width = (int)Math.Sqrt(fs.Length / 3);
            Height = (int)Math.Sqrt(fs.Length / 3);
            _data = new float[Width * 3, Height];

            int i = 0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width * 3; x++, i++)
                {
                    _data[x, y] = fs[i];
                }
            }
        }

        public async Task<WriteableBitmap> CreateBitmapImage()
        {
            var bitmapImage = new WriteableBitmap(Width, Height, 8, 8, PixelFormats.Bgr24, null);

            var pixels = new byte[Width * 3 * Height];

            var i = 0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width * 3; x += 3, i += 3)
                {
                    var r = (byte)(_data[x + 0, y] * 255);
                    var g = (byte)(_data[x + 1, y] * 255);
                    var b = (byte)(_data[x + 2, y] * 255);

                    pixels[i + 0] = b;
                    pixels[i + 1] = g;
                    pixels[i + 2] = r;
                }
            }

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                bitmapImage.Lock();
                bitmapImage.WritePixels(new Int32Rect(0, 0, Width, Height), pixels, Width * 3, 0);
                bitmapImage.Unlock();
            });

            return bitmapImage;
        }

        public Vector3 this[int x, int y]
        {
            get
            {
                var x0 = x * 3;
                return new Vector3(_data[x0, y], _data[x0 + 1, y], _data[x0 + 2, y]);
            }
        }
    }
}
