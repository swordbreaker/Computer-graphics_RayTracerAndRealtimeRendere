using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Alea;
using Alea.Parallel;
using Triangles.Helpers;

namespace Triangles.Models
{
    public class Renderer
    {
        #region Fields
        private readonly IScene _scene;
        private readonly int _imgHeight;
        private readonly int _imgWidth;
        [GpuParam]
        private readonly float[] _zBuffer;
        //private readonly unsafe float* _zBuffer;
        [GpuParam]
        private readonly float[] _normalBuffer;
        [GpuParam]
        private readonly float[] _diffuseBuffer;
        [GpuParam]
        private readonly float[] _textureBuffer;
        [GpuParam]
        private readonly float[] _positionBuffer;

        //private readonly unsafe byte* _zBufferPtr;
        //private readonly unsafe byte* _pixelBuffer;
        //private readonly unsafe byte* _normalBufferPtr;
        //private readonly unsafe byte* _diffuseBufferPtr;
        //private readonly unsafe byte* _textureBufferPtr;
        //private readonly unsafe byte* _positionBufferPtr;

        
        private readonly byte[] _zBufferImg;
        [GpuParam]
        private readonly byte[] _pixelBufferImg;
        private readonly byte[] _normalBufferImg;
        private readonly byte[] _diffuseBufferImg;
        private readonly byte[] _textureBufferImg;
        private readonly byte[] _positionBufferImg;


        public delegate void UpdateDelegate(Renderer renderer);
        private readonly UpdateDelegate _updateAction;
        private readonly Matrix4x4 _projection;
        private bool _isActive = true;

        private Task _bufferTask = Task.Run(() => {});
        private Task _mergeTask = Task.Run(() => {});

        private readonly int _colorSize;

