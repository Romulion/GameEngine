using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;

namespace Toys
{
	public class ShaderUniformMatrix4 : ShaderUniform
	{
		protected Dictionary<MaterialMorpher, UniformModifier> mods = new Dictionary<MaterialMorpher, UniformModifier>();
		string varName;
		Matrix4 value = Matrix4.Identity;

		public ShaderUniformMatrix4(string name, string group, Shader program, int id)
		{
			Name = name;
			Group = group;
			varId = id;
            Type = typeof(Matrix4);
			if (group != "")
				varName = group + "." + name;
			else
				varName = name;

			this.program = program;
		}

		public override bool CheckCompability(object val)
		{
			return val is Matrix4;
		}

		public override void Assign()
		{
			program.ApplyShader();
			program.SetUniform(value, varId);
		}

		public override object GetValue()
		{
			return value;
		}

		public override void AddModifier(MaterialMorpher caller, object val, ModifyType mod)
		{
			if (!CheckCompability(val))
				return;

			if (mods.ContainsKey(caller) &&
			    (mod == ModifyType.Multiply && RetrieveValue(val) == Matrix4.Identity))
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

        public override ShaderUniform Clone()
        {
            var shdrvar = new ShaderUniformMatrix4(Name, Group, program, varId);
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

		Matrix4 RetrieveValue(object obj)
		{
			return (Matrix4)obj;
		}
	}
}
