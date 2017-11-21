using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Triangles.Helpers;

namespace Triangles.Models
{
    public class Renderer
    {
        #region Fields
        private readonly IScene _scene;
        private readonly int _imgHeight;
        private readonly int _imgWidth;
        private readonly float[,] _zBuffer;
        //private readonly unsafe float* _zBuffer;
        private readonly Vector3[,] _normalBuffer;
        private readonly Vector3[,] _diffuseBuffer;
        private readonly Vector3[,] _textureBuffer;
        private readonly Vector3[,] _positionBuffer;

        private readonly unsafe byte* _pixelBuffer;

        public delegate void UpdateDelegate(Renderer renderer);
        private readonly UpdateDelegate _updateAction;
        private readonly Matrix4x4 _projection;
        private bool _isActive = true;


        private Vector3 _lightPos;
        #endregion

        #region Properties
        public WriteableBitmap Bitmap { get; }
        public WriteableBitmap ZBufferBitmap { get; }
        public WriteableBitmap NormalBufferBitmap { get; }
        public WriteableBitmap DiffuseBufferBitmap { get; }
        public WriteableBitmap TextureBufferBitmap { get; }
        public WriteableBitmap PositionBufferBitmap { get; }

        public FpsHelper FpsHelper { get; } = new FpsHelper();
        public Settings Settings { get; set; }
        public Matrix4x4 ViewModel { get; set; } = Matrix4x4.Identity;
        public bool IsActive => _isActive;
        public bool DrawBuffers { get; set; }
        #endregion

        public Renderer(IScene scene, int imgWidht, int imgHeight, Settings settings, UpdateDelegate updateAction = null)
        {
            _scene = scene;
            Settings = settings;
            _imgHeight = imgHeight;
            _imgWidth = imgWidht;
            _zBuffer = new float[_imgWidth, _imgHeight];
            _diffuseBuffer = new Vector3[_imgWidth, _imgHeight];
            _positionBuffer = new Vector3[_imgWidth, _imgHeight];
            _normalBuffer = new Vector3[_imgWidth, _imgHeight];
            _textureBuffer = new Vector3[_imgWidth, _imgHeight];
            _updateAction = updateAction;
            _lightPos = _scene.LightPos;

            _projection = new Matrix4x4(
                _imgWidth, 0, (float)imgWidht / 2, 0,
                0, _imgWidth, (float)_imgHeight / 2, 0,
                0, 0, 0, 0,
                0, 0, 1, 0);

            Bitmap = new WriteableBitmap(imgWidht, imgHeight, 8, 8, PixelFormats.Bgr24, null);
            ZBufferBitmap = new WriteableBitmap(imgWidht, imgHeight, 8, 8, PixelFormats.Bgr24, null);
            NormalBufferBitmap = new WriteableBitmap(imgWidht, imgHeight, 8, 8, PixelFormats.Bgr24, null);
            DiffuseBufferBitmap = new WriteableBitmap(imgWidht, imgHeight, 8, 8, PixelFormats.Bgr24, null);
            TextureBufferBitmap = new WriteableBitmap(imgWidht, imgHeight, 8, 8, PixelFormats.Bgr24, null);
            PositionBufferBitmap = new WriteableBitmap(imgWidht, imgHeight, 8, 8, PixelFormats.Bgr24, null);

            App.CurrentRenderer = this;

            unsafe
            {
                _pixelBuffer = (byte*)Bitmap.BackBuffer;
            }

            //ZBufferBitmap.Lock();
            //unsafe
            //{
            //    _zBuffer = (float*)ZBufferBitmap.BackBuffer;
            //}
            //ZBufferBitmap.Unlock();
        }

        public void StartRendering()
        {
            _isActive = true;
            var frameTime = TimeSpan.FromSeconds(1 / Settings.Fps);

            Task.Run(() =>
            {
                while (_isActive)
                {
                    FpsHelper.BeginOfFrame();

                    var t1 = Task.Run(Rendering);
                    var t2 = Task.Delay(frameTime);
                    Task.WaitAll(t1, t2);
                    _updateAction?.Invoke(this);

                    if (!Settings.StaticLight)
                        _lightPos = Vector3.Transform(Vector3.Zero,
                            Matrix4x4.CreateTranslation(_scene.LightPos) * ViewModel);
                    else _lightPos = Vector3.Transform(Vector3.Zero,
                        Matrix4x4.CreateTranslation(_scene.LightPos) * Matrix4x4.CreateTranslation(ViewModel.Translation));

                    FpsHelper.EndOfFrame();
                }
            });
        }