        private Vector3 _lightPos;
        [GpuParam]
        private float[] _lightPosArray;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Renderer"/> class.
        /// </summary>
        /// <param name="scene">The scene.</param>
        /// <param name="imgWidht">The img widht.</param>
        /// <param name="imgHeight">Height of the img.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="updateAction">The update action.</param>
        public Renderer(IScene scene, int imgWidht, int imgHeight, Settings settings, UpdateDelegate updateAction = null)
        {
            _scene = scene;
            Settings = settings;
            _imgHeight = imgHeight;
            _imgWidth = imgWidht;
            _zBuffer = new float[_imgWidth * _imgHeight];
            _diffuseBuffer = new float[_imgWidth * _imgHeight * 3];
            _positionBuffer = new float[_imgWidth * _imgHeight * 3];
            _normalBuffer = new float[_imgWidth * _imgHeight * 3];
            _textureBuffer = new float[_imgWidth * _imgHeight * 3];
            _updateAction = updateAction;
            _lightPos = _scene.LightPos;

            _lightPosArray = new[] { _lightPos.X, _lightPos.Y, _lightPos.Z };

            _projection = new Matrix4x4(
                _imgWidth, 0, (float)imgWidht / 2, 0,
                0, _imgWidth, (float)_imgHeight / 2, 0,
                0, 0, 0, 0,
                0, 0, 1, 0);

            Bitmap = new WriteableBitmap(imgWidht, imgHeight, 8, 8, PixelFormats.Bgr24, null);
            ZBufferBitmap = new WriteableBitmap(imgWidht, imgHeight, 8, 8, PixelFormats.Gray8, null);
            NormalBufferBitmap = new WriteableBitmap(imgWidht, imgHeight, 8, 8, PixelFormats.Rgb24, null);
            DiffuseBufferBitmap = new WriteableBitmap(imgWidht, imgHeight, 8, 8, PixelFormats.Rgb24, null);
            TextureBufferBitmap = new WriteableBitmap(imgWidht, imgHeight, 8, 8, PixelFormats.Rgb24, null);
            PositionBufferBitmap = new WriteableBitmap(imgWidht, imgHeight, 8, 8, PixelFormats.Rgb24, null);

            _colorSize = _imgWidth * _imgHeight * 3;

            App.CurrentRenderer = this;

            _pixelBufferImg = new byte[_imgWidth * _imgHeight * 3];
            _zBufferImg = new byte[_imgWidth * _imgHeight];
            _normalBufferImg = new byte[_imgWidth * _imgHeight * 3];
            _diffuseBufferImg = new byte[_imgWidth * _imgHeight * 3];
            _textureBufferImg = new byte[_imgWidth * _imgHeight * 3];
            _positionBufferImg = new byte[_imgWidth * _imgHeight * 3];

            //unsafe
            //{
            //    _pixelBuffer = (byte*)Bitmap.BackBuffer;
            //    _zBufferPtr = (byte*)ZBufferBitmap.BackBuffer;
            //    _normalBufferPtr = (byte*)NormalBufferBitmap.BackBuffer;
            //    _diffuseBufferPtr = (byte*)DiffuseBufferBitmap.BackBuffer;
            //    _textureBufferPtr = (byte*)TextureBufferBitmap.BackBuffer;
            //    _positionBufferPtr = (byte*)PositionBufferBitmap.BackBuffer;
            //}
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

                    var t1 = Task.Run(() => Rendering());
                    var t2 = Task.Delay(frameTime);
                    Task.WaitAll(t1, t2);
                    _updateAction?.Invoke(this);

                    if (!Settings.StaticLight)
                    {
                        _lightPos = Vector3.Transform(Vector3.Zero,
                            Matrix4x4.CreateTranslation(_scene.LightPos) * ViewModel);
                        _lightPosArray = new[] { _lightPos.X, _lightPos.Y, _lightPos.Z };
                    }
                    else
                    {
                        _lightPos = Vector3.Transform(Vector3.Zero,
                            Matrix4x4.CreateTranslation(_scene.LightPos) * Matrix4x4.CreateTranslation(ViewModel.Translation));
                        _lightPosArray = new[] { _lightPos.X, _lightPos.Y, _lightPos.Z };
                    }

                    FpsHelper.EndOfFrame();
                }
            });
        }

        public void StopRendering() => _isActive = false;

        private void Rendering()
        {
            _bufferTask = Task.Run(() => CalculateBuffer());

            Task.WaitAll(_bufferTask, _mergeTask);

            _mergeTask = Task.Run(() => MergeBuffers());

            Task.WaitAll(_mergeTask);
        }

        private async void CalculateBuffer()
        {
            _zBuffer.Clear(float.PositiveInfinity);
            _diffuseBuffer.Clear(0);
            _textureBuffer.Clear(0);
            _normalBuffer.Clear(0);
            _positionBuffer.Clear(0);

            if (DrawBuffers)
            {
                _zBufferImg.Clear((byte)0);
                _normalBufferImg.Clear((byte)0);
                _diffuseBufferImg.Clear((byte)0);
                _positionBufferImg.Clear((byte)0);
                _textureBufferImg.Clear((byte)0);
            }

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

                    var triaArray2d = trianlge2D.ToArray();
                    var triaArray = triangle.ToArray();
                    //triangles.Add((trianlge2D, xMin, xMax, yMin, yMax));

                    Parallel.For(yMin, yMax + 1, y =>
                    {
                        var i = y * _imgWidth * 3 + xMin * 3;
                        var iz = y * _imgWidth + xMin;

                        //var AB = ArrayMath.Sub(triaArray2d, triaArray2d, 0, 4, 4);
                        //var AC = ArrayMath.Sub(triaArray2d, triaArray2d, 0, 8, 4);
                        var AB = trianlge2D.AB;
                        var AC = trianlge2D.AC;

                        //var det = 1 / AB[0] * AC[1] - AC[0] * AB[1];
                        var det = 1 / (AB.X * AC.Y - AC.X * AB.Y);

                        //var ap = ArrayMath.Sub(new[] {xMin, y, 0f}, triaArray2d, 0, 0, 4);
                        var ap = new Vector3(xMin, y, 0) - trianlge2D.A;

                        //var u = (AC[1] * ap[0] + -AC[0] * ap[1]) * det;
                        //var v = (-AB[1] * ap[0] + AB[0] * ap[1]) * det;

                        var u = (AC.Y * ap.X + -AC.X * ap.Y) * det;
                        var v = (-AB.Y * ap.X + AB.X * ap.Y) * det;

                        //var uAdd = AC[1] * det;
                        //var vAdd = -AB[1] * det;

                        var uAdd = AC.Y * det;
                        var vAdd = -AB.Y * det;

                        for (int x = xMin; x < xMax + 1; x++, i += 3, iz++)
                        {
                            if (u >= 0 && v >= 0 && (u + v) < 1)
                            {
                                var vs = trianlge2D.Verts;
                                var p = VectorHelper.Lerp(vs[0], vs[1], vs[2], u, v);
                                var w = p.W;
                                p /= p.W;
                                p.W = w;

                                if (_zBuffer[iz] > p.W)
                                {
                                    _zBuffer[iz] = p.W;
                                    PrePass(trianlge2D, triangle, u, v, i);
                                    if (DrawBuffers)
                                    {
                                        _zBufferImg[iz] = (byte)(_zBuffer[iz] * 50);

                                        _normalBufferImg[i + 0] = (byte)(_normalBuffer[i + 0] * 255);
                                        _normalBufferImg[i + 1] = (byte)(_normalBuffer[i + 1] * 255);
                                        _normalBufferImg[i + 2] = (byte)(_normalBuffer[i + 2] * 255);

                                        _diffuseBufferImg[i + 0] = (byte)(_diffuseBuffer[i + 0] * 255);
                                        _diffuseBufferImg[i + 1] = (byte)(_diffuseBuffer[i + 1] * 255);
                                        _diffuseBufferImg[i + 2] = (byte)(_diffuseBuffer[i + 2] * 255);

                                        _positionBufferImg[i + 0] = (byte)(_positionBuffer[i + 0] * 50);
                                        _positionBufferImg[i + 1] = (byte)(_positionBuffer[i + 1] * 50);
                                        _positionBufferImg[i + 2] = (byte)(_positionBuffer[i + 2] * 50);

                                        _textureBufferImg[i + 0] = (byte)(_textureBuffer[i + 0] * 255);
                                        _textureBufferImg[i + 1] = (byte)(_textureBuffer[i + 1] * 255);
                                        _textureBufferImg[i + 2] = (byte)(_textureBuffer[i + 2] * 255);

                                        //unsafe
                                        //{
                                        //    _zBufferPtr[iz] = (byte)(_zBuffer[x, y] * 50);

                                        //    _normalBufferPtr[i + 0] = (byte)(_normalBuffer[x, y].X * 255);
                                        //    _normalBufferPtr[i + 1] = (byte)(_normalBuffer[x, y].Y * 255);
                                        //    _normalBufferPtr[i + 2] = (byte)(_normalBuffer[x, y].Z * 255);

                                        //    _diffuseBufferPtr[i + 0] = (byte)(_diffuseBuffer[x, y].X * 255);
                                        //    _diffuseBufferPtr[i + 1] = (byte)(_diffuseBuffer[x, y].Y * 255);
                                        //    _diffuseBufferPtr[i + 2] = (byte)(_diffuseBuffer[x, y].Z * 255);

                                        //    _positionBufferPtr[i + 0] = (byte)(_positionBuffer[x, y].X * 50);
                                        //    _positionBufferPtr[i + 1] = (byte)(_positionBuffer[x, y].Y * 50);
                                        //    _positionBufferPtr[i + 2] = (byte)(_positionBuffer[x, y].Z * 50);

                                        //    _textureBufferPtr[i + 0] = (byte)(_textureBuffer[x, y].X * 255);
                                        //    _textureBufferPtr[i + 1] = (byte)(_textureBuffer[x, y].Y * 255);
                                        //    _textureBufferPtr[i + 2] = (byte)(_textureBuffer[x, y].Z * 255);
                                        //}
                                    }
                                }
                            }

                            u += uAdd;
                            v += vAdd;

                            ap.X++;
                        }
                    });
                }
            }

            if(App.Current?.Dispatcher == null) return;
            await App.Current?.Dispatcher.InvokeAsync(() =>
            {
                if (DrawBuffers)
                {
                    if (NormalBufferBitmap == null) return;
                    WriteToBitmap(NormalBufferBitmap, _normalBufferImg);
                    WriteToBitmap(DiffuseBufferBitmap, _diffuseBufferImg);
                    WriteToBitmap(PositionBufferBitmap, _positionBufferImg);
                    WriteToBitmap(TextureBufferBitmap, _textureBufferImg);
                    WriteToBitmap(ZBufferBitmap, _zBufferImg, 1);
                }
            });
        }

        private async void MergeBuffers()
        {
            _pixelBufferImg.Clear((byte)0);
            var enviromentLigth = new[] { _scene.EnviromentLight.X, _scene.EnviromentLight.Y, _scene.EnviromentLight.Z };

            //color, normal, p3D, tex, l, Id, r, r_dot_p3D
            var color = Gpu.Default.Allocate(_diffuseBuffer);
            var normal = Gpu.Default.Allocate(_normalBuffer);
            var p3D = Gpu.Default.Allocate(_positionBuffer);
            var tex = Gpu.Default.Allocate(_textureBuffer);
            var l = new float[_imgWidth * _imgHeight * 3];
            var Id = new float[_imgWidth * _imgHeight * 3];
            var r = new float[_imgWidth * _imgHeight * 3];

            var widht = _imgWidth;
            Gpu.Default.For(0, _imgHeight * _imgWidth, i =>
            {
                var k = i * 3;
                int y = i / widht;
                int x = i % widht;

                ColorPass(x, y, enviromentLigth, color, normal, p3D, tex, l, Id, r, k);

                ArrayMath.Clamp(color, 0, 255, k, 3);

                _pixelBufferImg[k + 0] = (byte)(color[k + 0]);
                _pixelBufferImg[k + 1] = (byte)(color[k + 1]);
                _pixelBufferImg[k + 2] = (byte)(color[k + 2]);
            });

            //Parallel.For(0, _imgHeight, y =>
            //Gpu.Default.For(0, _imgHeight, y =>
            //{
            //    int i = y * widht * 3;
            //    for (int x = 0; x < widht; x++, i += 3)
            //    {
            //        if (float.IsInfinity(_zBuffer[x, y])) continue;

            //        ColorPass(x, y, enviromentLigth, color, normal, p3D, tex, l, Id, r, i);

            //        ArrayMath.Clamp(color, 0, 255, i, 3);

            //        _pixelBufferImg[i + 0] = (byte)(color[i + 0]);
            //        _pixelBufferImg[i + 1] = (byte)(color[i + 1]);
            //        _pixelBufferImg[i + 2] = (byte)(color[i + 2]);
            //    }
            //});

            await App.Current?.Dispatcher.InvokeAsync(() =>
            {
                if (Bitmap == null) return;
                WriteToBitmap(Bitmap, _pixelBufferImg);
            });
        }

        private unsafe void ClearBuffer(byte* buffer, int size)
        {
            var ptr = buffer;
            for (int i = 0; i < size; i++)
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

        private void AddDirtyRect(WriteableBitmap bmp)
        {
            bmp.Lock();
            bmp.AddDirtyRect(new Int32Rect(0, 0, _imgWidth, _imgHeight));
            bmp.Unlock();
        }

        private void WriteToBitmap<T>(WriteableBitmap bmp, T[] buffer, int componets = 3)
        {
            bmp.Lock();
            bmp.WritePixels(new Int32Rect(0, 0, _imgWidth, _imgHeight), buffer, _imgWidth * componets, 0);
            bmp.Unlock();
        }

        private void PrePass(Triangle tria, Triangle tria3D, float u, float v, int i)
        {
            var wAndUv = (tria.Verts[0].W, tria.Verts[1].W, tria.Verts[2].W, u, v);
            var normal = Vector3.Normalize(VectorHelper.LerpHomogeneous(tria3D.ANormal, tria3D.BNormal, tria3D.CNormal, wAndUv).ToVector3());
            _normalBuffer[i + 0] = normal.X;
            _normalBuffer[i + 1] = normal.Y;
            _normalBuffer[i + 2] = normal.Z;
            var p3D = VectorHelper.LerpHomogeneous(tria3D.A, tria3D.B, tria3D.C, wAndUv).ToVector3();
            _positionBuffer[i + 0] = p3D.X;
            _positionBuffer[i + 1] = p3D.Y;
            _positionBuffer[i + 2] = p3D.Z;
            var uv = VectorHelper.LerpHomogeneous(tria.VertUvs[0], tria.VertUvs[1], tria.VertUvs[2], wAndUv);
            var matColor = tria.Material.GetColor(uv.X, uv.Y);
            var color = VectorHelper.LerpHomogeneous(tria.AColor, tria3D.BColor, tria3D.CColor, wAndUv).ToVector3();
            _diffuseBuffer[i + 0] = color.X;
            _diffuseBuffer[i + 1] = color.Y;
            _diffuseBuffer[i + 2] = color.Z;
            _textureBuffer[i + 0] = matColor.X;
            _textureBuffer[i + 1] = matColor.Y;
            _textureBuffer[i + 2] = matColor.Z;
        }

        private void ColorPass(int x, int y, float[] eniromentLight, float[] color, float[] normal, float[] p3D, float[] tex, float[] l, float[] Id, float[] r, int from)
        {
            ArrayMath.Sub(_lightPosArray, p3D, l, 0, from, from, 3);
            ArrayMath.Normalize(l, from, 3);

            var dotNl = ArrayMath.Dot(normal, l, from, from, 3);

            ArrayMath.Mul(color, Math.Max(0f, dotNl), Id, from, from, 3);

            ArrayMath.Mul(normal, dotNl * 2, r, from, from, 3);
            ArrayMath.Sub(r, l, r, from, from, from, 3);
            ArrayMath.Normalize(r, from, 3);
            ArrayMath.Normalize(p3D, from, 3);

            var rDotP3D = -ArrayMath.Dot(r, p3D, from, from, 3);

            var max = Math.Max(0, rDotP3D);
            var Is = DeviceFunction.Pow(max, 50);
            //var Is = 0;
            
            ArrayMath.Mul(eniromentLight, color, color, 0, from, from, 3);
            ArrayMath.Mul(tex, Id, tex, from, from, from, 3);

            ArrayMath.Add(color, tex, color, from, from, from, 3);

            ArrayMath.Add(color, Is, color, from, 3);
            ArrayMath.Mul(color, 255, color, from, from, 3);
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
