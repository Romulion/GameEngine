using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Toys
{
    public class ShaderUniformManager
    {
        public ShaderUniform[] uniforms;
        Material material;

        public ShaderUniformManager(ShaderUniform[] unis,Material mat)
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
            var uniformVariable = GetUniform(name);
            if (uniformVariable != null)
            {
                uniformVariable.SetValue(val);
                VisibilityCheck(name, uniformVariable);
            }


        }

        public void Modify(MaterialMorpher caller, string name, object val, ModifyType type)
        {
            var uniformVariable =  GetUniform(name);

            if (uniformVariable != null)
            {
                uniformVariable.AddModifier(caller, val, type);
                VisibilityCheck(name, uniformVariable);
            }
        }

        public void Apply()
        {
            foreach (var uniformVariable in uniforms)
                uniformVariable.Assign();
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

        //for deletion
        void VisibilityCheck(string name, ShaderUniform uniformVariable)
        {
            if (name == "diffuse_color")
            {
				if (((Vector4)uniformVariable.GetValue()).W < 0.01f)
                    material.RenderDirrectives.IsRendered = false;
                else
                    material.RenderDirrectives.IsRendered = true;
            }
        }

        public ShaderUniform[] CopyUniforms()
        {
            ShaderUniform[] unis = new ShaderUniform[uniforms.Length];
            for (int i = 0; i < uniforms.Length; i++)
                unis[i] = uniforms[i].Clone();

            return unis;
        }
    }
}
