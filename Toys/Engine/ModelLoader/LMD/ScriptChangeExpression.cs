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
            var mds = Node.GetComponents<MeshDrawerRigged>();

            foreach (var md in mds)
                if (md.Materials[0].Name.Contains("face"))
                    face = md.Materials[0];
        }
        void Update()
        {
            //sefldestruct
            if (face == null)
                Node.RemoveComponent(this);
            
            Vector4 trans = Vector4.Zero;
            keyState = GLWindow.gLWindow.KeyboardState;
            
            if (keyState.IsKeyPressed(Keys.E))
            {
                if (currentFace == 15)
                    currentFace = 0;
                else
                    currentFace++;
                trans.Xy = exprList[currentFace];
                face.UniformManager.Set("uv_translation", trans);
            }
        }

        internal override Component Clone()
        {
            return new ScriptChangeExpression();
        }
    }
}