        public void StopRendering() => _isActive = false;

        private async Task Rendering()
        {
            _zBuffer.Clear(float.PositiveInfinity);
            _diffuseBuffer.Clear(Vector3.Zero);
            _textureBuffer.Clear(Vector3.Zero);
            _normalBuffer.Clear(Vector3.Zero);
            _positionBuffer.Clear(Vector3.Zero);

            var triangles = new List<(Triangle t2d, int xMin, int xMax, int yMin, int yMax)>();

            foreach (var mesh in _scene.Meshes)
            {
                foreach (var triangle in mesh.Triangles(Settings.ZPlane, Matrix4x4.Identity))
                {
                    var trianlge2D = triangle.TransformNormalized(_projection);
                    if (trianlge2D.IsClockWise) continue;

                    var tMin = trianlge2D.Min;
                    var tMax = trianlge2D.Max;
                    (var xMin, var yMin) = ((int)tMin.X.Clamp(0, _imgWidth - 1), (int)tMin.Y.Clamp(0, _imgHeight - 1));
                    (var xMax, var yMax) = ((int)tMax.X.Clamp(0, _imgWidth - 1), (int)tMax.Y.Clamp(0, _imgHeight - 1));

                    triangles.Add((trianlge2D, xMin, xMax, yMin, yMax));

                    Parallel.For(yMin, yMax + 1, y =>
                    {
                        var i = y * _imgWidth * 3 + xMin * 3;
                        var AB = trianlge2D.AB;
                        var AC = trianlge2D.AC;

                        var det = 1 / (AB.X * AC.Y - AC.X * AB.Y);
                        var ap = new Vector3(xMin, y, 0) - trianlge2D.A;

                        var u = (AC.Y * ap.X + -AC.X * ap.Y) * det;
                        var v = (-AB.Y * ap.X + AB.X * ap.Y) * det;

                        var uAdd = AC.Y * det;
                        var vAdd = -AB.Y * det;

                        for (int x = xMin; x < xMax + 1; x++, i += 3)
                        {
                            if (u >= 0 && v >= 0 && (u + v) < 1)
                            {
                                var vs = trianlge2D.Verts;
                                var p = VectorHelper.Lerp(vs[0], vs[1], vs[2], u, v);
                                var w = p.W;
                                p /= p.W;
                                p.W = w;

                                if (_zBuffer[x, y] > p.W)
                                {
                                    _zBuffer[x, y] = p.W;
                                    PrePass(trianlge2D, triangle, u, v, x, y);
                                }
                            }

                            u += uAdd;
                            v += vAdd;

                            ap.X++;
                        }
                    });
                }
            }



            //Parallel.For(0, _imgHeight, y =>
            //    //for (int y = 0; y < _imgHeight; y++)
            //{
            //    int i = 0;
            //    var pixels2 = new byte[_imgWidth * 3];
            //    for (int x = 0; x < _imgWidth; x++, i += 3)
            //    {
            //        var c = ColorPass(x, y).ToColor();

            //        pixels2[i + 0] = c.B;
            //        pixels2[i + 1] = c.G;
            //        pixels2[i + 2] = c.R;
            //    }

            //    App.Current?.Dispatcher.InvokeAsync(() =>
            //    {
            //        Bitmap.Lock();
            //        Bitmap.WritePixels(new Int32Rect(0, y, _imgWidth, 1), pixels2, _imgWidth * 3, 0);
            //        Bitmap.Unlock();
            //    });
            //});


            unsafe
            {
                var colorSize = _imgWidth * _imgHeight * 3;
                ClearBuffer(_pixelBuffer, colorSize);

                foreach (var tuple in triangles)
                {
                    var trianlge2D = tuple.t2d;

                    (var xMin, var yMin) = (tuple.xMin, tuple.yMin);
                    (var xMax, var yMax) = (tuple.xMax, tuple.yMax);

                    Parallel.For(yMin, yMax + 1, y =>
                    {
                        var i = y * _imgWidth * 3 + xMin * 3;
                        var AB = trianlge2D.AB;
                        var AC = trianlge2D.AC;

                        var det = 1 / (AB.X * AC.Y - AC.X * AB.Y);
                        var ap = new Vector3(xMin, y, 0) - trianlge2D.A;

                        var u = (AC.Y * ap.X + -AC.X * ap.Y) * det;
                        var v = (-AB.Y * ap.X + AB.X * ap.Y) * det;

                        var uAdd = AC.Y * det;
                        var vAdd = -AB.Y * det;

                        for (int x = xMin; x < xMax + 1; x++, i += 3)
                        {
                            if (u >= 0 && v >= 0 && (u + v) < 1)
                            {
                                var vs = trianlge2D.Verts;
                                var p = VectorHelper.Lerp(vs[0], vs[1], vs[2], u, v);
                                var w = p.W;
                                p /= p.W;
                                p.W = w;

                                if(_zBuffer[x, y] == p.W)
                                {
                                    var c = ColorPass(x, y).ToColor();

                                    _pixelBuffer[i + 0] = c.B;
                                    _pixelBuffer[i + 1] = c.G;
                                    _pixelBuffer[i + 2] = c.R;
                                    //pixels[i + 0] = c.B;
                                    //pixels[i + 1] = c.G;
                                    //pixels[i + 2] = c.R;
                                }
                            }

                            u += uAdd;
                            v += vAdd;

                            ap.X++;
                        }
                    });

                }
            }

            await App.Current?.Dispatcher.InvokeAsync(()=>
            {
                if (Bitmap == null) return;

                Bitmap.Lock();
                Bitmap.AddDirtyRect(new Int32Rect(0, 0, _imgWidth, _imgHeight));
                Bitmap.Unlock();
                if (DrawBuffers)
                {
                    WriteToBitmap(ZBufferBitmap, _zBuffer
                        .Cast<float>()
                        .SelectMany(f => new[] { (byte)(f * 50), (byte)(f * 50), (byte)(f * 50) })
                        .ToArray());

                    WriteToBitmap(NormalBufferBitmap, _normalBuffer
                        .Cast<Vector3>()
                        .SelectMany(v => new [] { (byte)(v.Y * 255), (byte)(v.Z * 255), (byte)(v.X * 255), })
                        .ToArray());

                    WriteToBitmap(DiffuseBufferBitmap, _diffuseBuffer
                        .Cast<Vector3>()
                        .SelectMany(v => new [] { (byte)(v.Y * 255), (byte)(v.Z * 255), (byte)(v.X * 255), })
                        .ToArray());

                    WriteToBitmap(TextureBufferBitmap, _textureBuffer
                        .Cast<Vector3>()
                        .SelectMany(v => new [] { (byte)(v.Y * 255), (byte)(v.Z * 255), (byte)(v.X * 255), })
                        .ToArray());

                    WriteToBitmap(PositionBufferBitmap, _positionBuffer
                        .Cast<Vector3>()
                        .SelectMany(v => new [] { (byte)(v.Y * 51), (byte)(v.Z * 51), (byte)(v.X * 51), })
                        .ToArray());
                }
            });

            
            //await App.Current?.Dispatcher.InvokeAsync(() =>
            //{
            //    if (Bitmap == null) return;
            //    WriteToBitmap(Bitmap, pixels);

            //    if (DrawBuffers)
            //    {
            //        //WriteToBitmap(ZBufferBitmap, _zBuffer.Cast<float>().Select(f => (float.IsInfinity(f)?0:f)).ToArray(), 1);
            //        WriteToBitmap(NormalBufferBitmap, _normalBuffer
            //            .Cast<Vector3>()
            //            .SelectMany(v => new byte[] { (byte)(v.Y * 255), (byte)(v.Z * 255), (byte)(v.X * 255), })
            //            .ToArray());

            //        WriteToBitmap(DiffuseBufferBitmap, _diffuseBuffer
            //            .Cast<Vector3>()
            //            .SelectMany(v => new byte[] { (byte)(v.Y * 255), (byte)(v.Z * 255), (byte)(v.X * 255), })
            //            .ToArray());

            //        WriteToBitmap(TextureBufferBitmap, _textureBuffer
            //            .Cast<Vector3>()
            //            .SelectMany(v => new byte[] { (byte)(v.Y * 255), (byte)(v.Z * 255), (byte)(v.X * 255), })
            //            .ToArray());

            //        WriteToBitmap(PositionBufferBitmap, _positionBuffer
            //            .Cast<Vector3>()
            //            .SelectMany(v => new byte[] { (byte)(v.Y * 51), (byte)(v.Z * 51), (byte)(v.X * 51), })
            //            .ToArray());
            //    }
            //});
        }

