using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Toys;
using Toys.Engine.Modules;

namespace ModelViewer.Scripts
{
    internal class RenderIconScript : ScriptingComponent
    {
        /// <summary>
        /// [.ShellClassInfo]
        /// IconResource=pr236.ico,0
        /// [ViewState]
        /// Mode=
        /// Vid=
        /// FolderType=Generic
        /// </summary>
        
        bool isSaved = true;
        int size = 256;
        public SceneNode ModelNode;

        int count = 100;
        int index = 0;
        //string path = @"L:\Coding\GameEngine5.0\bin\Debug\net8.0-windows7.0\Assets\Models\EL-Pr223(Selnaestine)\セルネスティーヌ.pmx";
        public string FilePath = @"C:\ModelViever\Assets\Models\Michelle\ミシェル新制服ver3.pmx";
        string name = null;
        string[] files = new string[0];
        const int delayFramesBase = 0;
        int delayFrames = 0;
        void Awake()
        {
            //ModelNode = ResourcesManager.LoadAsset<ModelPrefab>(path).CreateNode();
            //var cam = CoreEngine.GetCamera.Node;
            //var trans = cam.GetTransform;
            //trans.Position = new Vector3(0f, 1.14f, 1f);

            //files = GetModels(@"C:\ModelViever\Assets\Models\");
        }

        void Update()
        {
            if (delayFrames > 0)
                delayFrames--;
            else if (index < files.Length)
            //if (count > 0)
            {
                if (ModelNode != null)
                    CleanMemory();

                ModelNode = ResourcesManager.LoadAsset<ModelPrefab>(files[index]).CreateNode();
                //ModelNode = ResourcesManager.LoadAsset<ModelPrefab>(path).CreateNode();
                var pm = ModelNode.GetComponent<PhysicsManager>();
                ModelNode.RemoveComponent(pm);
                //Console.WriteLine($"model loaded {count}");
                Console.WriteLine($"model {Path.GetFileName(files[index])} loaded");
                AdjustCameraPos(ModelNode);
                name = $"{Path.GetFileNameWithoutExtension(files[index])}.ico";
                Console.WriteLine(name);
                isSaved = true;
                index++;
                //count--;
                delayFrames = delayFrames;
            }
            // Console.WriteLine(CoreEngine.GetCamera.Node.GetTransform.Position);
        }

        void AdjustCameraPos(SceneNode node)
        {
            var cam = CoreEngine.GetCamera;
            var md = node.GetComponent<MeshDrawerRigged>();
            var bb = md.Mesh.BoundingBox;
            var size = bb.MaxModel - bb.MinModel;
            var heigth = size.Y * 1.02f;
            var fov = cam.Width / cam.Height * cam.FOV;
            var dist = heigth / 2 / MathF.Tan(Toys.MathHelper.ConvertGrad2Radians(fov / 2));
            cam.Node.GetTransform.Position = new Vector3(0, heigth * 0.9f /2, dist);
        }

        void PostRender()
        {
            var keyState = GLWindow.Keyboard;
           
            if (keyState.IsKeyDown(Keys.LeftControl) && keyState.IsKeyPressed(Keys.S))
            {
                ProcessFolderIcon();
                //SaveIcon($"test.ico");
            }
            else if (!isSaved && delayFrames > 0)
            {
                SaveIcon(name);
                isSaved = true;
            }

            /*
            if (keyState.IsKeyPressed(Keys.G))
            {
                CleanMemory();
            }
            */
            /*
            if (keyState.IsKeyPressed(Keys.R))
            {
                if (ModelNode)
                    CleanMemory();

                ModelNode = ResourcesManager.LoadAsset<ModelPrefab>(FilePath).CreateNode();
                CoreEngine.MainScene.AddNode2Root(ModelNode);
                AdjustCameraPos(ModelNode);
            }
            */

            if (keyState.IsKeyPressed(Keys.D))
            {
                ResourcesManager.DebugResourses();
            }
        }

        string[] GetModels(string dir)
        {
            var list = Directory.GetFiles(dir, "*.pmx", SearchOption.AllDirectories);
            return list;
        }

