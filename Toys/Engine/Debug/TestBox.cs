using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;

namespace Toys.Debug
{
    public class TestBox
    {
        public static MeshDrawer CreateBox(Vector3 size)
        {
            var mesh = CreateBoxMesh(size / 2);

            ShaderSettings ss = new ShaderSettings();
            RenderDirectives rd = new RenderDirectives();
            string path = "Toys.Resourses.shaders.";
            string vs = ShaderManager.ReadFromAssetStream(path + "UIElement.vsh");
            string fs = ShaderManager.ReadFromAssetStream(path + "UIElement.fsh");
            ss.TextureDiffuse = true;
            var material = new MaterialCustom(ss, rd, vs, fs);
            material.Name = "Texture";
            material.SetTexture(Texture2D.LoadEmpty(), TextureType.Diffuse);

            return new MeshDrawer(mesh);
        }

        static Mesh CreateBoxMesh(Vector3 dimensions)
        {
            var scale = Matrix3.CreateScale(dimensions);
            Vertex3D[] verts = new Vertex3D[]
            {
                new Vertex3D(new Vector3(1, 1, -1), new Vector2(0,0)),
                new Vertex3D(new Vector3(1, -1, -1), new Vector2(0,1)),
                new Vertex3D(new Vector3(1, 1, 1), new Vector2(1,1)),
                new Vertex3D(new Vector3(1, -1, 1), new Vector2(0,0)),
                new Vertex3D(new Vector3(-1, 1, -1), new Vector2(0,0)),
                new Vertex3D(new Vector3(-1, -1, -1), new Vector2(0,1)),
                new Vertex3D(new Vector3(-1, 1, 1), new Vector2(1,1)),
                new Vertex3D(new Vector3(-1, -1, 1), new Vector2(0,0)),
            };

            //resize
            for (int i = 0; i< 8; i++)
            {
                verts[i].Position *= scale;
            }

            var indexes = new int[] { 4, 2, 0, 2, 7, 3, 6, 5, 7, 1, 7, 5, 0, 3, 1, 4, 1, 5, 4, 6, 2, 2, 6, 7, 6, 4, 5, 1, 3, 7, 0, 2, 3, 4, 0, 1 };

            var mesh = new Mesh(verts, indexes);
            return mesh;
        }


    }
}
