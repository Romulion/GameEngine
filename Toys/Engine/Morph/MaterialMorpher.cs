using System;
using System.Linq;
using OpenTK;

namespace Toys
{
     
    public class MaterialMorpher
    {

        Material Mat;
        public Vector4 diffuse;
        public int mode;

        string name = "diffuse_color";
        //ShaderUniform diffuseUni;

        public MaterialMorpher(Material mat)
        {
            Mat = mat;
        }

        public void Perform(float degree)
        {
            if (mode == 0)
                Mat.UniManager.Modify(this, name, (Vector4.One - (Vector4.One - diffuse) * degree), ModifyType.Multiply);
            //diffuseUni.AddModifier(this,(Vector4.One + (Vector4.One - diffuse) * degree),ModifyType.Multiply);
            else if (mode == 1)
                Mat.UniManager.Modify(this, name, diffuse * degree, ModifyType.Add);
            // diffuseUni.AddModifier(this, diffuse * degree, ModifyType.Add);
        }
    }
}
