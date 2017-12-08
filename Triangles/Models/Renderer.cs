using System;
using System.Linq;
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

        private readonly float[] _zBuffer;
        //private readonly unsafe float* _zBuffer;

        private readonly float[] _normalBuffer;
        private readonly float[] _diffuseBuffer;
        private readonly float[] _textureBuffer;
        private readonly float[] _positionBuffer;

        private readonly byte[] _zBufferImg;
        [GpuParam] private readonly byte[] _pixelBufferImg;
        private readonly byte[] _normalBufferImg;
        private readonly byte[] _diffuseBufferImg;
        private readonly byte[] _textureBufferImg;
        private readonly byte[] _positionBufferImg;

        [GpuParam] private readonly float[] _l;
        [GpuParam] private readonly float[] _Id;
        [GpuParam] private readonly float[] _r;

        [GpuParam] private readonly float[] _enviromentLight;

        public delegate void UpdateDelegate(Renderer renderer);

        private readonly UpdateDelegate _updateAction;
        private readonly Matrix4x4 _projection;
        private volatile bool _isActive = true;

        private Vector3 _lightPos;
        [GpuParam] private float[] _lightPosArray;

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
        public Matrix4x4 ViewModel = Matrix4x4.Identity;
        public bool IsActive => _isActive;
        public bool DrawBuffers { get; set; }


        private readonly int _triangleCount;

        private readonly float[] _triaVerts;
        private readonly float[] _triaNormals;
        private readonly float[] _triaColors;
        private readonly float[] _triaUvs;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Renderer"/> class.
        /// </summary>
        /// <param name="scene">The scene.</param>
        /// <param name="imgWidht">The img widht.</param>
        /// <param name="imgHeight">Height of the img.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="updateAction">The update action.</param>
        public Renderer(IScene scene, int imgWidht, int imgHeight, Settings settings,
            UpdateDelegate updateAction = null)
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

            _lightPosArray = new[] {_lightPos.X, _lightPos.Y, _lightPos.Z};

            _projection = new Matrix4x4(
                _imgWidth, 0, (float) imgWidht / 2, 0,
                0, _imgWidth, (float) _imgHeight / 2, 0,
                0, 0, 0, 0,
                0, 0, 1, 0);

            Bitmap = new WriteableBitmap(imgWidht, imgHeight, 8, 8, PixelFormats.Bgr24, null);
            ZBufferBitmap = new WriteableBitmap(imgWidht, imgHeight, 8, 8, PixelFormats.Gray8, null);
            NormalBufferBitmap = new WriteableBitmap(imgWidht, imgHeight, 8, 8, PixelFormats.Rgb24, null);
            DiffuseBufferBitmap = new WriteableBitmap(imgWidht, imgHeight, 8, 8, PixelFormats.Rgb24, null);
            TextureBufferBitmap = new WriteableBitmap(imgWidht, imgHeight, 8, 8, PixelFormats.Rgb24, null);
            PositionBufferBitmap = new WriteableBitmap(imgWidht, imgHeight, 8, 8, PixelFormats.Rgb24, null);

            _pixelBufferImg = new byte[_imgWidth * _imgHeight * 3];
            _zBufferImg = new byte[_imgWidth * _imgHeight];
            _normalBufferImg = new byte[_imgWidth * _imgHeight * 3];
            _diffuseBufferImg = new byte[_imgWidth * _imgHeight * 3];
            _textureBufferImg = new byte[_imgWidth * _imgHeight * 3];
            _positionBufferImg = new byte[_imgWidth * _imgHeight * 3];

            _l = new float[_imgWidth * _imgHeight * 3];
            _Id = new float[_imgWidth * _imgHeight * 3];
            _r = new float[_imgWidth * _imgHeight * 3];

            _enviromentLight = new[] {_scene.EnviromentLight.X, _scene.EnviromentLight.Y, _scene.EnviromentLight.Z};

            _triangleCount = _scene.Meshes.Sum(mesh => mesh.TriangleCount());
            _triaVerts = new float[_triangleCount * 4];
            _triaNormals = new float[_triangleCount * 3];
            _triaColors = new float[_triangleCount * 3];
            _triaUvs = new float[_triangleCount * 2];
        }

        public void StartRendering()
        {
            _isActive = true;

            Task.Run(() =>
            {
                while (_isActive)
                {
                    FpsHelper.BeginOfFrame();

                    Rendering();
                    _updateAction?.Invoke(this);

                    if (!Settings.StaticLight)
                    {
                        _lightPos = Vector3.Transform(Vector3.Zero,
                            Matrix4x4.CreateTranslation(_scene.LightPos) * ViewModel);
                        _lightPosArray = new[] {_lightPos.X, _lightPos.Y, _lightPos.Z};
                    }
                    else
                    {
                        _lightPos = Vector3.Transform(Vector3.Zero,
                            Matrix4x4.CreateTranslation(_scene.LightPos) *
                            Matrix4x4.CreateTranslation(ViewModel.Translation));
                        _lightPosArray = new[] {_lightPos.X, _lightPos.Y, _lightPos.Z};
                    }

                    FpsHelper.EndOfFrame();
                }
            });
        }

        public void StopRendering() => _isActive = false;

        private Task _bufferTask = Task.Run(() => { });
        private Task _mergeTask = Task.Run(() => { });

        private void Rendering()
        {
            _bufferTask = Task.Run(() => CalculateBuffer());
            Task.WaitAll(_bufferTask, _mergeTask);

            if (Settings.UseGpu)
            {
                var color = Gpu.Default.Allocate(_diffuseBuffer);
                var normal = Gpu.Default.Allocate(_normalBuffer);
                var p3D = Gpu.Default.Allocate(_positionBuffer);
                var tex = Gpu.Default.Allocate(_textureBuffer);

                _mergeTask = Task.Run(() => MergeBuffersGPU(color, normal, p3D, tex));
            }
            else
            {
                var color = _diffuseBuffer.Copy();
                var normal = _normalBuffer.Copy();
                var p3D = _positionBuffer.Copy();
                var tex = _textureBuffer.Copy();
                var zBuffer = _zBuffer.Copy();

                _mergeTask = Task.Run(() => MergeBuffersCPU(color, normal, p3D, tex, zBuffer));
            }
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
                _zBufferImg.Clear((byte) 0);
                _normalBufferImg.Clear((byte) 0);
                _diffuseBufferImg.Clear((byte) 0);
                _positionBufferImg.Clear((byte) 0);
                _textureBufferImg.Clear((byte) 0);
            }

            foreach (var mesh in _scene.Meshes)
            {
                foreach (var triangle in mesh.Triangles(Settings.ZPlane, Matrix4x4.Identity, ViewModel,
                    FpsHelper.DeltaTime))
                {
                    var trianlge2D = triangle.TransformNormalized(_projection);
                    if (trianlge2D.IsClockWise) continue;

                    var tMin = trianlge2D.Min;
                    var tMax = trianlge2D.Max;
                    (var xMin, var yMin) =
                        ((int) tMin.X.Clamp(0, _imgWidth - 1), (int) tMin.Y.Clamp(0, _imgHeight - 1));
                    (var xMax, var yMax) =
                        ((int) tMax.X.Clamp(0, _imgWidth - 1), (int) tMax.Y.Clamp(0, _imgHeight - 1));

                    Parallel.For(yMin, yMax + 1, y =>
                    {
                        var i = y * _imgWidth * 3 + xMin * 3;
                        var iz = y * _imgWidth + xMin;

                        var AB = trianlge2D.AB;
                        var AC = trianlge2D.AC;

                        var det = 1 / (AB.X * AC.Y - AC.X * AB.Y);

                        var ap = new Vector3(xMin, y, 0) - trianlge2D.A;

                        var u = (AC.Y * ap.X + -AC.X * ap.Y) * det;
                        var v = (-AB.Y * ap.X + AB.X * ap.Y) * det;

                        var uAdd = AC.Y * det;
                        var vAdd = -AB.Y * det;

                        for (int x = xMin; x < xMax + 1; x++, i += 3, iz++)
                        {
                            if (u >= 0 && v >= 0 && (u + v) < 1)
                            {
                                var vs = trianlge2D.Verts;
                                var p = VectorHelper.Lerp(vs[0], vs[1], vs[2], u, v);

                                if (_zBuffer[iz] > p.W)
                                {
                                    _zBuffer[iz] = p.W;
                                    PrePass(trianlge2D, triangle, u, v, i);
                                    if (DrawBuffers)
                                    {
                                        _zBufferImg[iz] = (byte) (_zBuffer[iz] * 50);

                                        _normalBufferImg[i + 0] = (byte) (_normalBuffer[i + 0] * 255);
                                        _normalBufferImg[i + 1] = (byte) (_normalBuffer[i + 1] * 255);
                                        _normalBufferImg[i + 2] = (byte) (_normalBuffer[i + 2] * 255);

                                        _diffuseBufferImg[i + 0] = (byte) (_diffuseBuffer[i + 0] * 255);
                                        _diffuseBufferImg[i + 1] = (byte) (_diffuseBuffer[i + 1] * 255);
                                        _diffuseBufferImg[i + 2] = (byte) (_diffuseBuffer[i + 2] * 255);

                                        _positionBufferImg[i + 0] = (byte) (_positionBuffer[i + 0] * 50);
                                        _positionBufferImg[i + 1] = (byte) (_positionBuffer[i + 1] * 50);
                                        _positionBufferImg[i + 2] = (byte) (_positionBuffer[i + 2] * 50);

                                        _textureBufferImg[i + 0] = (byte) (_textureBuffer[i + 0] * 255);
                                        _textureBufferImg[i + 1] = (byte) (_textureBuffer[i + 1] * 255);
                                        _textureBufferImg[i + 2] = (byte) (_textureBuffer[i + 2] * 255);
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

            if (!DrawBuffers) return;
            if (App.Current?.Dispatcher == null) return;
            await App.Current?.Dispatcher.InvokeAsync(() =>
            {
                if (NormalBufferBitmap == null) return;
                WriteToBitmap(NormalBufferBitmap, _normalBufferImg);
                WriteToBitmap(DiffuseBufferBitmap, _diffuseBufferImg);
                WriteToBitmap(PositionBufferBitmap, _positionBufferImg);
                WriteToBitmap(TextureBufferBitmap, _textureBufferImg);
                WriteToBitmap(ZBufferBitmap, _zBufferImg, 1);
            });
        }

        private async void MergeBuffersCPU(float[] color, float[] normal, float[] p3D, float[] tex, float[] zBuffer)
        {
            _pixelBufferImg.Clear((byte) 0);

            var specular = Settings.Specular;
            Parallel.For(0, _imgHeight, y =>
            {
                int i = y * _imgWidth * 3;
                int k = y * _imgWidth;

                for (int x = 0; x < _imgWidth; x++, i += 3, k++)
                {
                    if (float.IsInfinity(zBuffer[k])) continue;

                    ColorPass(_enviromentLight, color, normal, p3D, tex, specular, i);

                    ArrayMath.Clamp(color, 0, 255, i, 3);

                    _pixelBufferImg[i + 0] = (byte) (color[i + 0]);
                    _pixelBufferImg[i + 1] = (byte) (color[i + 1]);
                    _pixelBufferImg[i + 2] = (byte) (color[i + 2]);
                }
            });

            await App.Current?.Dispatcher.InvokeAsync(() =>
            {
                if (Bitmap == null) return;
                WriteToBitmap(Bitmap, _pixelBufferImg);
            });
        }

        private async void MergeBuffersGPU(float[] color, float[] normal, float[] p3D, float[] tex)
        {
            _pixelBufferImg.Clear((byte) 0);


            var specular = Settings.Specular;
            Gpu.Default.For(0, _imgHeight * _imgWidth, i =>
            {
                var k = i * 3;

                ColorPass(_enviromentLight, color, normal, p3D, tex, specular, k);

                ArrayMath.Clamp(color, 0, 255, k, 3);

                _pixelBufferImg[k + 0] = (byte) (color[k + 0]);
                _pixelBufferImg[k + 1] = (byte) (color[k + 1]);
                _pixelBufferImg[k + 2] = (byte) (color[k + 2]);
            });

            Gpu.Free(color);
            Gpu.Free(normal);
            Gpu.Free(p3D);
            Gpu.Free(tex);

            await App.Current?.Dispatcher.InvokeAsync(() =>
            {
                if (Bitmap == null) return;
                WriteToBitmap(Bitmap, _pixelBufferImg);
            });
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
            var normal = Vector3.Normalize(VectorHelper
                .LerpHomogeneous(tria3D.VertNormals[0], tria3D.VertNormals[1], tria3D.VertNormals[2], wAndUv)
                .ToVector3());
            _normalBuffer[i + 0] = normal.X;
            _normalBuffer[i + 1] = normal.Y;
            _normalBuffer[i + 2] = normal.Z;
            var p3D = VectorHelper.LerpHomogeneous(tria3D.A, tria3D.B, tria3D.C, wAndUv).ToVector3();
            _positionBuffer[i + 0] = p3D.X;
            _positionBuffer[i + 1] = p3D.Y;
            _positionBuffer[i + 2] = p3D.Z;
            var uv = VectorHelper.LerpHomogeneous(tria.VertUvs[0], tria.VertUvs[1], tria.VertUvs[2], wAndUv);
            var matColor = tria.Material.GetColor(uv.X, uv.Y, Settings.BilinearFiltering);
            var color = VectorHelper
                .LerpHomogeneous(tria.VertColors[0], tria3D.VertColors[1], tria3D.VertColors[2], wAndUv).ToVector3();
            _diffuseBuffer[i + 0] = color.X;
            _diffuseBuffer[i + 1] = color.Y;
            _diffuseBuffer[i + 2] = color.Z;
            _textureBuffer[i + 0] = matColor.X;
            _textureBuffer[i + 1] = matColor.Y;
            _textureBuffer[i + 2] = matColor.Z;
        }

        private void ColorPass(float[] eniromentLight, float[] color, float[] normal, float[] p3D, float[] tex,
            bool specular, int from)
        {
            ArrayMath.Sub(_lightPosArray, p3D, _l, 0, from, from, 3);
            ArrayMath.Normalize(_l, from, 3);

            var dotNl = ArrayMath.Dot(normal, _l, from, from, 3);

            ArrayMath.Mul(color, Math.Max(0f, dotNl), _Id, from, from, 3);

            ArrayMath.Mul(normal, dotNl * 2, _r, from, from, 3);
            ArrayMath.Sub(_r, _l, _r, from, from, from, 3);
            ArrayMath.Normalize(_r, from, 3);
            ArrayMath.Normalize(p3D, from, 3);

            var rDotP3D = -ArrayMath.Dot(_r, p3D, from, from, 3);

            var Is = 0f;
            if (specular)
            {
                var max = Math.Max(0, rDotP3D);
                Is = DeviceFunction.Pow(max, 50);
            }

            ArrayMath.Mul(eniromentLight, color, color, 0, from, from, 3);
            ArrayMath.Mul(tex, _Id, tex, from, from, from, 3);

            ArrayMath.Add(color, tex, color, from, from, from, 3);

            ArrayMath.Add(color, Is, color, from, 3);
            ArrayMath.Mul(color, 255, color, from, from, 3);
        }

        //private Vector3 CalcColor(Triangle tria, Triangle tria3d, float u, float v, Vector4 p)
        //{
        //    var wAndUv = (tria.Verts[0].W, tria.Verts[1].W, tria.Verts[2].W, u, v);

        //    var color = VectorHelper.LerpHomogeneous(tria.AColor, tria3d.BColor, tria3d.CColor, wAndUv).ToVector3();
        //    var uv = VectorHelper.LerpHomogeneous(tria.VertUvs[0], tria.VertUvs[1], tria.VertUvs[2], wAndUv);
        //    var matColor = tria.Material.GetColor(uv.X, uv.Y);
        //    var normal = Vector3.Normalize(VectorHelper.LerpHomogeneous(tria3d.ANormal, tria3d.BNormal, tria3d.CNormal, wAndUv).ToVector3());
        //    var p3D = VectorHelper.LerpHomogeneous(tria3d.A, tria3d.B, tria3d.C, wAndUv).ToVector3();
        //    var l = Vector3.Normalize(_lightPos - p3D);
        //    var dotNl = Vector3.Dot(normal, l);
        //    var Id = Math.Max(0f, dotNl) * color;

        //    var r = 2 * dotNl * normal - l;
        //    r = Vector3.Normalize(r);

        //    var Is = (float)Math.Pow(Math.Max(0, -Vector3.Dot(r, Vector3.Normalize(p3D))), 50) * new Vector3(1f, 1f, 1f);

        //    return (_scene.EnviromentLight * color) + matColor * Id + Is;
        //}
    }
}