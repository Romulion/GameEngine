using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK;

namespace Toys
{
    //Face Expressions viever for Pokemon Masters models
    class ScriptChangeExpression : ScriptingComponent
    {
        bool keyPressed;
        Material face;
        KeyboardState ks;
        int currentFace = 0;
        Vector2[] exprList =
        {
            new Vector2(0,0),
            new Vector2(0.25f,0),
            new Vector2(0.5f,0),
            new Vector2(0.75f,0),
            new Vector2(0,0.25f),
            new Vector2(0.25f,0.25f),
            new Vector2(0.5f,0.25f),
            new Vector2(0.75f,0.25f),
            new Vector2(0,0.5f),
            new Vector2(0.25f,0.5f),
            new Vector2(0.5f,0.5f),
            new Vector2(0.75f,0.5f),
            new Vector2(0,0.75f),
            new Vector2(0.25f,0.75f),
            new Vector2(0.5f,0.75f),
            new Vector2(0.75f,0.75f),
        };
        void Awake()
        {

            face = null;
            Component[] mds = Node.GetComponents(typeof(MeshDrawer));

            foreach (var md in mds)
                if (((MeshDrawer)md).Materials[0].Name.Contains("face"))
                    face = ((MeshDrawer)md).Materials[0];

        }
        void Update()
        {
            if (face == null)
                return;

            Vector4 trans = Vector4.Zero;
            ks = Keyboard.GetState();
            if (ks[Key.Number0])
            {
                trans.X = 0f;
                trans.Y = 0f;
                face.UniManager.Set("uv_translation", trans);
            }
            else if (ks[Key.Number1])
            {
                trans.X = 0.25f;
                trans.Y = 0f;
                face.UniManager.Set("uv_translation", trans);
            }
            else if (ks[Key.Number2])
            {
                trans.X = 0.5f;
                trans.Y = 0f;
                face.UniManager.Set("uv_translation", trans);
            }
            else if (ks[Key.Number3])
            {
                trans.X = 0.75f;
                trans.Y = 0f;
                face.UniManager.Set("uv_translation", trans);
            }
            else if (ks[Key.Number4])
            {
                trans.X = 0f;
                trans.Y = 0.25f;
                face.UniManager.Set("uv_translation", trans);
            }
            else if (ks[Key.Number5])
            {
                trans.X = 0.25f;
                trans.Y = 0.25f;
                face.UniManager.Set("uv_translation", trans);
            }
            else if (ks[Key.Number6])
            {
                trans.X = 0.75f;
                trans.Y = 0.25f;
                face.UniManager.Set("uv_translation", trans);
            }
            else if (ks[Key.Number7])
            {
                trans.X = 0.0f;
                trans.Y = 0.5f;
                face.UniManager.Set("uv_translation", trans);
            }
            else if (ks[Key.Number8])
            {
                trans.X = 0.25f;
                trans.Y = 0.5f;
                face.UniManager.Set("uv_translation", trans);
            }
            else if (ks[Key.A])
            {
                trans.X = 0.5f;
                trans.Y = 0.5f;
                face.UniManager.Set("uv_translation", trans);
            }

            if (ks.IsKeyUp(Key.E))
            {
                keyPressed = false;
            }

            if (ks.IsKeyDown(Key.E) && !keyPressed)
            {
                if (currentFace == 15)
                    currentFace = 0;
                else
                    currentFace++;
                trans.Xy = exprList[currentFace];
                face.UniManager.Set("uv_translation", trans);
                keyPressed = true;
            }
        }
    }
}
