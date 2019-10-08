using System;
using System.Linq;
using OpenTK;

namespace Toys
{
     
    public class MaterialMorpher
    {

        Material material;
        public Vector4 DiffuseColor;
        public Vector3 AmbientColor;
        public Vector3 SpecularColor;
        public int mode;

        string diffuseName = "diffuse_color";
        string ambientName = "ambient_color";
        string specularName = "specular_color";
        //ShaderUniform diffuseUni;

        public MaterialMorpher(Material mat)
        {
            material = mat;
        }

        public void Perform(float degree)
        {
            if (material == null)
                return;

            if (mode == 0)
            {
                material.UniManager.Modify(this, diffuseName, (Vector4.One - (Vector4.One - DiffuseColor) * degree), ModifyType.Multiply);
                material.UniManager.Modify(this, ambientName, (Vector3.One - (Vector3.One - AmbientColor) * degree), ModifyType.Multiply);
                material.UniManager.Modify(this, specularName, (Vector3.One - (Vector3.One - SpecularColor) * degree), ModifyType.Multiply);
            }
            //diffuseUni.AddModifier(this,(Vector4.One + (Vector4.One - diffuse) * degree),ModifyType.Multiply);
            else if (mode == 1)
            {
                material.UniManager.Modify(this, diffuseName, DiffuseColor * degree, ModifyType.Add);
                material.UniManager.Modify(this, ambientName, AmbientColor * degree, ModifyType.Add);
                material.UniManager.Modify(this, specularName, SpecularColor * degree, ModifyType.Add);
            }
            // diffuseUni.AddModifier(this, diffuse * degree, ModifyType.Add);
        }
    }
}
