using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Toys
{
    public class ShaderUniformVector3 : ShaderUniform
    {

        Dictionary<MaterialMorpher, UniformModifier> mods = new Dictionary<MaterialMorpher, UniformModifier>();
        string varName;
        Vector3 value = Vector3.Zero;



        public ShaderUniformVector3(string name, string group, Shader program, int id)
        {
            Name = name;
            Group = group;
			varId = id;
            Type = typeof(Vector3);
            if (group != "")
                varName = group + "." + name;
            else
                varName = name;

            this.program = program;
        }

        public override bool CheckCompability(object val)
        {
            return val is Vector3;
        }

        public override void Assign()
        {
            program.ApplyShader();
            program.SetUniform(value, varId);
        }

        public override void AddModifier(MaterialMorpher caller, object val, ModifyType mod)
        {
            if (!CheckCompability(val))
                return;

            if (mods.ContainsKey(caller) &&
                    (mod == ModifyType.Add && RetrieveValue(val) == Vector3.Zero ||
                    mod == ModifyType.Multiply && RetrieveValue(val) == Vector3.One))
                mods.Remove(caller);
            else
                mods[caller] = new UniformModifier(val, mod);
            CalculateFinal();
        }

        public override UniformModifier[] Getmods()
        {
            var query = from mod in mods
                        orderby mod.Value.t descending
                        select mod.Value;

            return query.ToArray();
        }

        public override object GetValue()
        {
            return value;
        }
        public override ShaderUniform Clone()
        {
            var shdrvar = new ShaderUniformVector3(Name, Group, program, varId);
            shdrvar.value = value;
            return shdrvar;
        }
        protected override void CalculateFinal()
        {
            value = RetrieveValue(defaultValue);
            foreach (UniformModifier mod in Getmods())
            {
                switch (mod.t)
                {
                    case ModifyType.Add:
                        value += RetrieveValue(mod.value);
                        break;
                    case ModifyType.Multiply:
                        value *= RetrieveValue(mod.value);
                        break;
                    default:
                        break;
                }
            }
            Assign();
        }

        Vector3 RetrieveValue(object obj)
        {
            return (Vector3)obj;
        }
    }
}