        private unsafe void ClearBuffer(byte* buffer, int size)
        {
            var ptr = buffer;
            for (int i = 0; i < _imgWidth * _imgHeight * 3; i++)
            {
                (*ptr++) = 0;
            }
        }

        private unsafe void ClearBuffer(float* buffer, int size)
        {
            var ptr = buffer;
            for (int i = 0; i < size; i++)
            {
                (*ptr++) = float.PositiveInfinity;
            }
        }

        private void WriteToBitmap<T>(WriteableBitmap bmp, T[] buffer, int componets = 3)
        {
            bmp.Lock();
            bmp.WritePixels(new Int32Rect(0, 0, _imgWidth, _imgHeight), buffer, _imgWidth * componets, 0);
            bmp.Unlock();
        }

        private void PrePass(Triangle tria, Triangle tria3d, float u, float v, int x, int y)
        {
            var wAndUv = (tria.Verts[0].W, tria.Verts[1].W, tria.Verts[2].W, u, v);
            var normal = Vector3.Normalize(VectorHelper.LerpHomogeneous(tria3d.ANormal, tria3d.BNormal, tria3d.CNormal, wAndUv).ToVector3());
            _normalBuffer[x, y] = normal;
            var p3D = VectorHelper.LerpHomogeneous(tria3d.A, tria3d.B, tria3d.C, wAndUv).ToVector3();
            _positionBuffer[x, y] = p3D;
            var uv = VectorHelper.LerpHomogeneous(tria.VertUvs[0], tria.VertUvs[1], tria.VertUvs[2], wAndUv);
            var matColor = tria.Material.GetColor(uv.X, uv.Y);
            var color = VectorHelper.LerpHomogeneous(tria.AColor, tria3d.BColor, tria3d.CColor, wAndUv).ToVector3();
            _diffuseBuffer[x, y] = color;
            _textureBuffer[x, y] = matColor;
        }

