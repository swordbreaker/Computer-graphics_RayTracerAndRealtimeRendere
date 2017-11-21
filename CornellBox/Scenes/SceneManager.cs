using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media;
using CornellBox.Models;
using CornellBox.Models.Objects;

namespace CornellBox.Scenes
{
    public static class SceneManager
    {
        public static readonly Dictionary<string, Scene> Scenes = new Dictionary<string, Scene>()
        {
            { "CornellBox", null},
            { "AlotOfSpheres", null},
            { "SkyBoxScene", null},
            { "AntiAliasingScene", null},
            { "DepthOfFielScene", null },
            { "CornelBoxScenePathCasting", null },
            { "MugScene", null }
        };

        public static HdrImage StoneTexture;
        public static HdrImage WoodTexture;
        public static HdrImage BrickTexture;
        public static HdrImage GravelTexture;
        public static HdrImage GraceProbeTextuere;

        public static Mesh MugMesh;
        //public static Mesh CupMesh;

        public static void LoadScenes()
        {
            StoneTexture = new HdrImage((Bitmap) Image.FromFile(@"Textures\stone.png"));
            WoodTexture = new HdrImage((Bitmap) Image.FromFile(@"Textures\wood1.jpg"));
            BrickTexture = new HdrImage((Bitmap) Image.FromFile(@"Textures\01.JPG"));
            GravelTexture = new HdrImage((Bitmap)Image.FromFile(@"Textures\03.JPG"));
            GraceProbeTextuere = new HdrImage(@"SkyMaps\grace_probe.float");
            MugMesh = new Mesh(@"Meshes\mug.obj", new Material(Colors.White, kreflection: 0f, kspecular: 20, kdiffuse: 1f));
            //CupMesh = new Mesh(@"Meshes\mug.obj", new Material(Colors.White, 0.1f));

            Scenes["CornellBox"] = CornellBoxScene.Scene;
            Scenes["AlotOfSpheres"] = AlotOfSpheres.Scene;
            Scenes["SkyBoxScene"] = SkyBoxScene.Scene;
            Scenes["AntiAliasingScene"] = AntiAliasingScene.Scene;
            Scenes["DepthOfFielScene"] = DepthOfFielScene.Scene;
            Scenes["CornelBoxScenePathCasting"] = CornelBoxScenePathCasting.Scene;
            Scenes["MugScene"] = MugScene.Scene;
            Scenes["MugScene"].UseAccelerationStructures = false;
        }

    }
}
