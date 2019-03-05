﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Toys
{
    public class ShaderUniformVector4 : ShaderUniform
    {

        protected Dictionary<MaterialMorpher, UniformModifier> mods = new Dictionary<MaterialMorpher, UniformModifier>();
        string varName;
        Vector4 value = Vector4.One;



        public ShaderUniformVector4(string name, string group, Shader program)
        {
            Name = name;
            Group = group;

            if (group != "")
                varName = group + "." + name;
            else
                varName = name;

            this.program = program;
        }

        public override bool CheckCompability(object val)
        {
            return val is Vector4;
        }

        public override void Assign()
        {
            program.ApplyShader();
            program.SetUniform(value, varName);
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
                    (mod == ModifyType.Add && RetrieveValue(val) == Vector4.Zero ||
                    mod == ModifyType.Multiply && RetrieveValue(val) == Vector4.One))
                mods.Remove(caller);
            else
                mods[caller] = new UniformModifier(val, mod);

            CalculateFinal();
        }

        public override  UniformModifier[] Getmods()
        {
            var query = from mod in mods
                        orderby mod.Value.t descending
                        select mod.Value;

            return query.ToArray();
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

        Vector4 RetrieveValue(object obj)
        {
            return (Vector4)obj;
        }
    }
}