        void SaveIcon(string name)
        {
            using (var screen = ReadPixelsFromBuffer.GetScreenImage())
            {
                screen.RotateFlip(RotateFlipType.Rotate180FlipX);
                var screenIco = Resize2ICO(screen);

                var icon = IconFromImage(screenIco);
                //screenIco.Save("test1234.ico", System.Drawing.Imaging.ImageFormat.Bmp);
                using (var fs = new FileStream(name, FileMode.Create))
                {
                    icon.Save(fs);
                }
                screenIco.Dispose();

            }
        }

        Bitmap Resize2ICO(Bitmap original)
        {
            var imgSize = Math.Min(original.Width, original.Height);
            Rectangle canvas;
            if (imgSize == original.Width)
                canvas = new Rectangle(0, (original.Height - original.Width) / 2, imgSize, imgSize);
            else
                canvas = new Rectangle((original.Width - original.Height) / 2, 0, imgSize, imgSize);
            var cropped = original.Clone(canvas, original.PixelFormat);
            var bitmap = new Bitmap(cropped, size, size);
            /*
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(cropped, 0, 0, size, size);
            }
            */
            return bitmap;
        }

        //Code from https://stackoverflow.com/questions/21387391/how-to-convert-an-image-to-an-icon-without-losing-transparency/21389253#21389253
        public static Icon IconFromImage(System.Drawing.Image img)
        {
            var ms = new System.IO.MemoryStream();
            var bw = new System.IO.BinaryWriter(ms);
            // Header
            bw.Write((short)0);   // 0 : reserved
            bw.Write((short)1);   // 2 : 1=ico, 2=cur
            bw.Write((short)1);   // 4 : number of images
                                  // Image directory
            var w = img.Width;
            if (w >= 256) w = 0;
            bw.Write((byte)w);    // 0 : width of image
            var h = img.Height;
            if (h >= 256) h = 0;
            bw.Write((byte)h);    // 1 : height of image
            bw.Write((byte)0);    // 2 : number of colors in palette
            bw.Write((byte)0);    // 3 : reserved
            bw.Write((short)0);   // 4 : number of color planes
            bw.Write((short)0);   // 6 : bits per pixel
            var sizeHere = ms.Position;
            bw.Write((int)0);     // 8 : image size
            var start = (int)ms.Position + 4;
            bw.Write(start);      // 12: offset of image data
                                  // Image data
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            var imageSize = (int)ms.Position - start;
            ms.Seek(sizeHere, System.IO.SeekOrigin.Begin);
            bw.Write(imageSize);
            ms.Seek(0, System.IO.SeekOrigin.Begin);

            // And load it
            return new Icon(ms);
        }


        void CleanMemory()
        {
            if (!ModelNode)
                return;
            var regex = new Regex(@"toon\d{2}.bmp$");
            var md = ModelNode.GetComponent<MeshDrawerRigged>();
            foreach (var mat in md.Materials)
            {
                foreach (var type in Enum.GetValues(typeof(TextureType)))
                {
                    var texType = (TextureType)type;
                    var tex1 = mat.GetTexture(texType);
                    if (tex1)
                    {
                        var tex = tex1 as Texture2D;
                        if (tex)
                        {
                            if (!tex.Name.EndsWith("def") && !tex.Name.EndsWith("toon_blank.bmp") && !(texType == TextureType.Toon && regex.IsMatch(tex.Name)))
                                ResourcesManager.DeleteResource(tex);
                        }
                    }
                }
                Resource.Destroy(mat);
            }
            Resource.Destroy(md.Mesh);
            foreach (var comp in ModelNode.GetComponents())
                ModelNode.RemoveComponent(comp);
            
            ResourcesManager.DeleteResource(FilePath);
            CoreEngine.MainScene.RemoveNode(ModelNode);
            ModelNode = null;
        }

        void ProcessFolderIcon()
        {
            if (!ModelNode) return;

            var name = Path.GetFileNameWithoutExtension(FilePath) + ".ico";
            var path = Path.GetDirectoryName(FilePath);
            var pathIco = Path.Combine(path, name);
            SaveIcon(pathIco);
            var fi = new FolderIcon(path);
            fi.Icon = name;
            fi.SaveIni();
        }

        void OnDestroy()
        {
        }
    }
}

