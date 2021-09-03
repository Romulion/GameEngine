using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace Toys
{
    //Face Expressions viever for Pokemon Masters models
    class ScriptChangeExpression : ScriptingComponent
    {
        bool keyPressed;
        Material face;
        KeyboardState keyState;
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
            keyState = GLWindow.gLWindow.KeyboardState;

            if (keyState.IsKeyDown(Keys.E))
            {
                keyPressed = false;
            }

            if (keyState.IsKeyDown(Keys.E) && !keyPressed)
            {
                if (currentFace == 15)
                    currentFace = 0;
                else
                    currentFace++;
                trans.Xy = exprList[currentFace];
                face.UniformManager.Set("uv_translation", trans);
                keyPressed = true;
            }
        }
    }
}
