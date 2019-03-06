using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Toys
{
    public class ShaderUniformManager
    {
        public ShaderUniform[] uniforms;
        IMaterial material;

        public ShaderUniformManager(ShaderUniform[] unis,IMaterial mat)
        {
            uniforms = unis;
            material = mat;
        }

        public bool Exists(string name)
        {
            var query = from v in uniforms
                        where v.Name == name
                        select v;

            return query.Count() >= 1;
        }

        public void Set(string name,object val)
        {
            var uni = GetUniform(name);
            if (uni != null)
            {
                uni.SetValue(val);
                VisibilityCheck(name, uni);
            }


        }

        public void Modify(MaterialMorpher caller, string name, object val, ModifyType type)
        {
            var uni =  GetUniform(name);

            if (uni != null)
            {
                uni.AddModifier(caller, val, type);
                VisibilityCheck(name, uni);
            }
        }

        ShaderUniform GetUniform(string name)
        {
            var query = from v in uniforms
                        where v.Name == name
                        select v;

            if (query.Count() >= 1)
                return query.First();

            return null;

        }

        void VisibilityCheck(string name, ShaderUniform uni)
        {
            if (name == "diffuse_color")
            {
				if (((Vector4)uni.GetValue()).W < 0.01f)
                    material.rndrDirrectives.render = false;
                else
                    material.rndrDirrectives.render = true;
            }
        }
    }
}