        private Vector3 ColorPass(int x, int y)
        {
            var color = _diffuseBuffer[x, y];
            var normal = _normalBuffer[x, y];
            var p3D = _positionBuffer[x, y];

            var l = Vector3.Normalize(_lightPos - p3D);
            var dotNl = Vector3.Dot(normal, l);
            var Id = Math.Max(0f, dotNl) * color;

            var r = 2 * dotNl * normal - l;
            r = Vector3.Normalize(r);

            var Is = (float)Math.Pow(Math.Max(0, -Vector3.Dot(r, Vector3.Normalize(p3D))), 50) * new Vector3(1f, 1f, 1f);

            return (_scene.EnviromentLight * color) + _textureBuffer[x, y] * Id + Is;
        }

        private Vector3 CalcColor(Triangle tria, Triangle tria3d, float u, float v, Vector4 p)
        {
            var wAndUv = (tria.Verts[0].W, tria.Verts[1].W, tria.Verts[2].W, u, v);

            var color = VectorHelper.LerpHomogeneous(tria.AColor, tria3d.BColor, tria3d.CColor, wAndUv).ToVector3();
            var uv = VectorHelper.LerpHomogeneous(tria.VertUvs[0], tria.VertUvs[1], tria.VertUvs[2], wAndUv);
            var matColor = tria.Material.GetColor(uv.X, uv.Y);
            var normal = Vector3.Normalize(VectorHelper.LerpHomogeneous(tria3d.ANormal, tria3d.BNormal, tria3d.CNormal, wAndUv).ToVector3());
            var p3D = VectorHelper.LerpHomogeneous(tria3d.A, tria3d.B, tria3d.C, wAndUv).ToVector3();
            var l = Vector3.Normalize(_lightPos - p3D);
            var dotNl = Vector3.Dot(normal, l);
            var Id = Math.Max(0f, dotNl) * color;

            var r = 2 * dotNl * normal - l;
            r = Vector3.Normalize(r);

            var Is = (float)Math.Pow(Math.Max(0, -Vector3.Dot(r, Vector3.Normalize(p3D))), 50) * new Vector3(1f, 1f, 1f);

            return (_scene.EnviromentLight * color) + matColor * Id + Is;
        }
    }
}
