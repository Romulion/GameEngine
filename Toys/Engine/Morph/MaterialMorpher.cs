using System;
using System.Linq;
using OpenTK;

namespace Toys
{
     
    public class MaterialMorpher
    {

        Material Mat;
        public Vector4 diffuse;
        public Vector3 ambient;
        public Vector3 specular;
        public int mode;

        string diffuseName = "diffuse_color";
        string ambientName = "ambient_color";
        string specularName = "specular_color";
        //ShaderUniform diffuseUni;

        public MaterialMorpher(Material mat)
        {
            Mat = mat;
        }

        public void Perform(float degree)
        {
            if (Mat == null)
                return;

            if (mode == 0)
            {
                Mat.UniManager.Modify(this, diffuseName, (Vector4.One - (Vector4.One - diffuse) * degree), ModifyType.Multiply);
                Mat.UniManager.Modify(this, ambientName, (Vector3.One - (Vector3.One - ambient) * degree), ModifyType.Multiply);
                Mat.UniManager.Modify(this, specularName, (Vector3.One - (Vector3.One - specular) * degree), ModifyType.Multiply);
            }
            //diffuseUni.AddModifier(this,(Vector4.One + (Vector4.One - diffuse) * degree),ModifyType.Multiply);
            else if (mode == 1)
            {
                Mat.UniManager.Modify(this, diffuseName, diffuse * degree, ModifyType.Add);
                Mat.UniManager.Modify(this, ambientName, ambient * degree, ModifyType.Add);
                Mat.UniManager.Modify(this, specularName, specular * degree, ModifyType.Add);
            }
            // diffuseUni.AddModifier(this, diffuse * degree, ModifyType.Add);
        }
    }
}
